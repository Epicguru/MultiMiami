using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MM.Core;
using System.Runtime.Intrinsics.Arm;

namespace MM.DearImGui;

/// <summary>
/// The Dear ImGui renderer for Monogame. 
/// </summary>
public class MMImGuiRenderer : MonoGame.ImGui.ImGuiRenderer
{
    private static readonly PresentInterval[] intervals = Enum.GetValues<PresentInterval>();

    private bool showDemoWindow;
    private bool showDebugReadout;

    public MMImGuiRenderer(Game owner) : base(owner)
    {
    }

    /// <summary>
    /// Fixes the half-pixel offset issue.
    /// Previously ImGui had to be offset by half a pixel due to monogame weirdness,
    /// but this has been fixed in recent versions of monogame.
    /// This needs to be accounted for, hence this fix.
    /// </summary>
    protected override Effect UpdateEffect(Texture2D texture)
    {
        System.Numerics.Vector2 vector2 = ImGui.GetIO().DisplaySize;
        var effect = (BasicEffect)base.UpdateEffect(texture);
        float offset = Owner.GraphicsDevice.UseHalfPixelOffset ? 0.5f : 0f;
        effect.Projection = Matrix.CreateOrthographicOffCenter(offset, vector2.X + offset, vector2.Y + offset, offset, -1f, 1f);
        return effect;
    }

    /// <summary>
    /// Draws a debugging main menu bar, with various tools and options.
    /// </summary>
    public void DrawDebugMainMenuBar()
    {
        if (showDemoWindow)
            ImGui.ShowDemoWindow(ref showDemoWindow);

        if (!ImGui.BeginMainMenuBar())
            return;

        // Main game options such as fullscreen, exit, info.
        if (ImGui.BeginMenu("Game"))
        {
            if (ImGui.MenuItem("Exit"))
            {
                Owner.Exit();
            }

            if (ImGui.MenuItem("Show Dear ImGui demo window", null, showDemoWindow))
            {
                showDemoWindow = !showDemoWindow;
            }

            ImGui.Separator();

            ImGui.LabelText("Monogame version", Runtime.MonogameVersion);

            ImGui.EndMenu();
        }

        // Screen related operations and info.
        if (ImGui.BeginMenu("Screen"))
        {
            if (ImGui.MenuItem("Fullscreen", "F11", Screen.IsFullscreen))
            {
                Screen.IsFullscreen = !Screen.IsFullscreen;
            }

            if (ImGui.MenuItem("Hardware Mode Switch", null, Screen.HardwareModeSwitch))
            {
                Screen.HardwareModeSwitch = !Screen.HardwareModeSwitch;
            }

            if (ImGui.BeginMenu("Presentation Interval"))
            {
                foreach (var interval in intervals)
                {
                    if (ImGui.MenuItem(interval.ToString(), null, Screen.PresentationInterval == interval))
                    {
                        Screen.PresentationInterval = interval;
                    }
                }
                ImGui.EndMenu();
            }

            if (ImGui.MenuItem("VSync", null, Screen.VSyncEnabled))
            {
                Screen.VSyncEnabled = !Screen.VSyncEnabled;
            }


            ImGui.Separator();

            ImGui.LabelText("Current Resolution", $"{Screen.ScreenSize.X} x {Screen.ScreenSize.Y}");
            ImGui.LabelText("Display Resolution", $"{Screen.DisplayResolution.X} x {Screen.DisplayResolution.Y}");
            ImGui.LabelText("MSAA Count", Screen.MSAASampleCount.ToString());
            ImGui.LabelText("Graphics Profile", Owner.GraphicsDevice.GraphicsProfile.ToString());

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Loop"))
        {
            if (ImGui.MenuItem("Fixed Time Step", null, Owner.IsFixedTimeStep))
            {
                Owner.IsFixedTimeStep = !Owner.IsFixedTimeStep;
            }

            int fps = (int)Math.Round(1.0 / Owner.TargetElapsedTime.TotalSeconds);
            int min = (int)Math.Round(1.0 / Owner.MaxElapsedTime.TotalSeconds);
            if (ImGui.DragInt("Fixed Time Step FPS", ref fps, 1, min, 360))
            {
                Owner.TargetElapsedTime = TimeSpan.FromSeconds(1.0 / fps);
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Debug"))
        {
            if (ImGui.MenuItem("Show Debug Values", null, showDebugReadout))
            {
                showDebugReadout = !showDebugReadout;
            }

            ImGui.EndMenu();
        }

        if (showDebugReadout)
            DebugReadoutDrawer.DrawWindow();

        ImGui.EndMainMenuBar();
    }
}
