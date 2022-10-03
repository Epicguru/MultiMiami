// Class: MM.Multiplayer.NetObject
// Client RPC Count: 0
// Sync Var Count: 1
// Debug output:

namespace MM.Multiplayer;

public abstract partial class NetObject
{
    // Class body...
    public override void WriteInitialNetData(Lidgren.Network.NetOutgoingMessage msg)
    {
        msg.Write(netID);

        base.WriteInitialNetData(msg);
    }

    public override void ReadInitialNetData(Lidgren.Network.NetIncomingMessage msg)
    {
        netID = msg.ReadUInt16();

        base.ReadInitialNetData(msg);
    }
}
