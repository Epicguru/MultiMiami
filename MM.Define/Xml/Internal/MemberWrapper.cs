using System.Reflection;

namespace MM.Define.Xml.Internal;

public readonly struct MemberWrapper
{
    public bool IsValid => IsProperty || IsField;
    public bool IsProperty => property != null;
    public bool IsField => field != null;
    public string Name => Member.Name;
    public Type Type => IsField ? field.FieldType : property.PropertyType;
    public MemberInfo Member => IsField ? field : property;
    public IEnumerable<CustomAttributeData> Attributes => IsField ? field.CustomAttributes : property.CustomAttributes;

    public BindingFlags BindingFlags { get; }

    private readonly PropertyInfo property;
    private readonly FieldInfo field;

    public MemberWrapper(PropertyInfo property)
    {
        this.property = property ?? throw new ArgumentNullException(nameof(property));
        field = null;

        BindingFlags = 0;
        if (property.GetMethod != null && property.GetMethod.IsPublic)
            BindingFlags |= BindingFlags.Public;
        else
            BindingFlags |= BindingFlags.NonPublic;

        BindingFlags |= property.GetMethod.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
    }

    public MemberWrapper(FieldInfo field)
    {
        this.field = field ?? throw new ArgumentNullException(nameof(field));
        property = null;

        BindingFlags = 0;
        if (field.IsPublic)
            BindingFlags |= BindingFlags.Public;
        else
            BindingFlags |= BindingFlags.NonPublic;

        BindingFlags |= field.IsStatic ? BindingFlags.Static : BindingFlags.Instance;
    }

    public IEnumerable<T> GetCustomAttributes<T>(bool inherit = true) where T : class
        => Member.GetCustomAttributes(inherit).Where(obj => obj is T t).Select(obj => obj as T);

    public T GetAttribute<T>() where T : Attribute
        => IsField ? field.GetCustomAttribute<T>() : property.GetCustomAttribute<T>();

    public object GetValue(object owner)
        => IsField ? field.GetValue(owner) : property.GetMethod != null ? property.GetValue(owner) : null;

    public void SetValue(object owner, object value)
    {
        if (IsField)
            field.SetValue(owner, value);
        else
            property.SetValue(owner, value);
    }
}
