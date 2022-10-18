namespace MM.Define.Xml;

/// <summary>
/// Specifies alternative names for this field or property.
/// This allows the value to be loaded from XML if the XML node has this specified name.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class AliasAttribute : Attribute
{
    public readonly string Alias;

    public AliasAttribute(string name) => Alias = name;
}