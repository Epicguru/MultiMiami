using System.Xml;

namespace MM.Define.Xml.Parsers;

public sealed class XmlNodeParser : XmlParser<XmlNode>
{
    public override object Parse(in XmlParseContext context) => context.Node;
}