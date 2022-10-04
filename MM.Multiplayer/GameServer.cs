using Lidgren.Network;
using MM.Logging;
using MM.Multiplayer.Internal;

namespace MM.Multiplayer;

public class GameServer : NetServer, IDisposable
{
    public static GameServer Instance { get; private set; }
    public static bool IsRunning => Instance != null; // Not really correct, but fast.

    public event Action<NetConnectionStatus> OnStatusChanged;

    public IReadOnlyList<INetPlayer> Players => ConnectedPlayers;
    public List<NetConnection> ConnectionsWithoutHost { get; } = new List<NetConnection>();

    public readonly ObjectTracker ObjectTracker = new ObjectTracker();

    internal int PendingCount => PendingNewPlayers.Count;

    protected readonly List<INetPlayer> ConnectedPlayers = new List<INetPlayer>();
    protected readonly List<INetPlayer> PendingNewPlayers = new List<INetPlayer>();
    protected readonly Func<NetIncomingMessage, INetPlayer> OnClientAttemptingToConnect;

    public GameServer(NetPeerConfiguration config, Func<NetIncomingMessage, INetPlayer> clientCreator) : base(ModConfig(config))
    {
        Instance = this;
        OnClientAttemptingToConnect = clientCreator;
        ObjectTracker.CreateMessage = CreateMessage;
        ObjectTracker.Recycle = Recycle;
        ObjectTracker.SendMessage = msg =>
        {
            if (Connections.Count > 0)
            {
                SendMessage(msg, Connections, NetDeliveryMethod.ReliableSequenced, 0);
            }
            else
            {
                Recycle(msg);
            }
        };
        ObjectTracker.SendExceptHost = ObjectTracker.SendMessage;
        //ObjectTracker.SendExceptHost = msg =>
        //{
        //    if (ConnectionsWithoutHost.Count > 0)
        //    {
        //        SendMessage(msg, ConnectionsWithoutHost, NetDeliveryMethod.ReliableSequenced, 0);
        //    }
        //    else
        //    {
        //        Recycle(msg);
        //    }
        //};
        ObjectTracker.ShouldSendSyncVars = true;
    }

    private static NetPeerConfiguration ModConfig(NetPeerConfiguration config)
    {
        config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
        return config;
    }

    protected void Error(string msg, Exception e = null)
    {
        Log.Error($"[Server] {msg}", e);
    }

    protected void Warn(string msg)
    {
        Log.Warn($"[Server] {msg}");
    }

    protected void Info(string msg)
    {
        Log.Info($"[Server] {msg}");
    }

    protected void Trace(string msg)
    {
        Log.Trace($"[Server] {msg}");
    }

    public bool Spawn(NetObject obj) => ObjectTracker.Spawn(obj);

    public void Tick()
    {
        ObjectTracker.Tick();

        while (ReadMessage(out var msg))
        {
            try
            {
                HandleMessage(msg);
            }
            catch (Exception e)
            {
                Error($"Exception handling message of type '{msg.MessageType}'", e);
            }
            finally
            {
                Recycle(msg);
            }
        }
    }

    protected virtual void HandleMessage(NetIncomingMessage msg)
    {
        switch (msg.MessageType)
        {
            case NetIncomingMessageType.Error:
                Error(msg.ReadString(out var errorMsg) ? errorMsg : "<Unknown net error>");
                break;

            case NetIncomingMessageType.VerboseDebugMessage:
                Trace(msg.ReadString());
                break;
            case NetIncomingMessageType.DebugMessage:
                Trace(msg.ReadString());
                break;
            case NetIncomingMessageType.WarningMessage:
                Warn(msg.ReadString());
                break;
            case NetIncomingMessageType.ErrorMessage:
                Error(msg.ReadString());
                break;

            case NetIncomingMessageType.StatusChanged:
                var newStatus = (NetConnectionStatus)msg.ReadByte();
                Trace($"{msg.SenderEndPoint} is now {newStatus}");
                OnStatusChanged?.Invoke(newStatus);

                if (newStatus == NetConnectionStatus.Connected)
                {
                    // Remove from pending and put into list of connected players.
                    var pending = TryGetPendingNewPlayer(msg.SenderConnection, true);
                    if (pending == null)
                        throw new InvalidOperationException("Connection was not approved.");
                    ConnectedPlayers.Add(pending);

                    if (!GameClient.IsRunning || GameClient.Instance.UniqueIdentifier != msg.SenderConnection.RemoteUniqueIdentifier)
                        ConnectionsWithoutHost.Add(msg.SenderConnection);
                }
                else if (newStatus == NetConnectionStatus.Disconnected)
                {
                    // Remove from pending or from player list.
                    var pending = TryGetPendingNewPlayer(msg.SenderConnection, true);
                    if (pending == null)
                        ConnectedPlayers.Remove(ConnectedPlayers.First(p => p.Connection == msg.SenderConnection));

                    if (ConnectionsWithoutHost.Contains(msg.SenderConnection))
                        ConnectionsWithoutHost.Remove(msg.SenderConnection);
                }

                break;

            case NetIncomingMessageType.ConnectionApproval:
                Trace("Incoming connection attempt...");
                var newPlayer = OnClientAttemptingToConnect(msg);
                if (newPlayer != null)
                {
                    PendingNewPlayers.Add(newPlayer);
                    Trace("Connection was approved!");
                }
                else
                {
                    Trace("Connection was rejected");
                }
                break;

            case NetIncomingMessageType.Data:
                byte type = msg.ReadByte();
                switch (type)
                {
                    // Server RPCs.
                    case 4:
                        ushort netID = msg.ReadUInt16();
                        var obj = ObjectTracker.TryGetObject(netID);

                        if (obj == null)
                            return;

                        byte methodID = msg.ReadByte();
                        obj.HandleServerRPC(msg, methodID);

                        break;

                    case 255:
                        Info($"Received debug msg: {msg.ReadString()}");
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, $"Unhandled data ID on server: {type}");
                }
                break;

            case NetIncomingMessageType.ConnectionLatencyUpdated:
                // Ignore.
                break;

            default:
                throw new ArgumentOutOfRangeException(msg.MessageType.ToString());
        }
    }

    protected INetPlayer TryGetPendingNewPlayer(NetConnection connection, bool removeFromPending)
    {
        for (int i = 0; i < PendingNewPlayers.Count; i++)
        {
            var pending = PendingNewPlayers[i];
            if (pending.Connection == connection)
            {
                if (removeFromPending)
                    PendingNewPlayers.RemoveAt(i);
                return pending;
            }
        }
        return null;
    }

    public void Dispose()
    {
        Shutdown("Server: Dispose()");
        Instance = null;
    }
}