using Microsoft.Xna.Framework;

namespace MM.Core;

public static class Runtime
{
    /// <summary>
    /// A string detailing the version of monogame that is currently running.
    /// </summary>
    public static string MonogameVersion => _monogameVersion ??= typeof(Game).Assembly.GetName().Version.ToString();

    private static string _monogameVersion;
}
