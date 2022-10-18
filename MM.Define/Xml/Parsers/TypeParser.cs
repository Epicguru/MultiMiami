namespace MM.Define.Xml.Parsers;

public class TypeParser : XmlParser<Type>
{
    public override bool CanParseNoContext => true;

    public override object Parse(in XmlParseContext context)
    {
        var found = TypeResolver.Get(context.TextValue);
        return found ?? throw new Exception($"Failed to find any type called '{context.TextValue}' in any loaded assembly."); ;
    }
}