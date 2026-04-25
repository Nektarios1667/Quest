using Quest.Editor;
using Quest.Interaction;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;

namespace Quest;
public class Window : Game
{
    readonly StringBuilder debugSb = new();
    readonly StringBuilder programDebugSb = new();
    // Devices and managers
    private readonly GraphicsDeviceManager graphics = null!;
    private SpriteBatch spriteBatch = null!;
    private GameManager gameManager = null!;
    private PlayerManager playerManager = null!;
    private OverlayManager overlayManager = null!;
    private LevelManager levelManager = null!;
    private MenuManager menuManager = null!;
    public static readonly Matrix Scale = Matrix.CreateScale(Constants.ScreenScale.X, Constants.ScreenScale.Y, 1f);
    public RenderTarget2D Render = null!;

    // Time
    private float delta;
    private Dictionary<string, double> frameTimes = [];

    // Textures
    public Texture2D CursorArrow { get; private set; } = null!;
    // Shaders
    public Effect Grading = null!;
    public Effect Grayscale = null!;

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
        SoundManager.Init(Content);
        levelManager = new();
        UserInterface.Init(spriteBatch, levelManager);
        playerManager = new();
        overlayManager = new(playerManager);
        gameManager = new(Content, spriteBatch, levelManager, overlayManager);
        menuManager = new(this, spriteBatch, Content, gameManager, playerManager);
        CommandManager.Init(this, gameManager, levelManager, playerManager);

        Logger.System("Initialized managers.");

        // Levels
        menuManager.RefreshWorldList();
        Logger.System("Loaded levels.");

        // Shaders
        Grading = Content.Load<Effect>("Shaders/Grading");
        Grading.Parameters["Saturation"].SetValue(1f);
        Grading.Parameters["Contrast"].SetValue(1f);
        Grading.Parameters["Tint"].SetValue(new Vector3(1, 1, 1));

        Grayscale = Content.Load<Effect>("Shaders/Grayscale");

