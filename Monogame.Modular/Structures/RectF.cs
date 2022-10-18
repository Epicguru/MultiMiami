using Microsoft.Xna.Framework;

namespace MM.Core.Structures;

public struct RectF : IEquatable<RectF>
{
    public Vector2 Position
    {
        get => new Vector2(X, Y);
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }
    public Vector2 Size
    {
        get => new Vector2(Width, Height);
        set
        {
            Width = value.X;
            Height = value.Y;
        }
    }

    public float X, Y, Width, Height;

    public RectF(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public RectF(Vector2 position, Vector2 size)
    {
        X = position.X;
        Y = position.Y;
        Width = size.X;
        Height = size.Y;
    }

    public RectF(in Rectangle rect)
    {
        X = rect.X;
        Y = rect.Y;
        Width = rect.Width;
        Height = rect.Height;
    }

    public override string ToString() => $"[X: {X:F2}, Y: {Y:F2}, W: {Width:F2}, H: {Height:F2}]";

    public bool Equals(RectF other) => X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;

    public override bool Equals(object obj)
    {
        return obj is RectF other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Width, Height);
    }

    public static bool operator ==(in RectF self, in RectF other) => self.X == other.X && self.Y == other.Y && self.Width == other.Width && self.Height == other.Height;

    public static bool operator !=(in RectF self, in RectF other) => self.X != other.X || self.Y != other.Y || self.Width != other.Width || self.Height != other.Height;
    
    public static implicit operator RectF(in Rectangle rect) => new RectF(rect);
}