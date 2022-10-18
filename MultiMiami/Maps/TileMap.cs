using Microsoft.Xna.Framework.Graphics;

namespace MultiMiami.Maps;

public class TileMap : IDisposable
{
    private static TileContainer emptyTile;

    public const int TILE_SIZE = 32;

    public IReadOnlyList<TileChunk> AllChunks => chunks;

    public readonly int WidthInTiles;
    public readonly int HeightInTiles;
    public readonly int ChunkSize;
    public readonly int WidthInChunks, HeightInChunks;

    private readonly TileChunk[] chunks;
    private readonly TileContainer[] tiles;

    public TileMap(int chunkSize, int widthInChunks, int heightInChunks)
    {
        ChunkSize = chunkSize;
        WidthInChunks = widthInChunks;
        HeightInChunks = heightInChunks;
        WidthInTiles = widthInChunks * chunkSize;
        HeightInTiles = heightInChunks * chunkSize;

        tiles = new TileContainer[WidthInTiles * HeightInTiles];

        chunks = new TileChunk[widthInChunks * heightInChunks];

        for (int x = 0; x < widthInChunks; x++)
        {
            for (int y = 0; y < heightInChunks; y++)
            {
                chunks[GetChunkIndex(x, y)] = new TileChunk(this, x, y);
            }
        }
    }

    public int GetChunkIndex(int x, int y) => x * WidthInChunks + y;

    public int GetTileIndex(int x, int y) => x * WidthInTiles + HeightInTiles;

    public bool IsInBounds(int x, int y) => x >= 0 && x < WidthInTiles && y >= 0 && y < HeightInTiles;

    public ref TileContainer GetTile(int x, int y)
    {
        if (IsInBounds(x, y))
            return ref tiles[GetTileIndex(x, y)];
        return ref emptyTile;
    }

    public ref TileContainer GetTileUnsafe(int x, int y) => ref tiles[GetTileIndex(x, y)];

    public void Draw(SpriteBatch spr)
    {
        foreach (var c in chunks)
        {
            c.Draw(spr);
        }
    }

    public void Dispose()
    {
        foreach (var c in chunks)
            c.Dispose();
    }
}