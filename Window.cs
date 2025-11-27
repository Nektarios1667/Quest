using SharpDX.Direct2D1.Effects;
using System.IO;
using System.Text;

namespace Quest;
public class Window : Game
{
    static readonly StringBuilder debugSb = new();
    // Devices and managers
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private GameManager gameManager;
    private PlayerManager playerManager;
    private OverlayManager overlayManager;
    private LevelManager levelManager;
    private MenuManager menuManager;
    public static readonly Matrix Scale = Matrix.CreateScale(Constants.ScreenScale.X, Constants.ScreenScale.Y, 1f);

    // Time
    private float delta;
    private Dictionary<string, double> frameTimes;

    // Textures
    public Texture2D CursorArrow { get; private set; }

    // Movements
    public int moveX;
    public int moveY;
    // Shaders
    public Effect Grayscale { get; private set; }
    public Effect Lighting { get; private set; }
    // Render targets
    private RenderTarget2D? ShaderTarget { get; set; }

    // Debug
    private static readonly Color[] colors = {
        Color.Purple, new(255, 128, 128), new(128, 255, 128), new(255, 255, 180), new(128, 255, 255),
        Color.Brown, Color.Gray, new(192, 128, 64), new(64, 128, 192), new(192, 192, 64),
        new(64, 192, 128), new(192, 64, 128), new(160, 80, 0), new(80, 160, 0), new(0, 160, 80),
        new(160, 0, 80), new(96, 96, 192), new(192, 96, 96), new(96, 192, 96), new(192, 192, 96),
        Color.DarkRed, Color.Cyan, Color.Magenta, Color.Orange, Color.Yellow, Color.Blue, Color.Red
    };
    private float debugUpdateTime;
    private float cacheDelta;
    public Window()
    {
        graphics = new GraphicsDeviceManager(this)
        {
            //PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
            //PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height,
            PreferredBackBufferWidth = Constants.ScreenResolution.X,
            PreferredBackBufferHeight = Constants.ScreenResolution.Y,
            IsFullScreen = false,
            SynchronizeWithVerticalRetrace = Constants.VSYNC,
            PreferHalfPixelOffset = false,
            GraphicsProfile = GraphicsProfile.HiDef,
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        IsFixedTimeStep = Constants.FPS != -1;
        if (IsFixedTimeStep)
            TargetElapsedTime = TimeSpan.FromSeconds(1d / Constants.FPS);
        Logger.System("Initialized game window object.");
    }

    protected override void Initialize()
    {
        // Defaults
        frameTimes = [];
        debugUpdateTime = 0;
        cacheDelta = 0f;

        Exiting += OnExiting;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new(GraphicsDevice);

        // State Manager
        StateManager.Mood = Mood.Calm;

        // Textures
        LoadTextures(Content);

        // Soundtracks
        SoundtrackManager.LoadSoundtracks(Content);

        // Managers
        playerManager = new();
        levelManager = new();
        overlayManager = new(levelManager, playerManager);
        gameManager = new(Content, spriteBatch, levelManager, overlayManager);
        menuManager = new(this, spriteBatch, Content, gameManager, playerManager);
        levelManager.LevelLoaded += _ => playerManager.CloseContainer();
        CommandManager.Init(this, gameManager, levelManager, playerManager);
        Pathfinder.Init(gameManager);
        StateManager.Init(gameManager);
        Logger.System("Initialized managers.");

        // Levels
        levelManager.ReadWorld(gameManager.UIManager, "islands");
        levelManager.ReadWorld(gameManager.UIManager, "house");
        menuManager.RefreshWorldList();
        Logger.System("Loaded levels.");

        // Shaders
        Lighting = Content.Load<Effect>("Shaders/Lighting");
        Lighting.Parameters["dim"].SetValue(Constants.NativeResolution.ToVector2());
        Lighting.Parameters["numLights"].SetValue(0);

        // Render Targets
        ShaderTarget = new(GraphicsDevice, Constants.NativeResolution.X, Constants.NativeResolution.Y);
        ShaderTarget = new(
            GraphicsDevice,
            width: Constants.NativeResolution.X,
            height: Constants.NativeResolution.Y,
            mipMap: false,
            preferredFormat: SurfaceFormat.Color,
            preferredDepthFormat: DepthFormat.None,
            preferredMultiSampleCount: 0,
            usage: RenderTargetUsage.DiscardContents
        );

        // Run test script - DELETEME after testing
        Quill.Interpreter.UpdateSymbols(gameManager, playerManager);
        string content = File.ReadAllText("Test.quill");
        _ = Quill.Interpreter.RunScriptAsync(content);

        // Other
        CursorArrow = Content.Load<Texture2D>("Images/Gui/CursorArrow");

        // Timer
        TimerManager.NewTimer("FrameTimeUpdate", 1, UpdateFrameTimes, int.MaxValue);

        // Final
        Logger.System("Game finished initializing.");
    }

    protected override void Update(GameTime gameTime)
    {
        DebugManager.Watch.Restart();

        // Exit
        if (InputManager.Hotkey(Keys.LeftControl, Keys.Escape)) Exit();

        // Delta
        delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Managers
        Quill.Interpreter.UpdateSymbols(gameManager, playerManager);
        InputManager.Update(this);
        DebugManager.Update();
        CameraManager.Update(delta);
        TimerManager.Update(gameManager);
        SoundtrackManager.Update();
        LightingManager.Update();

        gameManager.Update(delta);
        playerManager.Update(gameManager);
        levelManager.Update(gameManager);
        menuManager.Update(gameManager);
        overlayManager.Update(gameManager);

        // Console commands
        if (Constants.COMMANDS && InputManager.Hotkey(Keys.LeftControl, Keys.LeftShift, Keys.OemTilde))
            CommandManager.OpenCommandPrompt();
        // Spawn enemy
        if (Constants.COMMANDS && InputManager.Hotkey(Keys.LeftControl, Keys.LeftShift, Keys.E))
        {
            Enemy enemy = new(CameraManager.Camera.ToPoint() + InputManager.MousePosition - Constants.Middle);
            gameManager.LevelManager.Level.Enemies.Add(enemy);
            Logger.Log($"Spawned {enemy.Name} at {enemy.Location}");
        }

        // Final
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Magenta);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Scale);

