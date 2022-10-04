using Lidgren.Network;

namespace MM.Multiplayer;

public interface INetPlayer
{
    public NetConnection Connection { get; }
}