        // Render Targets
        Render = new RenderTarget2D(
            GraphicsDevice,
            Constants.ScreenResolution.X, Constants.ScreenResolution.Y,
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

    protected override void Update(GameTime gameTime)
    {
        DebugManager.Watch.Restart();

        // Exit
        if (InputManager.BindPressed(InputAction.Quit)) Exit();

        // Delta
        delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Managers
        Quill.Interpreter.Update(gameManager, playerManager);
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
        menuManager.Draw();

        // Text info
        DebugManager.StartBenchmark("DebugTextDraw");
        if (DebugManager.TextInfo)
            DrawTextInfo();

        // Program info
        if (DebugManager.ProgramInfo)
            DrawProgramInfo(programDebugSb, spriteBatch);

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
    public static void DrawProgramInfo(StringBuilder programDebugSb, SpriteBatch spriteBatch)
    {
        if (TimerManager.IsCompleteOrMissing("UpdateProgramInfo"))
        {
            programDebugSb.Clear();
            var process = Process.GetCurrentProcess();

            programDebugSb.Append("Memory: ");
            programDebugSb.AppendFormat("{0:0.0} MB", process.WorkingSet64 / 1024.0 / 1024.0);
            programDebugSb.Append("\nThreads: ");
            programDebugSb.Append(process.Threads.Count);
            programDebugSb.Append("\nHandles: ");
            programDebugSb.Append(process.HandleCount);
            programDebugSb.Append("\nUptime: ");
            programDebugSb.AppendFormat("{0:hh\\:mm\\:ss}", DateTime.Now - process.StartTime);
            programDebugSb.Append("\nGC Memory: ");
            programDebugSb.AppendFormat("{0:0.0} MB", GC.GetTotalMemory(false) / 1024.0 / 1024.0);
            programDebugSb.Append("\nGC Gen0: ");
            programDebugSb.Append(GC.CollectionCount(0));
            programDebugSb.Append("\nGC Gen1: ");
            programDebugSb.Append(GC.CollectionCount(1));
            programDebugSb.Append("\nGC Gen2: ");
            programDebugSb.Append(GC.CollectionCount(2));

            TimerManager.SetTimer("UpdateProgramInfo", 1f, null);
        }

        int height = programDebugSb.ToString().Split('\n').Length * 20;
        FillRectangle(spriteBatch, new(0, Constants.NativeResolution.Y - height, 220, height), Color.Black * 0.8f);
        spriteBatch.DrawString(Arial, programDebugSb.ToString(), new(5, Constants.NativeResolution.Y - height + 5), Color.White);
    }
    public void DrawTextInfo()
    {

        debugSb.Clear();
        debugSb.Append("FPS: ");
        debugSb.AppendFormat("{0:0.0}", cacheDelta != 0 ? 1f / cacheDelta : 0);
        debugSb.Append("\nGameTime: ");
        debugSb.AppendFormat("{0:0.00}", GameManager.GameTime);
        debugSb.Append("\nDayTime: ");
        debugSb.AppendFormat("{0:0.00}", gameManager.DayTime);
        debugSb.Append("\nTotalTime: ");
        debugSb.AppendFormat("{0:0.00}", GameManager.GameTime);
        debugSb.Append("\nCamera: ");
        debugSb.AppendFormat("{0:0.0},{1:0.0}", CameraManager.Camera.X, CameraManager.Camera.Y);
        debugSb.Append("\nTile Below: ");
        debugSb.Append(playerManager.TileBelow == null ? "none" : playerManager.TileBelow.Type.Texture.ToString());
        debugSb.Append("\nCoord: ");
        debugSb.AppendFormat("{0:0.0},{1:0.0}", CameraManager.TileCoord.X, CameraManager.TileCoord.Y);
        debugSb.Append("\nLevel: ");
        debugSb.Append(levelManager.Level?.Path);
        debugSb.Append("\nInventory: ");
        debugSb.Append(playerManager.InventoryOpen);
        debugSb.Append("\nGUI: ");
        debugSb.Append(overlayManager.Gui.Widgets.Count);
        debugSb.Append("\nMood: ");
        debugSb.Append(StateManager.Mood);
        debugSb.Append("\nMusic: ");
        debugSb.Append(SoundtrackManager.Playing.ToString() ?? "none");
        debugSb.Append("\nDaylight: ");
        debugSb.AppendFormat("{0:0}%", ColorTools.GetDaylightPercent(gameManager.DayTime));
        debugSb.Append("\nLighting: ");
        debugSb.AppendFormat("{0}", LightingManager.Lights.Count);
        debugSb.Append("\nWeather: ");
        debugSb.Append(StateManager.WeatherIntensity(GameManager.GameTime));
        debugSb.AppendFormat(" [{0:0.00}]", StateManager.WeatherValue(GameManager.GameTime));
        debugSb.Append("\nUIDs: ");
        debugSb.AppendFormat("L:{0}/{1} E:{2}/{3} I:{4}/{5} P:{6}/{7}", UIDManager.InUse(UIDCategory.Loot), UIDManager.Counter(UIDCategory.Loot), UIDManager.InUse(UIDCategory.Enemies), UIDManager.Counter(UIDCategory.Enemies), UIDManager.InUse(UIDCategory.Items), UIDManager.Counter(UIDCategory.Items), UIDManager.InUse(UIDCategory.Projectiles), UIDManager.Counter(UIDCategory.Projectiles));
        debugSb.Append("\nSave: ");
        debugSb.Append(StateManager.CurrentSave);
        debugSb.Append("\nCurrent Luxel: ");
        debugSb.Append(CameraManager.Camera.ToPoint() / Constants.TileSize.Scaled(0.5f));
        debugSb.Append("\nSoundtrack: ");
        debugSb.Append(SoundtrackManager.Playing);
        debugSb.Append("\nQuill: ");
        foreach (var inst in Quill.Interpreter.GetQuillInstances())
            debugSb.Append($"\n  {inst.Script.Name} | @{inst.L:000} | C:{inst.Callbacks.Count:0} | Sc:{(inst.Scopes.TryPeek(out string? sc) ? sc : "GLOBAL")}");

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
        cacheDelta = GameManager.DeltaTime;
    }
}
