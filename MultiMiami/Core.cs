using ImGuiNET;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MM.Core;
using MM.Core.Structures;
using MM.DearImGui;
using MM.Logging;
using MM.Multiplayer;
using MonoGame.ImGui.Extensions;
using System.Diagnostics;
using System.Globalization;

namespace MultiMiami;

public class Core : Game
{
    public static GraphicsDeviceManager GDM { get; private set; }
    public static GraphicsDevice GD { get; private set; }
    public static Camera2D Camera { get; private set; }
    public static HeartbeatComponent Heartbeats { get; private set; }

    public int TargetTickRate => 60;

    private SpriteBatch spr;
    private Texture2D texture;
    private Sprite sprite;
    private MMImGuiRenderer imGuiRenderer;
    private Stopwatch sw = new Stopwatch();

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

        Components.Add(Heartbeats = new HeartbeatComponent(this));

        Heartbeats.Add(NetTick, 1.0 / 60.0);
        Heartbeats.Add(OncePerSecond, 1.0);
            
        sw.Start();
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

        Screen.WindowTitle = $"MultiMiami - {Screen.UpdatesPerSecond} UPS, {Screen.FramesPerSecond} FPS";
    }

    private void NetTick()
    {
        server?.Tick();
        client?.Tick();
        Net.Tick++;
    }

    private void OncePerSecond()
    {
        if (server != null)
        {
            bytesOut.Enqueue(server.Statistics.SentBytes - lastBytesSent);
            lastBytesSent = server.Statistics.SentBytes;
        }
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

    private GameServer server;
    private GameClient client;
    private const int PORT = 7777;
    private readonly ScrollingBuffer<float> bytesOut = new ScrollingBuffer<float>(30);
    private long lastBytesSent;

    public class Player : INetPlayer
    {
        public NetConnection Connection { get; }
        public readonly string Name;

        public Player(NetConnection connection, string name)
        {
            Connection = connection;
            Name = name;
        }

        public override string ToString() => Name;
    }

    private static Player OnClientConnecting(NetIncomingMessage msg)
    {
        if (msg.ReadByte(out var b) && b == 123)
        {
            msg.SenderConnection.Approve();
            return new Player(msg.SenderConnection, msg.ReadString());
        }
        else
        {
            msg.SenderConnection.Deny();
            return null;
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
            server = new GameServer(serverConfig, OnClientConnecting);
            server.ObjectTracker.RegisterType<ExampleObj>();
            server.Start();
        }
        if (server != null && ImGui.Button("Stop server"))
        {
            server.Shutdown("Fuck off");
            server.Dispose();
            server = null;
        }
        if (server != null)
        {
            ImGui.LabelText("Server status", server.Status.ToString());
            ImGui.LabelText("Server connected client count", server.ConnectionsCount.ToString());

            if (ImGui.Button("Msg client"))
            {
                var m = server.CreateMessage();
                m.Write((byte) 255);
                m.Write("Hi from server!");
                server.SendMessage(m, server.ConnectionsNoAlloc, NetDeliveryMethod.ReliableSequenced, 0);
            }

            ImGui.BeginListBox("Connected players");

            foreach (var player in server.Players)
            {
                ImGui.Text(player.ToString());
            }

            ImGui.EndListBox();

            if (ImGui.Button("Spawn new object"))
            {
                var obj = new ExampleObj
                {
                    MyFloat = new Random().Next(0, 100)
                };
                obj.Spawn();
            }

            ImGui.BeginListBox("S.Objects");

            foreach (var o in server.ObjectTracker.GetAllTrackedObjects())
            {
                var obj = (ExampleObj) o;
                ImGui.DragFloat($"Obj[{obj.NetID}]", ref obj.MyFloat);
            }

            ImGui.EndListBox();
        }

        if (client == null && ImGui.Button("Start client"))
        {
            client = new GameClient(new NetPeerConfiguration("GAME"), GameServer.IsRunning);
            client.ObjectTracker?.RegisterType<ExampleObj>();
            client.Start();
            var msg = client.CreateMessage(128);
            msg.Write((byte) 123);
            msg.Write("James");
            client.Connect("localhost", PORT, msg);
            Log.Info("Started connect... ");
        }
        if (client != null && ImGui.Button("Stop client"))
        {
            client.Disconnect("Peace, I'm out.");
            client.Dispose();
            client = null;
        }
        if (client != null)
        {
            ImGui.LabelText("Client status", client.Status.ToString());
            ImGui.LabelText("Client connection status", client.ConnectionStatus.ToString());
            ImGui.LabelText("Connected to", client.ServerConnection?.RemoteEndPoint.ToString() ?? "null");
            ImGui.LabelText("RTT", client.ServerConnection?.AverageRoundtripTime.ToString() ?? "null");

            if (ImGui.Button("Msg server"))
            {
                var m = client.CreateMessage();
                m.Write((byte)255);
                m.Write("Hi from client!");
                client.SendMessage(m, NetDeliveryMethod.ReliableSequenced, 0);
            }

            if (client.ObjectTracker != null)
            {
                ImGui.BeginListBox("C.Objects");

                foreach (var o in client.ObjectTracker.GetAllTrackedObjects())
                {
                    var obj = (ExampleObj)o;
                    ImGui.LabelText($"Obj[{obj.NetID}]", obj.MyFloat.ToString(CultureInfo.InvariantCulture));
                }

                ImGui.EndListBox();
            }
        }

        ImGui.PlotLines("Server Bytes Out", ref bytesOut.GetRootItem(), bytesOut.Count, bytesOut.GetOffset());

        ImGui.End();
    }
}

internal partial class ExampleObj : NetObject
{
    [SyncVar]
    public float MyFloat;

    [ServerRPC]
    public void ExampleServerRPC(float increment)
    {
        MyFloat += increment;
    }
}