using Microsoft.Xna.Framework;

namespace MM.Multiplayer.Tests;

public class PatchingTests
{
    [Fact]
    public void TestServerRPC()
    {
        using var scope = Utils.StartLocalHost(false, out var client, out var server);

        var serverChild = new Child
        {
            SomeNumber = 567
        };
        server.ObjectTracker.Spawn(serverChild);

        Assert.True(serverChild.IsSpawned);
        Assert.Equal(1, server.ObjectTracker.TrackedObjectCount);

        Utils.AssertWithTimeout(100, () => client.ObjectTracker.TrackedObjectCount == 1, "Client did not receive server object.");

        var clientChild = client.ObjectTracker.GetAllTrackedObjects().First() as Child;
        Assert.NotNull(clientChild);
        Assert.Equal(serverChild.NetID, clientChild.NetID);
        Assert.Equal(serverChild.SomeNumber, clientChild.SomeNumber);

        // Should instantly call method.
        serverChild.MessageToServer(5, 6, "Hello, world!", new Vector3(1, 2, 3));
        Assert.Equal(5, serverChild.A);
        Assert.Equal(6, serverChild.B);
        Assert.Equal("Hello, world!", serverChild.Txt);
        Assert.Equal(new Vector3(1, 2, 3), serverChild.Vector);
    }
}