        // Draw game
        levelManager.Draw(gameManager);
        playerManager.Draw(gameManager);

        // Draw overlays
        overlayManager.Draw(GraphicsDevice, gameManager, playerManager);
        menuManager.Draw();

        // Text info
        DebugManager.StartBenchmark("DebugTextDraw");
        if (DebugManager.TextInfo)
            DrawTextInfo();

        // Frame info
        if (DebugManager.FrameInfo)
            DrawFrameInfo();
        DebugManager.EndBenchmark("DebugTextDraw");

        // Frame bar
        DebugManager.StartBenchmark("FrameBarDraw");
        if (DebugManager.FrameBar)
            DrawFrameBar();
        DebugManager.EndBenchmark("FrameBarDraw");

        // Cursor
        DrawTexture(spriteBatch, TextureID.CursorArrow, InputManager.MousePosition);

        // Final
        spriteBatch.End();
        base.Draw(gameTime);
    }
    protected void OnExiting(object? sender, EventArgs args)
    {
        Logger.System("Exiting game.");

        if (StateManager.CurrentSave != "")
            StateManager.WriteKeyValueFile("continue", new() { { "lastSave", StateManager.CurrentSave } });

        Logger.System("Game exited successfully.");
    }
    // For cleaner code
    public void DrawFrameInfo()
    {
        float boxHeight = DebugManager.FrameTimes.Count * 20;
        FillRectangle(spriteBatch, new(Constants.NativeResolution.X - 200, 0, 200, (int)boxHeight), Color.Black * 0.8f);

        debugSb.Clear();
        foreach (var kv in frameTimes)
        {
            debugSb.Append(kv.Key);
            debugSb.Append(": ");
            debugSb.AppendFormat("{0:0.0}ms", kv.Value);
            debugSb.Append('\n');
        }

        spriteBatch.DrawString(Arial, debugSb.ToString(), new Vector2(Constants.NativeResolution.X - 190, 0), Color.White);
    }
    public void DrawTextInfo()
    {

        debugSb.Clear();
        debugSb.Append("FPS: ");
        debugSb.AppendFormat("{0:0.0}", cacheDelta != 0 ? 1f / cacheDelta : 0);
        debugSb.Append("\nGameTime: ");
        debugSb.AppendFormat("{0:0.00}", gameManager.GameTime);
        debugSb.Append("\nDayTime: ");
        debugSb.AppendFormat("{0:0.00}", gameManager.DayTime);
        debugSb.Append("\nTotalTime: ");
        debugSb.AppendFormat("{0:0.00}", gameManager.TotalTime);
        debugSb.Append("\nCamera: ");
        debugSb.AppendFormat("{0:0.0},{1:0.0}", CameraManager.Camera.X, CameraManager.Camera.Y);
        debugSb.Append("\nTile Below: ");
        debugSb.Append(playerManager.TileBelow == null ? "none" : playerManager.TileBelow.Type.ToString());
        debugSb.Append("\nCoord: ");
        debugSb.AppendFormat("{0:0.0},{1:0.0}", CameraManager.TileCoord.X, CameraManager.TileCoord.Y);
        debugSb.Append("\nLevel: ");
        debugSb.Append(levelManager.Level?.Name);
        debugSb.Append("\nInventory: ");
        debugSb.Append(playerManager.Inventory.Opened);
        debugSb.Append("\nGUI: ");
        debugSb.Append(overlayManager.Gui.Widgets.Count);
        debugSb.Append("\nMood: ");
        debugSb.Append(StateManager.Mood);
        debugSb.Append("\nMusic: ");
        debugSb.Append(SoundtrackManager.Playing?.File ?? "none");
        debugSb.Append("\nDaylight: ");
        debugSb.AppendFormat("{0:0}%", ColorTools.GetDaylightPercent(gameManager.DayTime));
        debugSb.Append("\nLighting: ");
        debugSb.AppendFormat("{0}/{1}", LightingManager.GetVisibleLights().Length, LightingManager.Lights.Count);
        debugSb.Append("\nWeather: ");
        debugSb.Append(StateManager.WeatherIntensity(gameManager.GameTime));
        debugSb.AppendFormat(" [{0:0.00}]", StateManager.WeatherNoiseValue(gameManager.GameTime));

        FillRectangle(spriteBatch, new(0, 0, 220, debugSb.ToString().Split('\n').Length * 20), Color.Black * 0.8f);
        spriteBatch.DrawString(Arial, debugSb.ToString(), new Vector2(10, 10), Color.White);
    }
    public void DrawFrameBar()
    {
        // Update info twice a second
        if (debugUpdateTime >= .5)
        {
            cacheDelta = delta;
            frameTimes = new Dictionary<string, double>(DebugManager.FrameTimes);
            debugUpdateTime = 0;
        }
        // Background
        FillRectangle(spriteBatch, new(Constants.NativeResolution.X - 320, Constants.NativeResolution.Y - frameTimes.Count * 20 - 50, 320, 1000), Color.Black * .8f);

        // Labels and bars
        int start = 0;
        int c = 0;
        FillRectangle(spriteBatch, new(Constants.NativeResolution.X - 310, Constants.NativeResolution.Y - 40, 300, 25), Color.White);
        foreach (KeyValuePair<string, double> process in frameTimes)
        {
            spriteBatch.DrawString(Arial, process.Key, new(Constants.NativeResolution.X - Arial.MeasureString(process.Key).X - 5, Constants.NativeResolution.Y - 20 * c - 60), colors[c]);
            FillRectangle(spriteBatch, new(Constants.NativeResolution.X - 310 + start, Constants.NativeResolution.Y - 40, (int)(process.Value / (cacheDelta * 1000) * 300), 25), colors[c]);
            start += (int)(process.Value / (cacheDelta * 1000)) * 300;
            c++;
        }
    }
    public void UpdateFrameTimes()
    {
        frameTimes.Clear();
        frameTimes = new(DebugManager.FrameTimes);
        cacheDelta = gameManager.DeltaTime;
    }
}
