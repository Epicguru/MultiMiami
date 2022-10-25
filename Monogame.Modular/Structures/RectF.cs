using Microsoft.Xna.Framework;

namespace MM.Core.Structures;

/// <summary>
/// Represents a rectangle, with the origin (<see cref="Position"/>) in the top-left, extending down and to the right.
/// </summary>
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

    public readonly float Right => X + Width;
    public readonly float Bottom => Y + Height;

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

    /// <summary>
    /// Expands the rectangle to include all of the <paramref name="points"/>.
    /// </summary>
    /// <param name="points">The enumeration of points to encompass.</param>
    /// <param name="minimal">If true, this rect will be reset to encompass the <paramref name="points"/> in the smallest rect possible. If false, this rect will simply expand to encompass the points.</param>
    public void Encompass(IEnumerable<Vector2> points, bool minimal = false)
    {
        if (points == null)
            return;

        bool first = true;
        foreach (var p in points)
        {
            if (first && minimal)
            {
                X = p.X;
                Y = p.Y;
                Width = 0;
                Height = 0;
                first = false;
                continue;
            }

            X = Math.Min(X, p.X);
            Y = Math.Min(Y, p.Y);
            Width = Math.Max(Width, p.X - X);
            Height = Math.Max(Height, p.Y - Y);
        }
    }

    /// <summary>
    /// Returns a version of this rect with the width and height increased by <paramref name="w"/> and <paramref name="h"/> respectively.
    /// The expansion occurs around the center of this rect.
    /// </summary>
    public readonly RectF ExpandedBy(float w, float h) => new RectF(X - w * 0.5f, Y - h * 0.5f, Width + w, Height + h);

    public readonly bool Overlaps(in RectF other)
    {
        if (X >= other.Right || Y >= other.Bottom || Right <= other.X || Bottom <= other.Y)
            return false;
        return true;
    }

    public override string ToString() => $"[X: {X:F2}, Y: {Y:F2}, W: {Width:F2}, H: {Height:F2}]";

    public readonly bool Equals(RectF other) => X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;

    public readonly override bool Equals(object obj)
    {
        return obj is RectF other && Equals(other);
    }

    public readonly override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Width, Height);
    }

    public static bool operator ==(in RectF self, in RectF other) => self.X == other.X && self.Y == other.Y && self.Width == other.Width && self.Height == other.Height;

    public static bool operator !=(in RectF self, in RectF other) => self.X != other.X || self.Y != other.Y || self.Width != other.Width || self.Height != other.Height;
    
    public static implicit operator RectF(in Rectangle rect) => new RectF(rect);
}