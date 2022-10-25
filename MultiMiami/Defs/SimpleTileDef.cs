using Microsoft.Xna.Framework;
using MM.Core;
using MultiMiami.Maps;

namespace MultiMiami.Defs;

public class SimpleTileDef : TileDef
{
    public override void Draw(in TileDrawArgs args)
    {
        if (Sprite == null)
            return;

        args.Spritebatch.Draw(Sprite, args.DrawPosition, Vector2.One, Color.White);
    }
}