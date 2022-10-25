using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MM.Logging;
using System.Diagnostics;

namespace MM.Core.Atlas;

public class SpriteAtlas : IDisposable, IEnumerable<Sprite>
{
    public static SpriteAtlas FromPackedImages(GraphicsDevice graphicsDevice, string outputFilePath, string rootFolder, IEnumerable<string> inputFiles)
    {
        if (File.Exists(outputFilePath))
        {
            // TODO load from here.
        }

        int atlasSize = 1024;
        int padding = 1;

        var workingRects = new List<(Rectangle bounds, string filePath, Color[] pixels)>();

        bool IntersectsAny(in Rectangle bounds)
        {
            foreach (var pair in workingRects)
            {
                if (pair.bounds.Intersects(bounds))
                    return true;
            }
            return false;
        }

        foreach (var fp in inputFiles)
        {
            if (!File.Exists(fp))
            {
                Log.Error($"Input texture file '{fp}' does not exist!");
                continue;
            }

            using var fs = new FileStream(fp, FileMode.Open);
            using var tex = Texture2D.FromStream(graphicsDevice, fs);

            var bounds = new Rectangle(0, 0, tex.Width + padding * 2, tex.Height + padding * 2);
            var pixels = new Color[tex.Width * tex.Height];
            tex.GetData(pixels);

            // Find a spot for it.
            bool wasPacked = false;
            for (int x = 0; x < atlasSize - tex.Width - padding * 2; x++)
            {
                for (int y = 0; y < atlasSize - tex.Height - padding * 2; y++)
                {
                    if (IntersectsAny(bounds with { X = x, Y = y }))
                        continue;

                    wasPacked = true;
                    workingRects.Add((bounds with {X = x, Y = y}, fp, pixels));
                    break;
                }
                if (wasPacked)
                    break;
            }

            if (!wasPacked)
                throw new Exception($"Failed to fit {fp} into the {atlasSize}x{atlasSize} atlas!");
        }

        Debug.Assert(workingRects.Count == inputFiles.Count());

        var atlas = new SpriteAtlas();
        atlas.texture = new Texture2D(graphicsDevice, atlasSize, atlasSize, false, SurfaceFormat.Color);

        foreach (var item in workingRects)
        {
            var finalBounds = item.bounds.ExpandedBy(-padding);
            atlas.texture.SetData(0, finalBounds, item.pixels, 0, item.pixels.Length);

            string name = item.filePath.Replace('\\', '/');
            int contentIndex = name.IndexOf("Content/", StringComparison.Ordinal);
            if (contentIndex >= 0)
                name = name[(contentIndex + 8)..];
            string ext = new FileInfo(name).Extension;
            if (ext.Length > 0)
                name = name.Replace(ext, "");

            var spr = new Sprite(atlas.texture, finalBounds, name);
            atlas.sprites.Add(name, spr);
        }

        using var outFs = new FileStream(outputFilePath, FileMode.Create);
        atlas.texture.SaveAsPng(outFs, atlasSize, atlasSize);

        return atlas;
    }

    public int Count => sprites.Count;
    public Sprite this[string name] => sprites.TryGetValue(name, out var found) ? found : null;

    private readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>(64);
    private Texture2D texture;

    public void Dispose()
    {
        if (!texture.IsDisposed)
            texture.Dispose();
        texture = null;

        foreach (var s in sprites.Values)
            s.Texture = null;

        sprites.Clear();
    }

    public IEnumerator<Sprite> GetEnumerator() => sprites.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => sprites.Values.GetEnumerator();
}