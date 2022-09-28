using Lidgren.Network;
using MM.Logging;

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

    public event Action<NetConnectionStatus> OnStatusChanged;

    public GameClient(NetPeerConfiguration config) : base(config)
    {
        Instance = this;
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
                break;

            case NetIncomingMessageType.Data:
                Info($"Server says {msg.ReadString()}");
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
        if (Status != NetPeerStatus.NotRunning)
            throw new Exception("The client should be shut down before disposing.");

        if (Instance == this)
            Instance = null;
    }
}