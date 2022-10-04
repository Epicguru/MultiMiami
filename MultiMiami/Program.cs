using MM.Logging;

namespace MultiMiami;

internal static class Program
{
    private static void Main()
    {
        using var game = new Core();

        try
        {
            game.Run();
        }
        catch (Exception e)
        {
            Log.Error("     >>> FATAL ERROR <<<", e);
            Console.ReadKey();
        }
    }
}