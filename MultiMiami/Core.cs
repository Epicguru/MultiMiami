using ImGuiNET;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MM.Core;
using MM.Core.Structures;
using MM.DearImGui;
using MM.Define;
using MM.Define.Xml;
using MM.Logging;
using MM.Multiplayer;
using MonoGame.ImGui.Extensions;
using MultiMiami.Defs;
using MultiMiami.Defs.Parsers;
using MultiMiami.Maps;
using MultiMiami.Utility;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Xml;
using MM.Input;
using MM.Core.Atlas;

namespace MultiMiami;

public class Core : Game
{
    public static double MaxRunOnMainThreadTimeMS = 2.0;

    public static GraphicsDeviceManager GDM { get; private set; }
    public static GraphicsDevice GD { get; private set; }
    public static Camera2D Camera { get; private set; }
    public static HeartbeatComponent Heartbeats { get; private set; }

    private static readonly List<(Task task, Action<Task> callback)> trackedTasks = new List<(Task, Action<Task>)>(64);
    private static readonly Queue<Action> runOnMainThread = new Queue<Action>(64);

    public static Task TrackTask(Task task, Action<Task> onCompleted)
    {
        if (task == null)
        {
            Log.Error("Null task passed into TrackTask!");
            return null;
        }

        if (onCompleted == null)
        {
            Log.Error("OnCompleted callback passed into TrackTask is null!");
            return task;
        }

        lock (trackedTasks)
        {
            Debug.Assert(!trackedTasks.Contains((task, onCompleted)));
            trackedTasks.Add((task, onCompleted));
        }

        return task;
    }

    public static void RunOnMainThread(Action action)
    {
        if (action == null)
        {
            Log.Error("Called RunOnMainThread with null action.");
        }

        lock (runOnMainThread)
        {
            Debug.Assert(!runOnMainThread.Contains(action));
            runOnMainThread.Enqueue(action);
        }
    }

    private static void UpdatedThreadedJobs()
    {
        lock (trackedTasks)
        {
            for (int i = 0; i < trackedTasks.Count; i++)
            {
                var pair = trackedTasks[i];
                if (pair.task.IsCompleted)
                {
                    try
                    {
                        pair.callback?.Invoke(pair.task);
                    }
                    catch (Exception e)
                    {
                        Log.Error("Exception thrown during tracked task callback:", e);
                    }

                    trackedTasks.RemoveAt(i);
                    i--;
                }
            }
        }

        lock (runOnMainThread)
        {
            var timer = new Stopwatch();
            timer.Start();

            while (runOnMainThread.TryDequeue(out var item))
            {
                try
                {
                    item?.Invoke();
                }
                catch (Exception e)
                {
                    Log.Error("Exception thrown during RunOnMainThread callback:", e);
                }

                if (timer.Elapsed.TotalMilliseconds >= MaxRunOnMainThreadTimeMS)
                    break;
            }

            timer.Stop();
        }
    }

    public static SpriteAtlas Atlas;

    private SpriteBatch spr;
    private MMImGuiRenderer imGuiRenderer;
    private TileMap map;
    private float zoomTarget = 128f;
    private Vector2 startPos, startPosWorld;

    public Core()
    {
        GDM = new GraphicsDeviceManager(this);
        IsMouseVisible = true;
        IsFixedTimeStep = false; // Garbage :)
        GDM.GraphicsProfile = GraphicsProfile.HiDef;

        GDM.PreparingDeviceSettings += GDM_PreparingDeviceSettings;
        Window.ClientSizeChanged += WindowSizeChanged;
    }

    private static void WindowSizeChanged(object sender, EventArgs e)
    {
        if (Screen.IsFullscreen)
            return;

        Screen.SetSize(Screen.WindowSize.X, Screen.WindowSize.Y);
        Log.Trace($"Window resized, set resolution to {Screen.WindowSize}");
    }

    private static void GDM_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
    {
        e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 16;
    }

