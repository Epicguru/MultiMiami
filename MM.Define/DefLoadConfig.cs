using System.Reflection;

namespace MM.Define;

public class DefLoadConfig
{
    /// <summary>
    /// The binding flags for members that will be included in XML/Ceras serialization by default.
    /// Default value is <c>BindingFlags.Public | BindingFlags.Instance</c>
    /// </summary>
    public BindingFlags DefaultMemberBindingFlags { get; set; } = BindingFlags.Public | BindingFlags.Instance;

    /// <summary>
    /// The member types that will be included in XML/Ceras serialization by default.
    /// Only valid values are <see cref="MemberTypes.Field"/> and <see cref="MemberTypes.Property"/>.
    /// Default value is <c>MemberTypes.Field</c>
    /// </summary>
    public MemberTypes DefaultMemberTypes { get; set; } = MemberTypes.Field;

    /// <summary>
    /// Are field and property names case-sensitive when loading from XML?
    /// Default value is <c>true</c>.
    /// </summary>
    public bool MemberNamesAreCaseSensitive { get; set; } = true;

    /// <summary>
    /// The XML node name for list items.
    /// </summary>
    public string ListItemName { get; set; } = "li";
}