using Lidgren.Network;

namespace MM.Multiplayer;

/// <summary>
/// Classes and structures implementing this interface
/// can be written to and read from network messages. 
/// </summary>
public interface INetSerializable
{
    void NetWrite(NetOutgoingMessage msg);
    void NetRead(NetIncomingMessage msg);
}
