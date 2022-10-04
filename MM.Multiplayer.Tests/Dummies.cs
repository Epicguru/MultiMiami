using Lidgren.Network;

namespace MM.Multiplayer.Tests;

internal sealed class DummyPlayer : NetPlayer
{
    public string Name { get; set; }

    public DummyPlayer(NetConnection connection, string name) : base(connection)
    {
        Name = name;
    }
}

internal partial class DummyObj : NetObject
{
    public override int DefaultSyncVarInterval => 0;

    [SyncVar]
    public int SomeNumber;

    [SyncVar(initOnly: true)]
    public float InitOnly;
}
