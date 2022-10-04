using Lidgren.Network;

namespace MM.Multiplayer.Tests;

public class ConnectionTests
{
    [Fact]
    public void ClientConnectsAndDisconnects()
    {
        var serverConfig = new NetPeerConfiguration("TESTS")
        {
            Port = new Random().Next(5000, 10000)
        };

        var server = new GameServer(serverConfig, Utils.AlwaysAccept);
        server.Start();

        Assert.Equal(NetPeerStatus.Running, server.Status);
        Assert.Equal(0, server.ConnectionsCount);

        bool connectedEventRaised = false;

        var client = new GameClient(new NetPeerConfiguration("TESTS"), true);
        client.OnStatusChanged += status =>
        {
            if (status == NetConnectionStatus.Connected)
                connectedEventRaised = true;
        };
        client.Start();
        client.Connect("localhost", serverConfig.Port);

        Assert.Equal(NetPeerStatus.Running, client.Status);

        Utils.AssertWithTimeout(1000, () =>
        {
            server.Tick();
            client.Tick();
            return client.ConnectionStatus == NetConnectionStatus.Connected;
        }, "Client failed to connect within 5 seconds.");

        Assert.True(connectedEventRaised, "Client connected but the connected event was not raised.");
        Assert.Equal(1, server.ConnectionsCount);

        Utils.AssertWithTimeout(1000, () =>
        {
            server.Tick();
            client.Tick();
            return server.Players.Count == 1;
        }, "Server did not create/register client player object...");

        client.Disconnect("bye");

        Utils.AssertWithTimeout(1000, () =>
        {
            server.Tick();
            return server.ConnectionsCount == 0 && server.Players.Count == 0;
        }, "Client disconnected but remained on server.");

        server.Shutdown("bye all");
    }
}
