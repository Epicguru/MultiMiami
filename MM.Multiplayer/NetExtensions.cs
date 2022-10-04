using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace MM.Multiplayer;

public static class NetExtensions
{
    #region Vectors
    public static void Write(this NetOutgoingMessage msg, in Vector2 vector2)
    {
        msg.Write(vector2.X);
        msg.Write(vector2.Y);
    }

    public static Vector2 ReadVector2(this NetIncomingMessage msg)
        => new Vector2(msg.ReadSingle(), msg.ReadSingle());

    public static void Write(this NetOutgoingMessage msg, in Vector3 vector3)
    {
        msg.Write(vector3.X);
        msg.Write(vector3.Y);
        msg.Write(vector3.Z);
    }

    public static Vector3 ReadVector3(this NetIncomingMessage msg)
        => new Vector3(msg.ReadSingle(), msg.ReadSingle(), msg.ReadSingle());

    public static void Write(this NetOutgoingMessage msg, in Vector4 vector4)
    {
        msg.Write(vector4.X);
        msg.Write(vector4.Y);
        msg.Write(vector4.Z);
        msg.Write(vector4.W);
    }

    public static Vector4 ReadVector4(this NetIncomingMessage msg)
        => new Vector4(msg.ReadSingle(), msg.ReadSingle(), msg.ReadSingle(), msg.ReadSingle());
    #endregion

    public static void Write(this NetOutgoingMessage msg, Color color)
        => msg.Write(color.PackedValue);

    public static Color ReadColor(this NetIncomingMessage msg)
        => new Color(msg.ReadUInt32());
}
