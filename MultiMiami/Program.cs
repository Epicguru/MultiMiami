namespace MultiMiami;

internal static class Program
{
    private static void Main()
    {
        using var game = new Core();
        game.Run();
    }
}