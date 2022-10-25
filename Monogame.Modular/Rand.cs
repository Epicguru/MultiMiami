namespace MM.Core;

public static class Rand
{
    public static bool Bool => random.NextDouble() > 0.5;

    private static readonly Random random = new Random();

    /// <summary>
    /// Gets a random integer between 0 and <paramref name="upperBound"/>,
    /// including 0 but excluding <paramref name="upperBound"/>.
    /// </summary>
    /// <param name="upperBound">The exclusive upper bound.</param>
    /// <returns></returns>
    public static int Number(int upperBound) => random.Next(upperBound);

    /// <summary>
    /// Gets a random integer in the range [<paramref name="a"/>, <paramref name="b"/>).
    /// </summary>
    /// <param name="a">The inclusive lower bound.</param>
    /// <param name="b">The exclusive upper bound.</param>
    public static int InRange(int a, int b) => random.Next(a, b);

    /// <summary>
    /// Gets a random floating point number in the range [<paramref name="a"/>, <paramref name="b"/>].
    /// </summary>
    /// <param name="a">The inclusive lower bound.</param>
    /// <param name="b">The inclusive upper bound.</param>
    public static float InRange(float a, float b) => a + random.NextSingle() * (b - a);

    /// <summary>
    /// Returns <c>true</c> <paramref name="chance"/>% of the time,
    /// where <paramref name="chance"/> is in the range [0, 1].
    /// </summary>
    public static bool Chance(float chance) => chance > 0 && random.NextSingle() <= chance;

    /// <summary>
    /// Returns <c>true</c> <paramref name="chance"/>% of the time,
    /// where <paramref name="chance"/> is in the range [0, 1].
    /// </summary>
    public static bool Chance(double chance) => chance > 0 && random.NextDouble() <= chance;
}