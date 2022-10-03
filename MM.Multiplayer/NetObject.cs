namespace MM.Multiplayer;

public abstract partial class NetObject : SyncVarOwner
{
    public ushort NetID => netID;

    [SyncVar(initOnly: true)]
    private ushort netID;

    protected NetObject(ushort netID)
    {
        this.netID = netID;
    }
}
