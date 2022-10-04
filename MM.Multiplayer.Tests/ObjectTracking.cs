using Lidgren.Network;
using MM.Multiplayer.Internal;

namespace MM.Multiplayer.Tests;

public class ObjectTracking
{
    [Fact]
    public void RegisterAndSpawnTypes()
    {
        var tracker = new ObjectTracker();

        var id = tracker.RegisterType<DummyObj>();
        Assert.NotEqual(0, id);

        var instance = tracker.CreateInstance<DummyObj>();
        Assert.NotNull(instance);
        Assert.False(instance.IsSpawned, "Instance should not be spawned.");
        Assert.Equal(0, instance.NetID);

        instance = tracker.CreateInstance<DummyObj>(id);
        Assert.NotNull(instance);
        Assert.False(instance.IsSpawned, "Instance should not be spawned.");
        Assert.Equal(0, instance.NetID);

        instance = tracker.CreateInstance<DummyObj>(5);            
        Assert.Null(instance);
    }

    [Fact]
    public void ObjectDataSync()
    {
        using var scope = Utils.StartLocalHost(out var client, out var server);

        Assert.Equal(0, client.ObjectTracker.TrackedObjectCount);
        Assert.Equal(0, server.ObjectTracker.TrackedObjectCount);

        var obj = new DummyObj
        {
            SomeNumber = 123,
            InitOnly = 666.6f
        };

        // Spawn on server.
        Assert.True(server.ObjectTracker.Spawn(obj));

        // Check on server.
        Assert.True(obj.IsSpawned);
        Assert.NotEqual(0, obj.NetID);
        Assert.Equal(1, server.ObjectTracker.TrackedObjectCount);
        Assert.Single(server.ObjectTracker.GetAllTrackedObjects());

        // Wait for the client to get it.
        Utils.AssertWithTimeout(100, () => client.ObjectTracker.TrackedObjectCount == 1, "Client did not spawn the new object.");

        Assert.Single(client.ObjectTracker.GetAllTrackedObjects());

        var clientObj = client.ObjectTracker.GetAllTrackedObjects().First() as DummyObj;

        Assert.NotNull(clientObj);
        Assert.Equal(obj.NetID, clientObj.NetID);
        Assert.Equal(obj.SomeNumber, clientObj.SomeNumber);
        Assert.Equal(obj.InitOnly, clientObj.InitOnly);

        obj.SomeNumber = 878;
        obj.InitOnly = 999;
        Utils.AssertWithTimeout(100, () => clientObj.SomeNumber == 878, "Client did not receive updated syncvar.");

        Assert.NotEqual(obj.InitOnly, clientObj.InitOnly);
    }
}
