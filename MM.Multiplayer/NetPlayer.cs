using Lidgren.Network;

namespace MM.Multiplayer;

/// <summary>
/// Represents a player connected to the local server.
/// </summary>
public abstract class NetPlayer
{
    public readonly NetConnection Connection;

    protected NetPlayer(NetConnection connection)
    {
        Connection = connection;
    }
}
