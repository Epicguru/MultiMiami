using System.Diagnostics;

namespace MM.Core;

public static class Time
{
    /// <summary>
    /// The maximum allowed value for <see cref="DeltaTime"/>.
    /// </summary>
    public static float MaxDeltaTime { get; set; } = 1f / 10f;

    /// <summary>
    /// The time, in seconds, between the last frame and this one.
    /// Does not change throughout the duration of the frame.
    /// </summary>
    public static float DeltaTime { get; private set; }

    /// <summary>
    /// The total time that the game has been running for, in seconds.
    /// Updated at the start of each frame.
    /// </summary>
    public static float TotalTime { get; private set; }

    private static readonly Stopwatch updateTimer = new Stopwatch();
    private static readonly Stopwatch drawTimer = new Stopwatch();
    private static readonly Stopwatch globalTimer = new Stopwatch();

    static Time()
    {
        updateTimer.Start();
        drawTimer.Start();
        globalTimer.Start();
    }

    public static void Update()
    {
        DeltaTime = (float)Math.Min(updateTimer.Elapsed.TotalSeconds, MaxDeltaTime);
        updateTimer.Restart();

        TotalTime = (float)globalTimer.Elapsed.TotalSeconds;
    }

    public static void Draw()
    {
        DeltaTime = (float)Math.Min(drawTimer.Elapsed.TotalSeconds, MaxDeltaTime);
        drawTimer.Restart();
    }
}
