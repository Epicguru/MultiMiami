namespace MultiMiami;

internal static class Program
{
    private static void Main()
    {
        //BenchmarkRunner.Run<InstancingSpeedTest>();

        using var game = new Core();
        game.Run();
    }
}