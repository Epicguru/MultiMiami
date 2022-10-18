using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MM.Core.Structures;

namespace MultiMiami.Utility;

public static class MMExtensions
{
    private static Texture2D pixel;

    internal static void Init()
    {
        pixel = new Texture2D(Core.GD, 1, 1, false, SurfaceFormat.Color);
        pixel.SetData(new[] {Color.White});
    }

    public static void DrawBox(this SpriteBatch spr, in RectF rect, Color color, float depth = 0f)
    {
        spr.Draw(pixel, rect.Position, null, color, 0, default, rect.Size, SpriteEffects.None, depth);
    }

    internal static void Dispose()
    {
        pixel.Dispose();
    }
}
