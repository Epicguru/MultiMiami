using Ceras;
using Microsoft.Xna.Framework;
using MM.Core;
using MultiMiami.Maps;
using System.Xml.Serialization;

namespace MultiMiami.Defs;

public class ConnectedTileDef : TileDef
{
    [XmlIgnore, Exclude]
    public readonly Sprite[] Tiles = new Sprite[20];

    public override void PostLoad()
    {
        base.PostLoad();

        if (Sprite == null)
            return;

        for (int i = 0; i < 20; i++)
        {
            Tiles[i] = new Sprite(Sprite.Texture, new Rectangle(Sprite.Region.X + i * TileMap.TILE_SIZE, Sprite.Region.Y, TileMap.TILE_SIZE, TileMap.TILE_SIZE), $"{Sprite.Name}_{i}");
        }
    }

    public override void Draw(in TileDrawArgs args)
    {
        if (Sprite == null)
            return;

        var map = args.Map;
        int x = args.TileX;
        int y = args.TileY;
        var spr = args.Spritebatch;

        static bool DoesConnectTo(in TileContainer t) => t.Wall != null;

        // Main sprite.
        ref var left = ref map.GetTile(x - 1, y);
        ref var up = ref map.GetTile(x, y - 1);
        ref var right = ref map.GetTile(x + 1, y);
        ref var down = ref map.GetTile(x, y + 1);

        ref var tl = ref map.GetTile(x - 1, y - 1);
        ref var tr = ref map.GetTile(x + 1, y - 1);
        ref var br = ref map.GetTile(x + 1, y + 1);
        ref var bl = ref map.GetTile(x - 1, y + 1);

        int index = 0;
        if (DoesConnectTo(left))
            index |= 0b_1000;
        if (DoesConnectTo(up))
            index |= 0b_0100;
        if (DoesConnectTo(right))
            index |= 0b_0010;
        if (DoesConnectTo(down))
            index |= 0b_0001;

        spr.Draw(Tiles[index], new Vector2(x, y), Vector2.One, Color.White);

        if (DoesConnectTo(up))
        {
            // Top-left corner.
            if (DoesConnectTo(left) && !DoesConnectTo(tl))
            {
                spr.Draw(Tiles[16], new Vector2(x, y), Vector2.One, Color.White);
            }

            // Top-right corner.
            if (DoesConnectTo(right) && !DoesConnectTo(tr))
            {
                spr.Draw(Tiles[17], new Vector2(x, y), Vector2.One, Color.White);
            }
        }

        if (DoesConnectTo(down))
        {
            // Bottom-right corner.
            if (DoesConnectTo(right) && !DoesConnectTo(br))
            {
                spr.Draw(Tiles[18], new Vector2(x, y), Vector2.One, Color.White);
            }

            // Bottom-left corner.
            if (DoesConnectTo(left) && !DoesConnectTo(bl))
            {
                spr.Draw(Tiles[19], new Vector2(x, y), Vector2.One, Color.White);
            }
        }
    }
}