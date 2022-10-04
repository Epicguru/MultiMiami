using Lidgren.Network;

namespace MM.Multiplayer.Tests;

public class ServerStandalone
{
    [Fact]
    public void CannotBindMultipleToSamePort()
    {
        var random = new Random();
        var config = new NetPeerConfiguration("TESTS")
        {
            Port = random.Next(5000, 10000)
        };

        var serverA = new GameServer(config, Utils.AlwaysAccept);
        var serverB = new GameServer(config, Utils.AlwaysAccept);

        serverA.Start();
        Assert.Equal(NetPeerStatus.Running, serverA.Status);

        Assert.ThrowsAny<Exception>(serverB.Start);

        serverA.Shutdown("bye");
        serverB.Shutdown("bye");
    }

    [Fact]
    public void CanBindToDifferentPorts()
    {
        var random = new Random();

        int portA = random.Next(5000, 10000);
        int portB;
        do
        {
            portB = random.Next(5000, 10000);
        } while (portA == portB);

        var config = new NetPeerConfiguration("TESTS")
        {
            Port = portA
        };
        var config2 = new NetPeerConfiguration("TESTS")
        {
            Port = portB
        };

        var serverA = new GameServer(config, Utils.AlwaysAccept);
        var serverB = new GameServer(config2, Utils.AlwaysAccept);

        serverA.Start();
        Assert.Equal(NetPeerStatus.Running, serverA.Status);

        serverB.Start();
        Assert.Equal(NetPeerStatus.Running, serverB.Status);

        serverA.Shutdown("bye");
        serverB.Shutdown("bye");
    }
}