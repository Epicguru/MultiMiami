namespace MM.Define.Xml.Parsers;

public abstract class XmlParser
{
    /// <summary>
    /// Can this parser create an object based just on
    /// <see cref="XmlParseContext.TextValue"/> and <see cref="XmlParseContext.TargetType"/>?
    /// False by default.
    /// </summary>
    public virtual bool CanParseNoContext => false;

    public abstract bool CanHandle(Type type);

    public abstract object Parse(in XmlParseContext context);

    public virtual string ValueToString(object obj) => obj?.ToString();

    public virtual void EarlyPostLoad(XmlLoader loader) { }
}

public abstract class XmlParser<T> : XmlParser
{
    public sealed override bool CanHandle(Type t) => typeof(T) == t;
}
