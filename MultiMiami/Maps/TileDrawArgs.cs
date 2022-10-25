using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MultiMiami.Maps;

public readonly ref struct TileDrawArgs
{
    /// <summary>
    /// The spritebatch to draw with.
    /// </summary>
    public SpriteBatch Spritebatch { get;init; }

    /// <summary>
    /// The tile's draw position on the map.
    /// </summary>
    public Vector2 DrawPosition { get; init; }

    public int TileX { get; init; }

    public int TileY { get; init; }

    /// <summary>
    /// The map being drawn.
    /// </summary>
    public TileMap Map { get; init; }

    /// <summary>
    /// The layer of the tile being drawn.
    /// </summary>
    public TileLayer Layer { get; init; }

    /// <summary>
    /// The tile being rendered.
    /// </summary>
    public readonly ref TileContainer Tile;

    public TileDrawArgs(ref TileContainer tile)
    {
        Tile = ref tile;
    }
}
