using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace MM.Core;

public static class Screen
{
    /// <summary>
    /// The size, in pixels, of the logical screen (the back buffer).
    /// See also <see cref="WindowSize"/> and <see cref="SetSize(int, int)"/>.
    /// </summary>
    public static Point ScreenSize => new Point(gdm.PreferredBackBufferWidth, gdm.PreferredBackBufferHeight);

    /// <summary>
    /// The size, in pixels, of the game window.
    /// This is normally the same value as <see cref="ScreenSize"/>.
    /// </summary>
    public static Point WindowSize => new Point(game.Window.ClientBounds.Width, game.Window.ClientBounds.Height);

    /// <summary>
    /// The fullscreen resolution of the display that the game is running on.
    /// </summary>
    public static Point DisplayResolution => new Point(game.GraphicsDevice.Adapter.CurrentDisplayMode.Width, game.GraphicsDevice.Adapter.CurrentDisplayMode.Height);

    /// <summary>
    /// The user-facing title of the game window.
    /// </summary>
    public static string WindowTitle
    {
        get => game.Window.Title;
        set => game.Window.Title = value;
    }

    /// <summary>
    /// Can the game window be resized?
    /// </summary>
    public static bool Resizable
    {
        get => game.Window.AllowUserResizing;
        set => game.Window.AllowUserResizing = value;
    }

    /// <summary>
    /// Is the game rendering in fullscreen mode?
    /// </summary>
    public static bool IsFullscreen
    {
        get => gdm.IsFullScreen;
        set
        {
            if (value != IsFullscreen)
                SetFullscreen(value);
        }
    }

    /// <summary>
    /// Should hardware-mode switching be used when enabling fullscreen?
    /// </summary>
    public static bool HardwareModeSwitch
    {
        get => gdm.HardwareModeSwitch;
        set => gdm.HardwareModeSwitch = value;
    }

    /// <summary>
    /// Is this Screen class initialized? Call Init() to initialize, before using any other functionality
    /// of this class.
    /// </summary>
    public static bool IsInitialized { get; private set; }

    /// <summary>
    /// The number of displayed frames per second.
    /// </summary>
    public static int FramesPerSecond => sensor.DrawsPerSecond;

    /// <summary>
    /// The number of updates per second.
    /// </summary>
    public static int UpdatesPerSecond => sensor.UpdatesPerSecond;

    /// <summary>
    /// The number of stable ticks per second. See <see cref="IStableTicker"/>.
    /// </summary>
    public static int StableTicksPerSecond => sensor.StableTicksPerSecond;

    /// <summary>
    /// The presentation interval for the game window. See also <see cref="VSyncEnabled"/>.
    /// </summary>
    public static PresentInterval PresentationInterval
    {
        get => game.GraphicsDevice.PresentationParameters.PresentationInterval;
        set
        {
            if (PresentationInterval != value)
            {
                game.GraphicsDevice.PresentationParameters.PresentationInterval = value;
                gdm.ApplyChanges();
            }
        }
    }

    /// <summary>
    /// Is vsync enabled?
    /// </summary>
    public static bool VSyncEnabled
    {
        get => gdm.SynchronizeWithVerticalRetrace;
        set
        {
            if (VSyncEnabled != value)
            {
                gdm.SynchronizeWithVerticalRetrace = value;
                gdm.ApplyChanges();
            }
        }
    }

    /// <summary>
    /// Is Multi Sample Anti Aliasing enabled?
    /// Even if it is enabled, it may not actually be active if the hardware does not support it...
    /// See <see cref="MSAASampleCount"/>.
    /// </summary>
    public static bool IsMSAAEnabled
    {
        get => gdm.PreferMultiSampling;
        set
        {
            if (value != IsMSAAEnabled)
            {
                gdm.PreferMultiSampling = value;
                gdm.ApplyChanges();
            }
        }
    }

