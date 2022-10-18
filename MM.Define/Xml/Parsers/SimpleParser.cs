using System.Diagnostics;

namespace MM.Define.Xml.Parsers;

public sealed class SimpleParser<T> : XmlParser<T>
{
    public override bool CanParseNoContext => true;

    public readonly Func<string, T> ParseFunction;

    public SimpleParser(Func<string, T> parseFunc)
    {
        ParseFunction = parseFunc ?? throw new ArgumentNullException(nameof(parseFunc));
    }

    public override object Parse(in XmlParseContext context)
    {
        Debug.Assert(context.TextValue != null);
        return ParseFunction(context.TextValue);
    }
}