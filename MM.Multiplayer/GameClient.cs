using Lidgren.Network;
using MM.Logging;

namespace MM.Multiplayer;

public class GameClient : NetClient
{
    public event Action<NetConnectionStatus> OnStatusChanged;

    public GameClient(NetPeerConfiguration config) : base(config)
    {
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
}