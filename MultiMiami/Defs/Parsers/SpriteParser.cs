using Microsoft.Xna.Framework.Graphics;
using MM.Core;
using MM.Define.Xml;
using MM.Define.Xml.Parsers;

namespace MultiMiami.Defs.Parsers;

public class SpriteParser : XmlParser<Sprite>
{
    private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    public override bool CanParseNoContext => true;

    public override object Parse(in XmlParseContext context)
    {
        string txt = context.TextValue;
        if (cache.TryGetValue(txt, out var found))
            return found;

        var inAtlas = Core.Atlas[txt];
        if (inAtlas != null)
            return inAtlas;

        var tex = ContentLoader.Load<Texture2D>(txt);
        var spr = new Sprite(tex);
        cache.Add(txt, spr);
        return spr;
    }
}