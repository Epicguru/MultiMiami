using System.Numerics;
using System.Reflection;

namespace MM.Multiplayer;

public partial class Prefixes_NETGEN
{
    private static Dictionary<Type, MethodInfo> delegates = new Dictionary<Type, MethodInfo>();

    [ClientRPC]
    public void Test(Vector2 sex)
    {
    }

    [ClientRPC]
    private void Test2(int f)
    {
    }

    [ClientRPC]
    internal void Test3()
    {
    }

    [ClientRPC]
    void Test4()
    {
    }
}