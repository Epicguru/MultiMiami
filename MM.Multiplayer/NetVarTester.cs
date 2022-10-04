using MM.Logging;

namespace MM.Multiplayer;

public partial class NetVarTester : NetObject
{
    [SyncVar]
    public int IntNetVar;

    [SyncVar(30, nameof(FloatChanged))]
    public float FloatNetVar;

    [SyncVar(5)]
    public string MySyncString;

    [ServerRPC]
    public void Test()
    {
        
    }

    private void FloatChanged(float newValue)
    {
        Log.Info($"Float changed from {FloatNetVar} to {newValue}");
        FloatNetVar = newValue;
    }
}

public partial class Sub : NetVarTester
{
    [SyncVar(callbackMethodName: nameof(ByteCallback))]
    public byte MyByte = 123;

    [SyncVar(initOnly: true)]
    public double SomeDouble = 123.4;

    [SyncVar]
    public ushort HitPoints = 100;

    [SyncVar]
    private uint works;

    [SyncVar]
    private long doesNotWork;

    [SyncVar]
    private double staticDoesNotWork;

    private void ByteCallback(byte newValue)
    {
        MyByte = newValue;
    }
}
