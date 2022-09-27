using Lidgren.Network;

namespace MM.Multiplayer;

public static class Extensions
{
    public static void Write<T>(this NetOutgoingMessage msg, in T obj) where T : INetSerializable, new()
    {
        msg.Write(obj != null);
        obj?.NetWrite(msg);
    }

    public static void Read<T>(this NetIncomingMessage msg, ref T obj) where T : INetSerializable, new()
    {
        bool isNull = msg.ReadBoolean();
        if (isNull)
        {
            obj = default;
        }
        else
        {
            obj ??= new T();
            obj.NetRead(msg);
        }
    }
}
