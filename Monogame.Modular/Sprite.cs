using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MM.Core;

/// <summary>
/// Represents a region of a <see cref="Texture2D"/>.
/// </summary>
public class Sprite
{
    public int Width => Region.Width;
    public int Height => Region.Height;

    /// <summary>
    /// The name of the sprite. May be null.
    /// </summary>
    public string Name;

    /// <summary>
    /// The <see cref="Texture2D"/> that this sprite is part of.
    /// May be null.
    /// </summary>
    public Texture2D Texture;

    /// <summary>
    /// The region, measured in pixels, of this sprite within the texture.
    /// </summary>
    public Rectangle Region;

    /// <summary>
    /// The origin about which this sprite is rendered and rotated,
    /// where (0, 0) is the top-left corner.
    /// Defaults to the center (0.5, 0.5).
    /// </summary>
    public Vector2 OriginNormalized = new Vector2(0.5f, 0.5f);

    private Sprite() { }

    public Sprite(Texture2D texture) : this(texture, texture?.Bounds ?? default)
    { }

    public Sprite(Texture2D texture, Rectangle region) : this(texture, region, texture?.Name)
    { }

    public Sprite(Texture2D texture, Rectangle region, string name)
    {
        Name = name;
        Texture = texture;
        Region = region;
    }

    /// <summary>
    /// Creates a clone of this sprite.
    /// </summary>
    public Sprite Clone()
    {
        return new Sprite
        {
            Name = Name,
            Texture = Texture,
            Region = Region,
            OriginNormalized = OriginNormalized
        };
    }

    public override string ToString() => $"[Sprite '{Name}' {Region}]";
}