    /// <summary>
    /// The number of sample to be used if <see cref="IsMSAAEnabled"/> is true.
    /// Not any value is valid - a value of 0 is the same as disabling MSAA, and values larger than 16 will often not work,
    /// it depends on the current hardware. There is currently no way to enumerate the allowed values.
    /// Generally, multiples of 2 up to 16 i.e. (1, 2, 4, 8, 16) are valid.
    /// </summary>
    public static int MSAASampleCount
    {
        get => game.GraphicsDevice.PresentationParameters.MultiSampleCount;
        set => game.GraphicsDevice.PresentationParameters.MultiSampleCount = value;
    }

    private static Game game;
    private static GraphicsDeviceManager gdm;
    private static Point resBeforeFullscreen;
    private static SensorComp sensor;

    public static void Init(Game game, GraphicsDeviceManager graphicsDeviceManager)
    {
        IsInitialized = true;
        Screen.game = game ?? throw new ArgumentNullException(nameof(game));
        gdm = graphicsDeviceManager ?? throw new ArgumentNullException(nameof(graphicsDeviceManager));

        sensor = new SensorComp(game);
        game.Components.Add(sensor);
    }

    public static void SetFullscreen(bool fullscreen, bool changeResolution = true)
    {
        gdm.IsFullScreen = fullscreen;

        if (changeResolution)
        {
            if (fullscreen)
            {
                resBeforeFullscreen = ScreenSize;
                var res = DisplayResolution;
                gdm.PreferredBackBufferWidth = res.X;
                gdm.PreferredBackBufferHeight = res.Y;
            }
            else if (resBeforeFullscreen != Point.Zero)
            {
                gdm.PreferredBackBufferWidth = resBeforeFullscreen.X;
                gdm.PreferredBackBufferHeight = resBeforeFullscreen.Y;
            }
        }

        gdm.ApplyChanges();
    }

    public static void SetSize(int width, int height)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0)
            throw new ArgumentOutOfRangeException(nameof(height));

        if (gdm.PreferredBackBufferWidth == width && gdm.PreferredBackBufferHeight == height)
            return;

        gdm.PreferredBackBufferWidth = width;
        gdm.PreferredBackBufferHeight = height;
        gdm.ApplyChanges();
    }

    private class SensorComp : GameComponent, IDrawable
    {
        public const int MAX_TICKS_PER_UPDATE = 3;

        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;

        public int StableTicksPerSecond { get; private set; }
        public int UpdatesPerSecond { get; private set; }
        public int DrawsPerSecond { get; private set; }

        public int DrawOrder => 0;
        public bool Visible => true;

        private readonly Stopwatch timer = new Stopwatch();
        private readonly Stopwatch tickerTimer = new Stopwatch();
        private readonly IStableTicker ticker;
        private double tickerAccumulator = 0; 
        private int updates;
        private int draws;
        private int stableTicks;
        private int lastSecond;

        public SensorComp(Game game) : base(game)
        {
            timer.Start();
            tickerTimer.Start();
            ticker = game as IStableTicker;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (ticker != null)
                UpdateTicker();

            if ((int)timer.Elapsed.TotalSeconds != lastSecond)
            {
                lastSecond = (int) timer.Elapsed.TotalSeconds;
                UpdatesPerSecond = updates;
                DrawsPerSecond = draws;
                StableTicksPerSecond = stableTicks;
                updates = 0;
                draws = 0;
                stableTicks = 0;
            }

            updates++;
        }

        private void UpdateTicker()
        {
            tickerAccumulator += tickerTimer.Elapsed.TotalSeconds;
            tickerTimer.Restart();

            double timePerTick = 1.0 / ticker.TargetTickRate;
            for (int i = 0; i < MAX_TICKS_PER_UPDATE; i++)
            {
                if (tickerAccumulator >= timePerTick)
                {
                    tickerAccumulator -= timePerTick;
                    stableTicks++;
                    ticker.StableTick();
                }
                else
                {
                    break;
                }
            }
        }

        public void Draw(GameTime gameTime)
        {
            draws++;
        }
    }
}
