using Ceras;
using Microsoft.Xna.Framework;
using MM.Core;
using MM.Define;
using MultiMiami.Maps;
using System.Xml.Serialization;

namespace MultiMiami.Defs
{
    public class TileDef : Def
    {
        [XmlIgnore, Exclude]
        public readonly Sprite[] Tiles = new Sprite[16];

        public Sprite Texture;

        public override void PostLoad()
        {
            base.PostLoad();

            if (Texture == null)
                return;

            for (int i = 0; i < 16; i++)
            {
                Tiles[i] = new Sprite(Texture.Texture, new Rectangle(Texture.Region.X + i * TileMap.TILE_SIZE, Texture.Region.Y, TileMap.TILE_SIZE, TileMap.TILE_SIZE), $"{Texture.Name}_{i}");
            }
        }

        public override void ConfigErrors(ConfigErrorReporter config)
        {
            base.ConfigErrors(config);

            const int WIDTH = TileMap.TILE_SIZE * 16;

            config.Assert(Texture.Width == WIDTH, $"Tile atlases should have a width of {WIDTH} pixels.");
            config.Assert(Texture.Height == TileMap.TILE_SIZE, $"Tile atlases should have a height of {TileMap.TILE_SIZE} pixels.");

            if (Texture == null)
                config.Error("Missing texture.");
        }

        public Sprite GetSpriteForTile(in TileContainer tile, int x, int y, TileMap map)
        {
            return Tiles[0];
        }
    }
}
