using ImGuiNET;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MM.Core;
using MM.DearImGui;
using MM.Logging;
using MM.Multiplayer;
using MonoGame.ImGui.Extensions;

namespace MultiMiami;

public class Core : Game, IStableTicker
{
    public static GraphicsDeviceManager GDM { get; private set; }
    public static GraphicsDevice GD { get; private set; }
    public static Camera2D Camera { get; private set; }

    public int TargetTickRate => 60;

    private SpriteBatch spr;
    private Texture2D texture;
    private Sprite sprite;
    private MMImGuiRenderer imGuiRenderer;

    public Core()
    {
        GDM = new GraphicsDeviceManager(this);
        IsMouseVisible = true;
        IsFixedTimeStep = false; // Garbage :)
        GDM.GraphicsProfile = GraphicsProfile.HiDef;

        GDM.PreparingDeviceSettings += GDM_PreparingDeviceSettings;
        Window.ClientSizeChanged += WindowSizeChanged;
    }

    private void WindowSizeChanged(object sender, EventArgs e)
    {
        if (Screen.IsFullscreen)
            return;

        Screen.SetSize(Screen.WindowSize.X, Screen.WindowSize.Y);
        Log.Trace($"Window resized, set resolution to {Screen.WindowSize}");
    }

    private void GDM_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
    {
        e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 16;
    }

    protected override void Initialize()
    {
        base.Initialize();

        Log.Info("Hello, world!");
        GD = GraphicsDevice;

        // Init camera and screen.
        Camera = new Camera2D
        {
            Scale = 128
        };

        Screen.Init(this, GDM);
        Screen.Resizable = true;

        // Spritebatch and other rendering stuff.
        spr = new SpriteBatch(GraphicsDevice, 2048);

        // Dear ImGui
        imGuiRenderer = new MMImGuiRenderer(this);
        imGuiRenderer.Initialize();
        imGuiRenderer.RebuildFontAtlas();

        InitNet();
    }

    protected override void LoadContent()
    {
        base.LoadContent();

        using var fs = new FileStream("./Content/Textures/Mogus.png", FileMode.Open);
        texture = Texture2D.FromStream(GraphicsDevice, fs);
        sprite = new Sprite(texture);
    }

    protected override void UnloadContent()
    {
        base.UnloadContent();

        texture.Dispose();
        spr.Dispose();
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        Screen.WindowTitle = $"MultiMiami - {Screen.StableTicksPerSecond} TPS, {Screen.UpdatesPerSecond} UPS, {Screen.FramesPerSecond} FPS";
    }

    public void StableTick()
    {

    }

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        // Clear screen.
        GraphicsDevice.Clear(Color.AliceBlue);

        spr.Begin(new SpriteBatchArgs
        {
            BlendState = BlendState.NonPremultiplied,
            Matrix = Camera.GetMatrix(Screen.ScreenSize.ToVector2()),
            SamplerState = SamplerState.PointClamp
        });

        // Draw here!
        spr.Draw(sprite, new Vector2(0, 0), new Vector2(1, 1), Color.White);

        spr.End();

        // ImGui overlay.
        imGuiRenderer.BeginLayout(gameTime);
        DrawImGui();
        imGuiRenderer.EndLayout();
    }

    private void DrawImGui()
    {
        imGuiRenderer.DrawDebugMainMenuBar();

        float rads = Camera.Rotation;
        if (ImGui.SliderAngle("Camera Rotation", ref rads))
        {
            Camera.Rotation = rads;
        }

        float scale = Camera.Scale;
        if (ImGui.DragFloat("Camera Zoom", ref scale))
        {
            Camera.Scale = scale;
        }

        var pos = new System.Numerics.Vector2(Camera.Position.X, Camera.Position.Y);
        if (ImGui.DragFloat2("Camera Position", ref pos))
        {
            Camera.Position = pos.ToXnaVector2();
        }

        DrawNetWindow();
    }

    private GameServer<Player> server;
    private GameClient client;
    private const int PORT = 7777;

    private void InitNet()
    {
        
    }

    public class Player : NetPlayer
    {
        public readonly string Name;

        public Player(NetConnection connection, string name) : base(connection) { Name = name; }

        public override string ToString() => Name;
    }

    private void OnClientConnecting(NetIncomingMessage msg, out Player player)
    {
        if (msg.ReadByte(out var b) && b == 123)
        {
            msg.SenderConnection.Approve();
            player = new Player(msg.SenderConnection, msg.ReadString());
        }
        else
        {
            msg.SenderConnection.Deny();
            player = null;
        }
    }

    private void DrawNetWindow()
    {
        server?.Tick();
        client?.Tick();

        if (!ImGui.Begin("Network", ImGuiWindowFlags.AlwaysAutoResize))
            return;

        // TODO

        if (server == null && ImGui.Button("Start server"))
        {
            var serverConfig = new NetPeerConfiguration("GAME")
            {
                Port = PORT,
            };
            server = new GameServer<Player>(serverConfig, OnClientConnecting);
            server.Start();
        }
        if (server != null && ImGui.Button("Stop server"))
        {
            server.Shutdown("Fuck off");
            server = null;
        }
        if (server != null)
        {
            ImGui.LabelText("Server status", server.Status.ToString());
            ImGui.LabelText("Server connected client count", server.ConnectionsCount.ToString());
            ImGui.BeginListBox("Connected players");

            foreach (var player in server.Players)
            {
                ImGui.Text(player.ToString());
            }

            ImGui.EndListBox();
        }

        if (client == null && ImGui.Button("Start client"))
        {
            client = new GameClient(new NetPeerConfiguration("GAME"));
            client.Start();
            var msg = client.CreateMessage(128);
            msg.Write((byte) 123);
            msg.Write("James");
            client.Connect("localhost", PORT, msg);
            Log.Info("Started connect...");
        }
        if (client != null && ImGui.Button("Stop client"))
        {
            client.Disconnect("Peace, I'm out.");
            client = null;
        }
        if (client != null)
        {
            ImGui.LabelText("Client status", client.Status.ToString());
            ImGui.LabelText("Client connection status", client.ConnectionStatus.ToString());
            ImGui.LabelText("Connected to", client.ServerConnection?.RemoteEndPoint.ToString() ?? "null");
            ImGui.LabelText("RTT", client.ServerConnection?.AverageRoundtripTime.ToString() ?? "null");
        }

        ImGui.End();
    }
}
