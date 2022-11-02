using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MM.Core;
using MM.Core.Structures;
using MM.Logging;
using MultiMiami.Utility;
using System.Diagnostics;

namespace MultiMiami.Maps;

public class TileChunk : IDisposable
{
    public static int MaxSpritebatchCount = 10;

    private static readonly Queue<SpriteBatch> spriteBatchPool = new Queue<SpriteBatch>(32);
    private static int borrowedCount;

    private static SpriteBatch GetSpritebatch()
    {
        // Spin-lock threads if there are already too many active sprite batches.
        // This prevents the creation of too many batches (which use loads of memory) and also actually speeds up rendering.
        while (borrowedCount >= MaxSpritebatchCount)
        {
            Thread.Sleep(1);
        }

        lock (spriteBatchPool)
        {
            borrowedCount++;
            if (spriteBatchPool.TryDequeue(out var spr))
                return spr;
            return new SpriteBatch(Core.GD, 1024 * 6); // Important - capacity must be large enough to hold all drawable in chunk.
        }
    }

    private static void ReturnSpritebatch(SpriteBatch spr)
    {
        lock (spriteBatchPool)
        {
            borrowedCount--;
            Debug.Assert(!spriteBatchPool.Contains(spr));
            spriteBatchPool.Enqueue(spr);
        }
    }

    public int SizeInPixels => Map.ChunkSize * TileMap.TILE_SIZE;
    public State CurrentState { get; private set; } = State.Unloaded;
    public Vector2 Origin => new Vector2(X * Map.ChunkSize, Y * Map.ChunkSize);
    public Vector2 CenterPosition => Origin + new Vector2(Map.ChunkSize * 0.5f, Map.ChunkSize * 0.5f);
    public RectF Bounds => new RectF(Origin, new Vector2(Map.ChunkSize, Map.ChunkSize));
    public RectF DrawBounds => new RectF(Origin - new Vector2(0.5f, 0.5f), new Vector2(Map.ChunkSize, Map.ChunkSize));

    public readonly TileMap Map;
    public readonly int X, Y;
    public RenderTarget2D Texture { get; private set; }

    private Sprite sprite;
    private float timeSinceLoaded;

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
            spr.Begin(new SpriteBatchArgs
            {
                BlendState = BlendState.NonPremultiplied,
                SamplerState = SamplerState.PointClamp,
                Matrix = Matrix.CreateTranslation(-X * Map.ChunkSize + 0.5f, -Y * Map.ChunkSize + 0.5f, 0) * Matrix.CreateScale(TileMap.TILE_SIZE)
            });

            for (int x = X * Map.ChunkSize; x < (X + 1) * Map.ChunkSize; x++)
            {
                for (int y = Y * Map.ChunkSize; y < (Y + 1) * Map.ChunkSize; y++)
                {
                    ref var tile = ref Map.GetTileUnsafe(x, y);
                    var args = new TileDrawArgs(ref tile)
                    {
                        Map = Map,
                        Spritebatch = spr,
                        DrawPosition = new Vector2(x, y),
                        TileX = x,
                        TileY = y,
                    };

                    tile.Floor?.Draw(args with { Layer = TileLayer.Floor });
                    tile.Wall?.Draw(args with { Layer = TileLayer.Wall });
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
                    Core.GD.SetRenderTarget(Texture);
                    Core.GD.Clear(Color.Bisque);
                    spr.End();
                    Core.GD.SetRenderTarget(null);

                    // Finalize.
                    sprite = new Sprite(Texture)
                    {
                        OriginNormalized = Vector2.Zero
                    };
                    timeSinceLoaded = 0f;
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

    public void Update()
    {
        timeSinceLoaded += Time.DeltaTime;
    }

    public void Draw(SpriteBatch spr)
    {
        var origin = Origin - new Vector2(0.5f, 0.5f);
        var size = new Vector2(Map.ChunkSize, Map.ChunkSize);

        if (CurrentState != State.Loaded)
        {
            //spr.DrawBox(new RectF(origin, size), CurrentState == State.Unloaded ? Color.Red : Color.Yellow);
            return;
        }

        if (Texture.IsDisposed)
            return;

        var c = Color.White;
        c.A = (timeSinceLoaded / 0.4f).ToNormalizedByte();
        spr.Draw(sprite, origin, size, c);
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