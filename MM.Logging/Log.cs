using System.Runtime.CompilerServices;

namespace MM.Logging;

public static class Log
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Output(string message, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = ConsoleColor.White;
    }

    public static void Trace(string message)
    {
        Output(message, ConsoleColor.Gray);
    }

    public static void Info(string message)
    {
        Output(message, ConsoleColor.Cyan);
    }

    public static void Warn(string message)
    {
        Output(message, ConsoleColor.Yellow);
    }

    public static void Error(string message, Exception e = null)
    {
        Output(message, ConsoleColor.Red);
        if (e != null)
            Output(e.ToString(), ConsoleColor.Red);
    }
}
