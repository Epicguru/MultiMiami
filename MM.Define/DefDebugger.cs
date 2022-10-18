using MM.Define.Xml;

namespace MM.Define;

public static class DefDebugger
{
    public static event Action<string> OnWarning;
    public static event Action<string, Exception> OnError;
    public static event ParseErrorDelegate OnXmlParseError;

    public delegate void ParseErrorDelegate(string message, in XmlParseContext ctx, Exception e);

    internal static void Warn(string message)
    {
        OnWarning?.Invoke(message);
    }

    internal static void Error(string message, in XmlParseContext ctx, Exception e = null)
    {
        OnXmlParseError?.Invoke(message, ctx, e);
    }

    internal static void Error(string message, Exception e = null)
    {
        OnError?.Invoke(message, e);
    }
}