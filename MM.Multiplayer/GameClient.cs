using Lidgren.Network;
using MM.Logging;
using MM.Multiplayer.Internal;

namespace MM.Multiplayer;

public class GameClient : NetClient, IDisposable
{
    /// <summary>
    /// Is the client connected to a server?
    /// </summary>
    public static bool IsConnected => Instance?.ConnectionStatus == NetConnectionStatus.Connected;
    /// <summary>
    /// Is the client running? Also see <see cref="IsConnected"/>.
    /// </summary>
    public static bool IsRunning => Instance?.Status == NetPeerStatus.Running;
    /// <summary>
    /// The current client instance. It is assigned when creating a client, and remove when disposing that client.
    /// </summary>
    public static GameClient Instance { get; protected set; }

    public readonly bool IsHost;
    public readonly ObjectTracker ObjectTracker;
    public event Action<NetConnectionStatus> OnStatusChanged;

    public GameClient(NetPeerConfiguration config, bool isHost) : base(config)
    {
        Instance = this;
        IsHost = isHost;
        if (!isHost)
        {
            ObjectTracker = new ObjectTracker
            {
                SendMessage = msg => SendMessage(msg, NetDeliveryMethod.UnreliableSequenced, 0)
            };
        }
    }

    protected void Error(string msg, Exception e = null)
    {
        Log.Error($"[Client] {msg}", e);
    }

    protected void Warn(string msg)
    {
        Log.Warn($"[Client] {msg}");
    }

    protected void Info(string msg)
    {
        Log.Info($"[Client] {msg}");
    }

    protected void Trace(string msg)
    {
        Log.Trace($"[Client] {msg}");
    }

    public void Tick()
    {
        ObjectTracker?.Tick();

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
        Log.Trace($"Got {msg.MessageType} of len {msg.LengthBytes}");
        for (int i = 0; i < msg.LengthBytes; i++)
        {
            Log.Trace(msg.PeekDataBuffer()[i].ToString());
        }

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
                break;

            case NetIncomingMessageType.Data:
                byte type = msg.ReadByte();
                switch (type)
                {
                    // Spawn object.
                    case 1:
                        if (IsHost)
                        {
                            Error("Should not receive spawn messages because we are host!");
                            break;
                        }

                        Trace("Reading spawn");
                        ObjectTracker.OnReceiveSpawnMessage(msg);
                        break;

                    // Despawn object.
                    case 2:
                        if (IsHost)
                        {
                            Error("Should not receive despawn messages because we are host!");
                            break;
                        }

                        Warn("Despawn not handled.");
                        break;

                    // Sync vars.
                    case 3:
                        if (IsHost)
                        {
                            Error("Should not receive sync var messages because we are host!");
                            break;
                        }
                        ObjectTracker.OnReceiveSyncVarMessage(msg);
                        break;

                    case 255:
                        Info($"Received debug msg: {msg.ReadString()}");
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, $"Unhandled data ID on client: {type}");
                }

                if (msg.Position != msg.LengthBits)
                    Warn($"Data message was not read fully: read {msg.Position} / {msg.LengthBits} bits.");
                break;

            case NetIncomingMessageType.ConnectionLatencyUpdated:
                // Ignore.
                break;

            default:
                throw new ArgumentOutOfRangeException(msg.MessageType.ToString());
        }
    }

    public virtual void Dispose()
    {
        Shutdown("Client: Dispose()");

        if (Instance == this)
            Instance = null;
    }
}