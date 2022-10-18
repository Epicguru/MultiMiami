namespace MM.Define.Xml.Parsers;

public sealed class EnumParser : XmlParser
{
    public override bool CanParseNoContext => true;

    public override bool CanHandle(Type type) => type.IsEnum;

    public override object Parse(in XmlParseContext context)
    {
        return Enum.Parse(context.TargetType, context.TextValue);
    }
}