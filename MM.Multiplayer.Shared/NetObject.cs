using Lidgren.Network;

namespace MM.Multiplayer.Shared;

public abstract partial class NetObject : SyncVarOwner
{
    [field: SyncVar(initOnly: true)]
    public ushort NetID { get; }

    protected NetObject(ushort netID)
    {
        NetID = netID;
    }

    // GENERATED
    public override void WriteInitialNetData(NetOutgoingMessage msg)
    {

    }
}
