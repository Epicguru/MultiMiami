using HarmonyLib;

namespace MM.Multiplayer;

public static class Net
{
    /// <summary>
    /// The <see cref="Harmony"/> instanced used to patch networking functions such as RPCs.
    /// </summary>
    public static Harmony Harmony { get; } = new Harmony("MM.Multiplayer");

    /// <summary>
    /// The net tick counter. Increases by 1 each tick.
    /// </summary>
    public static int Tick;
}
