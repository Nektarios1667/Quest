using System.Text;

// TODO Player not being drawn, dialog box not being drawn, update minimap on level change, inventory not being drawn

namespace Quest;
public class Window : Game
{
    static readonly StringBuilder debugSb = new();
    // Devices and managers
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private GameManager gameManager;
    private PlayerManager playerManager;
    private UIManager uiManager;
    private LevelManager levelManager;
    private MenuManager menuManager;
    private EnemyManager enemyManager;

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
    public Effect Light { get; private set; }
    // Render targets
    public RenderTarget2D? Minimap { get; set; }

    // Debug
    private static readonly Color[] colors = {
        Color.Purple, new(255, 128, 128), new(128, 255, 128), new(255, 255, 180), new(128, 255, 255),
        Color.Brown, Color.Gray, new(192, 128, 64), new(64, 128, 192), new(192, 192, 64),
        new(64, 192, 128), new(192, 64, 128), new(160, 80, 0), new(80, 160, 0), new(0, 160, 80),
        new(160, 0, 80), new(96, 96, 192), new(192, 96, 96), new(96, 192, 96), new(192, 192, 96)
    };
    private float debugUpdateTime;
    private float cacheDelta;
    public Window()
    {
        graphics = new GraphicsDeviceManager(this)
        {
            //PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
            //PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height,
            PreferredBackBufferWidth = Constants.Window.X,
            PreferredBackBufferHeight = Constants.Window.Y,
            IsFullScreen = false,
            SynchronizeWithVerticalRetrace = Constants.VSYNC,
            PreferHalfPixelOffset = false,
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        IsFixedTimeStep = false;
        Logger.System("Initialized window object.");
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
        playerManager = new();
        uiManager = new();
        levelManager = new();
        menuManager = new();
        gameManager = new(Content, spriteBatch, playerManager.Inventory, levelManager, uiManager);
        enemyManager = new();
        CommandManager.Init(this, gameManager, levelManager, playerManager);
        Logger.System("Initialized managers.");

        // Levels
        levelManager.ReadLevel(gameManager.UIManager, "island_house");
        levelManager.ReadLevel(gameManager.UIManager, "island_house_basement");
        levelManager.LoadLevel(gameManager, 0);
        Logger.System("Loaded levels.");

        // Shaders
        Grayscale = Content.Load<Effect>("Shaders/Grayscale");

        // Other
        CursorArrow = Content.Load<Texture2D>("Images/Gui/CursorArrow");

        // Timer
        TimerManager.NewTimer("frameTimeUpdate", 1, UpdateFrameTimes, int.MaxValue);
    }

    protected override void Update(GameTime gameTime)
    {
        DebugManager.Watch.Restart();

        // Exit
        if (InputManager.KeyDown(Keys.Escape)) Exit();

        // Delta
        delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Managers
        InputManager.Update();
        DebugManager.Update();
        CameraManager.Update(delta);
        TimerManager.Update(gameManager);
        SoundtrackManager.Update();

        gameManager.Update(delta);
        playerManager.Update(gameManager);
        enemyManager.Update(gameManager, [.. playerManager.Attacks]);
        levelManager.Update(gameManager);
        uiManager.Update(gameManager);

        // Console commands
        if (Constants.COMMANDS && InputManager.Hotkey(Keys.LeftControl, Keys.LeftShift, Keys.OemTilde))
        {
            Console.Write(">> ");
            string? cmd = Console.ReadLine();
            string resp = CommandManager.Execute(cmd ?? "");
            Logger.Log(resp);
        }

        // Final
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Clear and start
        GraphicsDevice.Clear(Color.Magenta);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // Draw game
        levelManager.Draw(gameManager);
        playerManager.Draw(gameManager);
        uiManager.Draw(GraphicsDevice, gameManager, playerManager.Inventory);
        menuManager.Draw(gameManager);

        // Text info
        DebugManager.StartBenchmark("DebugTextDraw");
        if (Constants.TEXT_INFO)
            DrawTextInfo();

        // Frame info
        if (Constants.FRAME_INFO)
            DrawFrameInfo();
        DebugManager.EndBenchmark("DebugTextDraw");

        // Frame bar
        DebugManager.StartBenchmark("FrameBarDraw");
        if (Constants.FRAME_BAR)
            DrawFrameBar();
        DebugManager.EndBenchmark("FrameBarDraw");

        // Cursor
        DrawTexture(spriteBatch, TextureID.CursorArrow, InputManager.MousePosition);

        // Final
        spriteBatch.End();
        base.Draw(gameTime);
    }
    // For cleaner code
    public void DrawFrameInfo()
    {
        float boxHeight = DebugManager.FrameTimes.Count * 20;
        spriteBatch.FillRectangle(new(Constants.Window.X - 190, 0, 180, boxHeight), Color.Black * 0.8f);

        debugSb.Clear();
        foreach (var kv in frameTimes)
        {
            debugSb.Append(kv.Key);
            debugSb.Append(": ");
            debugSb.AppendFormat("{0:0.0}ms", kv.Value);
            debugSb.Append('\n');
        }

        spriteBatch.DrawString(Arial, debugSb.ToString(), new Vector2(Constants.Window.X - 180, 10), Color.White);
    }
    public void DrawTextInfo()
    {
        spriteBatch.FillRectangle(new(0, 0, 220, 200), Color.Black * 0.8f);

        debugSb.Clear();
        debugSb.Append("FPS: ");
        debugSb.AppendFormat("{0:0.0}", cacheDelta != 0 ? 1f / cacheDelta : 0);
        debugSb.Append("\nTime: ");
        debugSb.AppendFormat("{0:0.00}", gameManager.TotalTime);
        debugSb.Append("\nCamera: ");
        debugSb.AppendFormat("{0:0.0},{1:0.0}", CameraManager.Camera.X, CameraManager.Camera.Y);
        debugSb.Append("\nTile Below: ");
        debugSb.Append(playerManager.TileBelow == null ? "none" : playerManager.TileBelow.Type);
        debugSb.Append("\nCoord: ");
        debugSb.AppendFormat("{0:0.0},{1:0.0}", CameraManager.TileCoord.X, CameraManager.TileCoord.Y);
        debugSb.Append("\nLevel: ");
        debugSb.Append(levelManager.Level?.Name);
        debugSb.Append("\nInventory: ");
        debugSb.Append(playerManager.Inventory.Opened);
        debugSb.Append("\nGUI: ");
        debugSb.Append(uiManager.Gui.Widgets.Count);
        debugSb.Append("\nMood: ");
        debugSb.Append(StateManager.Mood);
        debugSb.Append("\nMusic: ");
        debugSb.Append(SoundtrackManager.Playing?.File ?? "none");

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
        spriteBatch.FillRectangle(new(Constants.Window.X - 320, Constants.Window.Y - frameTimes.Count * 20 - 50, 320, 1000), Color.Black * .8f);

        // Labels and bars
        int start = 0;
        int c = 0;
        spriteBatch.FillRectangle(new(Constants.Window.X - 310, Constants.Window.Y - 40, 300, 25), Color.White);
        foreach (KeyValuePair<string, double> process in frameTimes)
        {
            spriteBatch.DrawString(Arial, process.Key, new(Constants.Window.X - Arial.MeasureString(process.Key).X - 5, Constants.Window.Y - 20 * c - 60), colors[c]);
            spriteBatch.FillRectangle(new(Constants.Window.X - 310 + start, Constants.Window.Y - 40, (int)(process.Value / (cacheDelta * 1000) * 300), 25), colors[c]);
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
