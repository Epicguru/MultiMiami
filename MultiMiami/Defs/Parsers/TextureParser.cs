using Microsoft.Xna.Framework.Graphics;
using MM.Define.Xml;
using MM.Define.Xml.Parsers;

namespace MultiMiami.Defs.Parsers;

public class TextureParser : XmlParser<Texture2D>
{
    public override bool CanParseNoContext => true;

    public override object Parse(in XmlParseContext context)
    {
        string txt = context.TextValue;
        return ContentLoader.Load<Texture2D>(txt);
    }
}