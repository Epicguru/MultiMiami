using Lidgren.Network;
using Microsoft.Xna.Framework;
using System.Net;

namespace MM.Multiplayer.Tests;

internal sealed class DummyPlayer : INetPlayer
{
    public NetConnection Connection { get; }
    public string Name { get; set; }

    public DummyPlayer(NetConnection connection, string name)
    {
        Connection = connection;
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

    [SyncVar]
    public IPEndPoint Endpoint;

    public int A { get; private set; }
    public byte B { get; private set; }
    public string Txt { get; private set; }
    public Vector3 Vector { get; private set; }

    [ServerRPC]
    public void MessageToServer(int a, byte b, string txt, Vector3 vector)
    {
        A = a;
        B = b;
        Txt = txt;
        Vector = vector;
    }
}

internal partial class Child : DummyObj
{
    [ServerRPC]
    public void SomeOtherServerRPC(float f, Vector2 position, int i = 123)
    {
        
    }
}
