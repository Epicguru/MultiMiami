using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MM.Core.TileMaps;

public class Tileset
{
    public Texture2D Texture => Sprites[0].Texture;
    public int TileSize => Sprites[0].Region.Width;

    public readonly string Name;
    public readonly Sprite[] Sprites;

    public Tileset(Texture2D texture, Point origin, int tileSize, string name)
    {
        Name = name;
        Sprites = new Sprite[16];

        var size = new Point(tileSize);
        for (int i = 0; i < 16; i++)
        {
            var region = new Rectangle(origin + new Point(i * tileSize, 0), size);
            Sprites[i] = new Sprite(texture, region, $"{name}_{i}");
        }
    }

    public override string ToString() => Name;
}
