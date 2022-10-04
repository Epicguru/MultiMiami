using BenchmarkDotNet.Attributes;
using MM.Multiplayer;
using MM.Multiplayer.Internal;

namespace MultiMiami;

public class InstancingSpeedTest
{
    private ObjectTracker tracker;
    private ushort id;

    [GlobalSetup]
    public void Setup()
    {
        tracker = new ObjectTracker();

        id  = tracker.RegisterType<TestClass>();
        tracker.RegisterType<OtherClass>();
    }

    [Benchmark]
    public TestClass TrackerGeneric() => tracker.CreateInstance<TestClass>();

    [Benchmark]
    public TestClass TrackerWithID() => tracker.CreateInstance<TestClass>(id);

    [Benchmark]
    public TestClass RegularConstructor() => new TestClass();

    public class TestClass : NetObject
    {

    }

    public class OtherClass : NetObject
    {
    }
}