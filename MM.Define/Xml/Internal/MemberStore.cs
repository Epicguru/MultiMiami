using System.Reflection;
using System.Xml.Serialization;

namespace MM.Define.Xml.Internal;

public class MemberStore
{
    public readonly Type TargetType;
    public readonly DefLoadConfig Config;

    private readonly Dictionary<string, MemberWrapper> members = new Dictionary<string, MemberWrapper>();

    public MemberStore(DefLoadConfig config, Type targetType)
    {
        TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
        Config = config ?? throw new ArgumentNullException(nameof(config));

        // Disallow writing ID.
        members["ID"] = default;
        members["Id"] = default;
        members["id"] = default;
        members["iD"] = default;
    }

    private bool ShouldSee(FieldInfo field)
    {
        if (field.GetCustomAttribute<XmlIncludeAttribute>() != null)
            return true;

        if (field.GetCustomAttribute<XmlIgnoreAttribute>() != null)
            return false;

        if (!Config.DefaultMemberTypes.HasFlag(MemberTypes.Field))
            return false;

        if (field.IsPublic && !Config.DefaultMemberBindingFlags.HasFlag(BindingFlags.Public))
            return false;

        if (!field.IsPublic && !Config.DefaultMemberBindingFlags.HasFlag(BindingFlags.NonPublic))
            return false;

        return true;
    }

    private bool ShouldSee(PropertyInfo prop)
    {
        if (prop.SetMethod == null)
            return false;

        if (prop.GetCustomAttribute<XmlIncludeAttribute>() != null)
            return true;

        if (prop.GetCustomAttribute<XmlIgnoreAttribute>() != null)
            return false;

        if (!Config.DefaultMemberTypes.HasFlag(MemberTypes.Property))
            return false;

        if (prop.SetMethod.IsPublic && !Config.DefaultMemberBindingFlags.HasFlag(BindingFlags.Public))
            return false;

        if (!prop.SetMethod.IsPublic && !Config.DefaultMemberBindingFlags.HasFlag(BindingFlags.NonPublic))
            return false;

        return true;
    }

    private IEnumerable<string> GetNames(MemberInfo member)
    {
        yield return member.Name;

        foreach (var alias in member.GetCustomAttributes<AliasAttribute>())
            yield return alias.Alias;
    }

    private IEnumerable<MemberWrapper> GetMember(Predicate<MemberInfo> selector)
    {
        const BindingFlags ALL = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        foreach (var field in TargetType.GetFields(ALL))
        {
            if (!ShouldSee(field))
                continue;

            if (selector(field))
                yield return new MemberWrapper(field);
        }

        foreach (var prop in TargetType.GetProperties(ALL))
        {
            if (!ShouldSee(prop))
                continue;

            if (selector(prop))
                yield return new MemberWrapper(prop);
        }
    }

    public MemberWrapper GetMember(string name)
    {
        if (members.TryGetValue(name, out var f))
            return f;

        var stringComp = Config.MemberNamesAreCaseSensitive ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        var found = GetMember(member => GetNames(member).Any(n => n.Equals(name, stringComp))).FirstOrDefault();

        members.Add(name, found);
        return found;
    }

    public IEnumerable<MemberWrapper> GetAllMembers() => GetMember(_ => true);
}