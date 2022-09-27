﻿using Lidgren.Network;
using MM.Logging;

namespace MM.Multiplayer;

public class GameServer<T> : NetServer where T : NetPlayer
{
    public delegate void OnClientConnecting(NetIncomingMessage msg, out T newPlayer);

    public event Action<NetConnectionStatus> OnStatusChanged;

    public IReadOnlyList<T> Players => ConnectedPlayers;

    internal int PendingCount => PendingNewPlayers.Count;

    protected readonly List<T> ConnectedPlayers = new List<T>();
    protected readonly List<T> PendingNewPlayers = new List<T>();
    protected readonly OnClientConnecting OnClientAttemptingToConnect;

    public GameServer(NetPeerConfiguration config, OnClientConnecting clientCreator) : base(ModConfig(config))
    {
        OnClientAttemptingToConnect = clientCreator;
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

                if (newStatus == NetConnectionStatus.Connected)
                {
                    // Remove from pending and put into list of connected players.
                    var pending = TryGetPendingNewPlayer(msg.SenderConnection, true);
                    if (pending == null)
                        throw new InvalidOperationException("Connection was not approved.");
                    ConnectedPlayers.Add(pending);
                }
                else if (newStatus == NetConnectionStatus.Disconnected)
                {
                    // Remove from pending or from player list.
                    var pending = TryGetPendingNewPlayer(msg.SenderConnection, true);
                    if (pending == null)
                        ConnectedPlayers.Remove(ConnectedPlayers.First(p => p.Connection == msg.SenderConnection));
                }

                break;

            case NetIncomingMessageType.ConnectionApproval:
                Trace("Incoming connection attempt...");
                OnClientAttemptingToConnect(msg, out var newPlayer);
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
                Info($"{msg.SenderEndPoint} says {msg.ReadString()}");
                break;

            case NetIncomingMessageType.ConnectionLatencyUpdated:
                // Ignore.
                break;

            default:
                throw new ArgumentOutOfRangeException(msg.MessageType.ToString());
        }
    }

    protected T TryGetPendingNewPlayer(NetConnection connection, bool removeFromPending)
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
}