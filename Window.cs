using Quest.Interaction;
using System.Diagnostics;
using System.Text;

namespace Quest;

public class Window : Game, IAdjustableWindow
{
    readonly StringBuilder infoSb = new();
    readonly StringBuilder memoryDebugSb = new();
    readonly StringBuilder frameTimesSb = new();
    // Devices and managers
    private readonly GraphicsDeviceManager graphics = null!;
    private SpriteBatch spriteBatch = null!;
    private GameManager gameManager = null!;
    private PlayerManager playerManager = null!;
    private OverlayManager overlayManager = null!;
    private LevelManager levelManager = null!;
    private MenuManager menuManager = null!;
    private static Matrix Scale = Matrix.CreateScale(SettingsManager.ScreenScale.X, SettingsManager.ScreenScale.Y, 1f);
    public RenderTarget2D Render = null!;

    // Time
    private float delta;
    private Dictionary<string, double> frameTimes = [];

    // Textures
    public Texture2D CursorArrow { get; private set; } = null!;
    // Shaders
    public Effect Grading = null!;

    // Movements
    public int moveX;
    public int moveY;

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
            PreferredBackBufferWidth = SettingsManager.ScreenResolution.X,
            PreferredBackBufferHeight = SettingsManager.ScreenResolution.Y,
            IsFullScreen = false,
            SynchronizeWithVerticalRetrace = SettingsManager.VSYNC,
            PreferHalfPixelOffset = false,
            GraphicsProfile = GraphicsProfile.HiDef,
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        IsFixedTimeStep = SettingsManager.FPS != 0;
        if (IsFixedTimeStep)
            TargetElapsedTime = TimeSpan.FromSeconds(1d / SettingsManager.FPS);
        Logger.System("Initialized game window object.");
    }
    public void SetVsync(bool enabled)
    {
        graphics.SynchronizeWithVerticalRetrace = enabled;
        graphics.ApplyChanges();
    }
    public void SetResolution(int width, int height)
    {
        graphics.PreferredBackBufferWidth = width;
        graphics.PreferredBackBufferHeight = height;

        Scale = Matrix.CreateScale(SettingsManager.ScreenScale.X, SettingsManager.ScreenScale.Y, 1f);

        Render = new RenderTarget2D(
            GraphicsDevice,
            SettingsManager.ScreenResolution.X, SettingsManager.ScreenResolution.Y,
            false,
            SurfaceFormat.Color,
            DepthFormat.None
        );

        graphics.ApplyChanges();
    }
    public void SetFullscreen(bool enabled)
    {
        graphics.IsFullScreen = enabled;
        graphics.ApplyChanges();
    }
    protected override void Initialize()
    {
        // Defaults
        frameTimes = [];
        debugUpdateTime = 0;
        cacheDelta = 0f;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new(GraphicsDevice);

        // Shaders
        Grading = Content.Load<Effect>("Shaders/Grading");
        Grading.Parameters["Saturation"].SetValue(1f);
        Grading.Parameters["Contrast"].SetValue(1f);
        Grading.Parameters["Tint"].SetValue(new Vector3(1, 1, 1));

        // State Manager
        StateManager.Mood = Mood.Calm;

        // Textures
        LoadTextures(Content);

        // Soundtracks
        SoundtrackManager.LoadSoundtracks(Content);

        // Managers
        SoundManager.Init(Content);
        levelManager = new();
        UserInterface.Init(spriteBatch, levelManager);
        playerManager = new();
        overlayManager = new(playerManager);
        gameManager = new(Content, spriteBatch, levelManager, overlayManager, Grading);
        menuManager = new(this, spriteBatch, Content, gameManager, playerManager);
        CommandManager.Init(this, gameManager, levelManager, playerManager);

        Logger.System("Initialized managers.");

        // Levels
        menuManager.RefreshWorldList();
        Logger.System("Loaded levels.");

        // Render Targets
        Render = new RenderTarget2D(
            GraphicsDevice,
            SettingsManager.ScreenResolution.X, SettingsManager.ScreenResolution.Y,
            false,
            SurfaceFormat.Color,
            DepthFormat.None
        );

        Quill.Interpreter.UpdateSymbols(gameManager, playerManager);

        // Other
        CursorArrow = Content.Load<Texture2D>("Images/Gui/CursorArrow");

        // Timer
        TimerManager.NewTimer("FrameTimeUpdate", 0.5f, UpdateFrameTimes, int.MaxValue);

        // Final
        Logger.System("Game finished initializing.");
    }
    protected override void OnExiting(object sender, ExitingEventArgs args)
    {
        DebugManager.CloseDebugWindow();
        base.OnExiting(sender, args);
    }

    protected override void Update(GameTime gameTime)
    {
        DebugManager.Watch.Restart();

        // Exit
        if (InputManager.KeyPressed(Keys.LeftAlt) && InputManager.KeyPressed(Keys.Escape)) Exit();

        // Delta
        delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Managers
        Quill.Interpreter.Update(gameManager, playerManager);
        InputManager.Update(this);
        DebugManager.Update(infoSb.ToString().Replace("\n", "\r\n"), memoryDebugSb.ToString().Split("\n"));
        CameraManager.Update(delta);
        TimerManager.Update(gameManager);
        SoundtrackManager.Update();
        LightingManager.Update();

        gameManager.Update(delta);
        playerManager.Update(gameManager);
        levelManager.Update(gameManager);
        menuManager.Update(gameManager);
        overlayManager.Update(gameManager, playerManager);

        // Console commands
        if (Constants.COMMANDS && InputManager.BindPressed(InputAction.OpenCommandWindow))
            CommandManager.OpenCommandPrompt();

        // Final
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(Render);
        GraphicsDevice.Clear(Color.Magenta);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, transformMatrix: Scale);

        // Draw game
        levelManager.Draw(gameManager);
        playerManager.Draw(gameManager);

        // Draw overlays
        overlayManager.Draw(GraphicsDevice, gameManager, playerManager);
        playerManager.StatusArea.Draw(spriteBatch);
        menuManager.Draw(spriteBatch);

        // Text info
        DebugManager.StartBenchmark("DebugText");
        DrawTextInfo();

        // Program info
        DrawProgramInfo(memoryDebugSb, spriteBatch);

        // Frame info
        DrawFrameInfo();
        DebugManager.EndBenchmark("DebugText");

        // Frame bar
        DebugManager.StartBenchmark("FrameBar");
        DrawFrameBar();
        DebugManager.EndBenchmark("FrameBar");

        // Cursor
        DrawTexture(spriteBatch, TextureID.CursorArrow, InputManager.MousePosition);

        // Final
        spriteBatch.End();
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Transparent);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, effect: Grading);
        spriteBatch.Draw(Render, Vector2.Zero, Color.White);
        spriteBatch.End();

        base.Draw(gameTime);
    }
    // For cleaner code
    public void DrawFrameInfo()
    {
        float boxHeight = DebugManager.FrameTimes.Count * 20;

        frameTimesSb.Clear();
        foreach (var kv in frameTimes)
        {
            frameTimesSb.Append(kv.Key);
            frameTimesSb.Append(": ");
            frameTimesSb.AppendFormat("{0:0.0}ms", kv.Value);
            frameTimesSb.Append('\n');
        }

        // Draw
        if (!DebugManager.FrameInfo) return;

        FillRectangle(spriteBatch, new(Constants.NativeResolution.X - 200, 0, 200, (int)boxHeight), Color.Black * 0.8f);
        spriteBatch.DrawString(Arial, frameTimesSb.ToString(), new Vector2(Constants.NativeResolution.X - 190, 0), Color.White);
    }
    public static void DrawProgramInfo(StringBuilder memoryDebugSb, SpriteBatch spriteBatch)
    {
        if (TimerManager.IsCompleteOrMissing("UpdateProgramInfo"))
        {
            memoryDebugSb.Clear();
            var process = Process.GetCurrentProcess();

            memoryDebugSb.Append("Memory: ");
            memoryDebugSb.AppendFormat("{0:0.0} MB", process.WorkingSet64 / 1024.0 / 1024.0);
            memoryDebugSb.Append("\nThreads: ");
            memoryDebugSb.Append(process.Threads.Count);
            memoryDebugSb.Append("\nHandles: ");
            memoryDebugSb.Append(process.HandleCount);
            memoryDebugSb.Append("\nUptime: ");
            memoryDebugSb.AppendFormat("{0:hh\\:mm\\:ss}", DateTime.Now - process.StartTime);
            memoryDebugSb.Append("\nGC Memory: ");
            memoryDebugSb.AppendFormat("{0:0.0} MB", GC.GetTotalMemory(false) / 1024.0 / 1024.0);
            memoryDebugSb.Append("\nGC Gen0: ");
            memoryDebugSb.Append(GC.CollectionCount(0));
            memoryDebugSb.Append("\nGC Gen1: ");
            memoryDebugSb.Append(GC.CollectionCount(1));
            memoryDebugSb.Append("\nGC Gen2: ");
            memoryDebugSb.Append(GC.CollectionCount(2));

            TimerManager.SetTimer("UpdateProgramInfo", 1f, null);
        }

        if (!DebugManager.ProgramInfo) return;

        int height = memoryDebugSb.ToString().Split('\n').Length * 20;
        FillRectangle(spriteBatch, new(0, Constants.NativeResolution.Y - height, 220, height), Color.Black * 0.8f);
        spriteBatch.DrawString(Arial, memoryDebugSb.ToString(), new(5, Constants.NativeResolution.Y - height + 5), Color.White);
    }
    public void DrawTextInfo()
    {

        infoSb.Clear();
        infoSb.Append("FPS: ");
        infoSb.AppendFormat("{0:0.0}", cacheDelta != 0 ? 1f / cacheDelta : 0);
        infoSb.Append("\nGameTime: ");
        infoSb.AppendFormat("{0:0.00}", GameManager.GameTime);
        infoSb.Append("\nDayTime: ");
        infoSb.AppendFormat("{0:0.00}", gameManager.DayTime);
        infoSb.Append("\nTotalTime: ");
        infoSb.AppendFormat("{0:0.00}", GameManager.GameTime);
        infoSb.Append("\nCamera: ");
        infoSb.AppendFormat("{0:0.0},{1:0.0}", CameraManager.Camera.X, CameraManager.Camera.Y);
        infoSb.Append("\nTile Below: ");
        infoSb.Append(playerManager.TileBelow == null ? "none" : playerManager.TileBelow.Type.Texture.ToString());
        infoSb.Append("\nCoord: ");
        infoSb.AppendFormat("{0:0.0},{1:0.0}", CameraManager.TileCoord.X, CameraManager.TileCoord.Y);
        infoSb.Append("\nLevel: ");
        infoSb.Append(levelManager.Level?.Path);
        infoSb.Append("\nInventory: ");
        infoSb.Append(playerManager.InventoryOpen);
        infoSb.Append("\nGUI: ");
        infoSb.Append(overlayManager.Gui.Widgets.Count);
        infoSb.Append("\nMood: ");
        infoSb.Append(StateManager.Mood);
        infoSb.Append("\nMusic: ");
        infoSb.Append(SoundtrackManager.Playing.ToString() ?? "none");
        infoSb.Append("\nDaylight: ");
        infoSb.AppendFormat("{0:0}%", ColorTools.GetDaylightPercent(gameManager.DayTime));
        infoSb.Append("\nLighting: ");
        infoSb.AppendFormat("{0}", LightingManager.Lights.Count);
        infoSb.Append("\nWeather: ");
        infoSb.Append(StateManager.WeatherIntensity(GameManager.GameTime));
        infoSb.AppendFormat(" [{0:0.00}]", StateManager.WeatherValue(GameManager.GameTime));
        infoSb.Append("\nUIDs: ");
        infoSb.AppendFormat("L:{0}/{1} E:{2}/{3} I:{4}/{5} P:{6}/{7}", UIDManager.InUse(UIDCategory.Loot), UIDManager.Counter(UIDCategory.Loot), UIDManager.InUse(UIDCategory.Enemies), UIDManager.Counter(UIDCategory.Enemies), UIDManager.InUse(UIDCategory.Items), UIDManager.Counter(UIDCategory.Items), UIDManager.InUse(UIDCategory.Projectiles), UIDManager.Counter(UIDCategory.Projectiles));
        infoSb.Append("\nSave: ");
        infoSb.Append(StateManager.CurrentSave);
        infoSb.Append("\nCurrent Luxel: ");
        infoSb.Append(CameraManager.Camera.ToPoint() / Constants.TileSize.Scaled(0.5f));
        infoSb.Append("\nSoundtrack: ");
        infoSb.Append(SoundtrackManager.Playing);
        infoSb.Append("\nQuill: ");
        foreach (var inst in Quill.Interpreter.GetQuillInstances())
            infoSb.Append($"\n  {inst.Script.Name} | @{inst.L:000} | C:{inst.Callbacks.Count:0} | Sc:{(inst.Scopes.TryPeek(out string? sc) ? sc : "GLOBAL")}");

        // Drawing  
        if (!DebugManager.TextInfo) return;

        FillRectangle(spriteBatch, new(0, 0, 220, infoSb.ToString().Split('\n').Length * 20), Color.Black * 0.8f);
        spriteBatch.DrawString(Arial, infoSb.ToString(), new Vector2(10, 10), Color.White);
    }
    public void DrawFrameBar()
    {
        if (!DebugManager.FrameBar) return;

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
        cacheDelta = GameManager.DeltaTime;
    }
}
