using MM.Core;
using MM.DearImGui;
using MM.Define;
using MultiMiami.Maps;

namespace MultiMiami.Defs;

public abstract class TileDef : Def
{
    [DebugReadout]
    private static string TileDefsDiagnostics => $"Tile defs loaded: {DefDatabase.GetAll<TileDef>().Count}";

    public Sprite Sprite;

    public override void ConfigErrors(ConfigErrorReporter config)
    {
        base.ConfigErrors(config);

        config.Assert(Sprite != null, "Missing sprite.");
    }

    public abstract void Draw(in TileDrawArgs args);
}