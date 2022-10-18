using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace MultiMiami;

public static class ContentLoader
{
    private static readonly Dictionary<string, object> cache = new Dictionary<string, object>(256);
    private static readonly string[] textureExtensions =
    {
        ".png",
        ".jpg",
        ".jpeg",
        ".bmp"
    };

    public static T Load<T>(string path, bool addToCache = true) => (T)Load(path, typeof(T), addToCache);

    public static object Load(string path, Type type, bool addToCache = true)
    {
        Debug.Assert(path != null);
        Debug.Assert(type != null);

        if (cache.TryGetValue(path, out var found))
            return found;

        var loaded = LoadInternal(path, type);

        if (loaded != null && addToCache)
            cache.Add(path, loaded);

        return loaded;
    }

    private static object LoadInternal(string path, Type type)
    {
        if (type == typeof(Texture2D))
            return LoadTexture(path);

        throw new Exception($"Unknown content type: {type.FullName}");
    }

    private static string TryResolveFilePath(string rawPath, string[] extensions)
    {
        var info = new FileInfo($"./Content/{rawPath}");
        bool hasExt = info.Extension.Length != 0;

        if (hasExt)
        {
            return info.Exists ? $"./Content/{rawPath}" : null;
        }

        foreach (var ext in extensions)
        {
            string path = $"./Content/{rawPath}{ext}";
            if (File.Exists(path))
                return path;
        }

        return null;
    }

    private static Texture2D LoadTexture(string path)
    {
        var resolvedPath = TryResolveFilePath(path, textureExtensions);
        if (resolvedPath == null)
            throw new FileNotFoundException($"Failed to find any valid texture file at '{path}'");

        using var fs = new FileStream(resolvedPath, FileMode.Open);
        var tex = Texture2D.FromStream(Core.GD, fs);
        tex.Name = path;
        return tex;
    }
}
