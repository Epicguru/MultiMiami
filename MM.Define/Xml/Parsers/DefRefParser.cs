namespace MM.Define.Xml.Parsers;

public sealed class DefRefParser : XmlParser
{
    public override bool CanParseNoContext => true;

    public override bool CanHandle(Type type) => typeof(IDef).IsAssignableFrom(type);

    public override object Parse(in XmlParseContext context)
    {
        var found = context.Loader.TryGetDef(context.TextValue);
        if (found != null && !context.TargetType.IsInstanceOfType(found))
            throw new Exception($"Def reference '{context.TextValue}' is of type '{found.GetType().FullName}' which cannot be assigned to target def type '{context.TargetType.FullName}'.");

        return found ?? throw new Exception($"Failed to resolve def reference: '{context.TextValue}' as def type '{context.TargetType.FullName}'.");
    }

    public override string ValueToString(object obj) => ((IDef)obj).ID;
}