    protected override void Initialize()
    {
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

        MMExtensions.Init();
        DebugReadoutDrawer.RegisterAssembly(Assembly.GetExecutingAssembly());

        // This calls LoadContent.
        base.Initialize();
    }

    protected override void LoadContent()
    {
        base.LoadContent();

        Atlas = SpriteAtlas.FromPackedImages(GD, "./Content/PackedAtlas.png", "", Directory.EnumerateFiles("./Content/Textures", "*.png", SearchOption.AllDirectories));
        foreach (var spr in Atlas)
        {
            Log.Info(spr.ToString());
        }

        DefDebugger.OnWarning += Log.Warn;
        DefDebugger.OnError += Log.Error;
        DefDebugger.OnXmlParseError += (string message, in XmlParseContext _, Exception e) => Log.Error(message, e);

        var config = new DefLoadConfig();
        DefDatabase.StartLoading(config);
        DefDatabase.Loader.AddParser(new SpriteParser());
        DefDatabase.Loader.AddParser(new TextureParser());

        foreach (var file in Directory.EnumerateFiles("./Content/Defs", "*.xml", SearchOption.AllDirectories))
        {
            var doc = new XmlDocument();
            doc.Load(file);
            DefDatabase.AddDefDocument(doc, new FileInfo(file).FullName);
        }

        DefDatabase.FinishLoading();

        map = new TileMap(64, 32, 32);
    }

    protected override void UnloadContent()
    {
        base.UnloadContent();

        MMExtensions.Dispose();

        spr.Dispose();
    }

    protected override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        Time.Update();
        Input.Update(Camera);
        UpdatedThreadedJobs();

        if (Input.MouseScrollDelta > 0)
            zoomTarget *= 1.1f;
        else if (Input.MouseScrollDelta < 0)
            zoomTarget /= 1.1f;

        Camera.Scale = Mathf.Lerp(Camera.Scale, zoomTarget, 0.1f);

        if (Input.IsJustDown(MouseButton.Middle))
        {
            startPos = Input.ScreenMousePosition;
            startPosWorld = Camera.Position;
        }

        if (Input.IsPressed(MouseButton.Middle))
        {
            var delta = Input.ScreenMousePosition - startPos;
            var offset = Camera.GetWorldVector(delta);
            var finalPos = startPosWorld - offset;
            Camera.Position = finalPos;
        }

        Screen.WindowTitle = $"MultiMiami - {Screen.UpdatesPerSecond} UPS, {Screen.FramesPerSecond} FPS";

        map.Update();
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

    private int i;

    protected override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);

        Time.Draw();

        if (Input.IsJustDown(Keys.Space))
        {
            i++;
            foreach (var c in map.AllChunks.OrderBy(c => (c.CenterPosition - Camera.Position).Length()))
            { 
                if (i % 2 == 0)
                    c.Unload();
                else
                    c.Load();
            }
        }

        // Clear screen.
        GraphicsDevice.Clear(Color.AliceBlue);

        spr.Begin(new SpriteBatchArgs
        {
            BlendState = BlendState.NonPremultiplied,
            Matrix = Camera.GetMatrix(Screen.ScreenSize.ToVector2()),
            SamplerState = Camera.Scale >= TileMap.TILE_SIZE ? SamplerState.PointClamp : SamplerState.LinearClamp
        });

        // Draw here!
        map.Draw(spr);

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

        if (ImGui.Button("Load all chunks"))
        {
            foreach (var c in map.AllChunks)
                c.Load();
        }

        if (ImGui.Button("Unload all chunks"))
        {
            foreach (var c in map.AllChunks)
                c.Unload();
        }

        ImGui.Checkbox("Draw net window", ref drawNet);

        if (drawNet)
            DrawNetWindow();
    }

    private bool drawNet;
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
                server.SendMessage(m, server.Connections, NetDeliveryMethod.ReliableSequenced, 0);
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

public partial class ExampleObj : NetObject
{
    [SyncVar]
    public float MyFloat;

    [ServerRPC]
    public void ExampleServerRPC(float increment)
    {
        MyFloat += increment;
    }
}