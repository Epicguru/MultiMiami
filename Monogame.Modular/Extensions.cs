using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MM.Core;

public static class Extensions
{
    private static readonly Dictionary<SpriteBatch, SpriteBatchArgs> currentArgs = new Dictionary<SpriteBatch, SpriteBatchArgs>();

    public static void Begin(this SpriteBatch batch, in SpriteBatchArgs args)
    {
        batch.Begin(args.SortMode, args.BlendState, args.SamplerState, args.DepthStencilState, args.RasterizerState, args.Effect, args.Matrix);
        lock (currentArgs)
        {
            currentArgs[batch] = args;
        }
    }

    public static SpriteBatchArgs GetArgs(this SpriteBatch batch)
    {
        lock (currentArgs)
        {
            if (currentArgs.TryGetValue(batch, out var found))
                return found;
        }

        return new SpriteBatchArgs();
    }

    public static void Draw(this SpriteBatch spr, Sprite sprite, Vector2 position, Vector2 size, Color color)
    {
        if (sprite?.Texture == null)
            throw new ArgumentNullException(nameof(sprite), "Sprite or sprite texture is null, cannot draw.");

        var scale = new Vector2(size.X / sprite.Region.Width, size.Y / sprite.Region.Height);
        spr.Draw(sprite.Texture, position, sprite.Region, color, 0, sprite.Region.Size.ToVector2() * sprite.OriginNormalized, scale, SpriteEffects.None, 0f);
    }

    public static Rectangle ExpandedBy(in this Rectangle rect, int offset) => rect.ExpandedBy(offset, offset);

    public static Rectangle ExpandedBy(in this Rectangle rect, int x, int y) => new Rectangle(rect.X - x, rect.Y - y, rect.Width + x * 2, rect.Height + y * 2);

    public static byte ToNormalizedByte(this float f) => f > 1f ? (byte)255 : f < 0 ? (byte)0 : (byte)(f * byte.MaxValue);
}
