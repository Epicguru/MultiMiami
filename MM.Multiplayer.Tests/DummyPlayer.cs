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
