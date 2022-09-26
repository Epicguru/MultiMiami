namespace MM.Logging;

[Flags]
public enum LogLevel
{
    Trace  = 1 << 1,
    Info   = 1 << 2,
    Warn   = 1 << 3,
    Error  = 1 << 4,
}
