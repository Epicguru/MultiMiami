using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MM.Core;
using MM.Core.Structures;
using MM.Define;
using MM.Logging;
using MultiMiami.Defs;
using MultiMiami.Utility;
using System.Diagnostics;

namespace MultiMiami.Maps;

public class TileChunk : IDisposable
{
    private static readonly Queue<SpriteBatch> spriteBatchPool = new Queue<SpriteBatch>(32);

    private static SpriteBatch GetSpritebatch()
    {
        lock (spriteBatchPool)
        {
            if (spriteBatchPool.TryDequeue(out var spr))
                return spr;
            return new SpriteBatch(Core.GD, 1024 * 4); // Important - capacity must be large enough to hold all drawable in chunk.
        }
    }

    private static void ReturnSpritebatch(SpriteBatch spr)
    {
        lock (spriteBatchPool)
        {
            Debug.Assert(!spriteBatchPool.Contains(spr));
            spriteBatchPool.Enqueue(spr);
        }
    }

    public int SizeInPixels => Map.ChunkSize * TileMap.TILE_SIZE;
    public State CurrentState { get; private set; } = State.Unloaded;
    public Vector2 Origin => new Vector2(X * Map.ChunkSize, Y * Map.ChunkSize);
    public Vector2 CenterPosition => Origin + new Vector2(Map.ChunkSize * 0.5f, Map.ChunkSize * 0.5f);

    public readonly TileMap Map;
    public readonly int X, Y;
    public RenderTarget2D Texture { get; private set; }

    private Sprite sprite;

    public TileChunk(TileMap map, int x, int y)
    {
        Map = map;
        X = x;
        Y = y;
    }

    public void Load()
    {
        if (CurrentState != State.Unloaded)
            return;

        CurrentState = State.Loading;
        var t = Task.Run(() =>
        {
            Texture = new RenderTarget2D(Core.GD, SizeInPixels, SizeInPixels, true, SurfaceFormat.Bgra5551, DepthFormat.None, 0, RenderTargetUsage.PlatformContents);
            RenderToTarget();
        });
    }

    private void RenderToTarget()
    {
        var spr = GetSpritebatch();

        try
        {
            var rand = new Random();

            spr.Begin(new SpriteBatchArgs
            {
                BlendState = BlendState.NonPremultiplied,
                SamplerState = SamplerState.PointClamp
            });

            for (int x = X * Map.ChunkSize; x < (X + 1) * Map.ChunkSize; x++)
            {
                for (int y = Y * Map.ChunkSize; y < (Y + 1) * Map.ChunkSize; y++)
                {
                    ref var tile = ref Map.GetTileUnsafe(x, y);

                    var sprite = tile.Tile?.GetSpriteForTile(tile, x, y, Map);
                    if (sprite != null)
                        spr.Draw(sprite, new Vector2(x, y), Vector2.One, Color.White);
                }
            }

            if (CurrentState != State.Loading)
                return;

            Core.RunOnMainThread(() =>
            {
                if (CurrentState != State.Loading)
                {
                    spr.Dispose();
                    return;
                }

                try
                {
                    // Render
                    var oldTarget = Core.GD.GetRenderTargets().FirstOrDefault().RenderTarget as RenderTarget2D;
                    Core.GD.SetRenderTarget(Texture);
                    Core.GD.Clear(Color.Bisque);
                    spr.End();
                    Core.GD.SetRenderTarget(oldTarget);

                    // Finalize.
                    sprite = new Sprite(Texture)
                    {
                        OriginNormalized = Vector2.Zero
                    };
                    CurrentState = State.Loaded;
                    ReturnSpritebatch(spr);

                }
                catch (Exception e)
                {
                    Log.Error("Exception blitting chunk texture:", e);
                    spr.Dispose();
                }
            });
        }
        catch (Exception e)
        {
            ReturnSpritebatch(spr);
            Log.Error("Exception rendering chunk texture:", e);
        }
    }

    public void Unload()
    {
        if (CurrentState == State.Unloaded)
            return;

        if (CurrentState == State.Loading)
        {
            // TODO cancel task.
        }
        
        if (Texture != null)
        {
            if (!Texture.IsDisposed)
                Texture.Dispose();
            Texture = null;
        }

        CurrentState = State.Unloaded;
    }

    public void Draw(SpriteBatch spr)
    {
        var origin = Origin - new Vector2(0.5f, 0.5f);
        var size = new Vector2(Map.ChunkSize, Map.ChunkSize);

        if (CurrentState != State.Loaded)
        {
            spr.DrawBox(new RectF(origin, size), CurrentState == State.Unloaded ? Color.Red : Color.Yellow);
            return;
        }

        if (Texture.IsDisposed)
            return;

        spr.Draw(sprite, origin, size, Color.White);
    }

    public void Dispose()
    {
        Unload();
    }

    public enum State
    {
        Unloaded,
        Loading,
        Loaded
    }
}