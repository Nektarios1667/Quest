using System.IO;
using System.IO.Compression;
using System.Text;

namespace Quest.Editor;
public class Window : Game
{
    static readonly StringBuilder debugSb = new();
    // Devices and managers
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private GameManager gameManager;
    private UIManager uiManager;
    private LevelManager levelManager;
    private MenuManager menuManager;
    private EditorManager editorManager;

    // Editing
    private TileType Material;
    private int Selection;
    private Point mouseCoord;
    private readonly Color highlightColor = new(1, 1, 1, .8f);
    private Tile mouseTile;
    private LevelGenerator levelGenerator;
    private RenderTarget2D? minimap;
    private bool rebuildMinimap = true;

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
        spriteBatch = new SpriteBatch(GraphicsDevice);

        // Textures
        LoadTextures(Content);

        // Managers
        levelGenerator = new(42, 1f / 64);
        uiManager = new();
        levelManager = new();
        menuManager = new();
        gameManager = new(Content, spriteBatch, new(0, 0), levelManager, uiManager);
        editorManager = new(gameManager, levelManager, levelGenerator, spriteBatch, debugSb);
        StateManager.State = GameState.Editor;

        // Shaders
        Grayscale = Content.Load<Effect>("Shaders/Grayscale");

        // Other
        CursorArrow = Content.Load<Texture2D>("Images/Gui/CursorArrow");

        // Timer
        TimerManager.NewTimer("FrameTimeUpdate", 1, editorManager.UpdateFrameTimes, int.MaxValue);
    }

    protected override void Update(GameTime gameTime)
    {
        DebugManager.Watch.Restart();

        // Exit
        if (InputManager.KeyDown(Keys.Escape)) Exit();

        // Delta
        delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Mouse
        mouseCoord = (InputManager.MousePosition + CameraManager.Camera.ToPoint() - Constants.Middle) / Constants.TileSize;
        mouseCoord.X = Math.Clamp(mouseCoord.X, 0, Constants.MapSize.X - 1);
        mouseCoord.Y = Math.Clamp(mouseCoord.Y, 0, Constants.MapSize.Y - 1);
        mouseTile = GetTile(mouseCoord);

        // Movement
        DebugManager.StartBenchmark("InputUpdate");
        int speedup = InputManager.KeyDown(Keys.LeftAlt) ? 5 : 2;
        moveX = 0; moveY = 0;
        moveX += InputManager.AnyKeyDown(Keys.A, Keys.Left) ? -Constants.PlayerSpeed : 0;
        moveX += InputManager.AnyKeyDown(Keys.D, Keys.Right) ? Constants.PlayerSpeed : 0;
        moveY += InputManager.AnyKeyDown(Keys.W, Keys.Up) ? -Constants.PlayerSpeed : 0;
        moveY += InputManager.AnyKeyDown(Keys.S, Keys.Down) ? Constants.PlayerSpeed : 0;
        CameraManager.CameraDest += new Vector2(moveX, moveY) * delta * speedup;
        DebugManager.EndBenchmark("InputUpdate");

        // Manager
        editorManager.Update(Material, delta, mouseTile, mouseCoord);

        // Minimap
        if (rebuildMinimap) RebuildMiniMap();

        // Open file
        if (InputManager.Hotkey(Keys.LeftControl, Keys.O))
        {
            string filename = Logger.Input("Open level file: ");
            try
            {
                levelManager.ReadLevel(uiManager, filename);
                levelManager.LoadLevel(gameManager, filename);
                Logger.Log($"Opened level '{filename}'.");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to open level '{filename}': {ex.Message}");
            }
        }

        // Change material
        if (InputManager.ScrollWheelChange > 0 || InputManager.KeyPressed(Keys.OemCloseBrackets))
        {
            Selection = (Selection + 1) % Constants.TileNames.Length;
            Material = (TileType)Enum.Parse(typeof(TileType), Constants.TileNames[Selection]);
            Logger.Log($"Material set to '{Material}'.");
        }
        if (InputManager.ScrollWheelChange < 0 || InputManager.KeyPressed(Keys.OemCloseBrackets))
        {
            Selection = (Selection - 1) % Constants.TileNames.Length;
            if (Selection < 0) Selection += Constants.TileNames.Length;
            Material = (TileType)Enum.Parse(typeof(TileType), Constants.TileNames[Selection]);
            Logger.Log($"Material set to '{Material}'.");
        }

        // Draw
        if (InputManager.LMouseDown && mouseTile.Type != Material)
        {
            // Add tile
            Tile tile;
            if (Selection == (int)TileType.Stairs)
                tile = new Stairs(mouseCoord, "null", Constants.MiddleCoord);
            else if (Selection == (int)TileType.Door)
                tile = new Door(mouseCoord, "Key");
            else
                tile = LevelManager.TileFromId(Selection, mouseCoord);

            SetTile(tile);
            rebuildMinimap = true;
            Logger.Log($"Set tile to '{Material}' @ {mouseCoord.X}, {mouseCoord.Y}.");
        }

        // Edit options
        if (InputManager.Hotkey(Keys.LeftControl, Keys.M)) EditorManager.EditTile(mouseTile);

        // Erase (set to sky)
        if (InputManager.RMouseDown && mouseTile.Type != TileType.Sky)
        {
            SetTile(new Sky(mouseCoord));
            rebuildMinimap = true;
            Logger.Log($"Set tile to '{Material}' @ {mouseCoord.X}, {mouseCoord.Y}.");
        }

        // Pick
        if (InputManager.MMouseClicked)
        {
            Selection = (int)mouseTile.Type;
            Material = (TileType)Enum.Parse(typeof(TileType), Constants.TileNames[Selection]);
            Logger.Log($"Picked tile '{Material}' @ {mouseCoord.X}, {mouseCoord.Y}.");
        }

        // Fill
        if (InputManager.KeyPressed(Keys.F)) editorManager.FloodFill();
        // NPCs
        if (InputManager.Hotkey(Keys.LeftControl, Keys.N)) editorManager.EditNPCs();
        // Loot
        if (InputManager.Hotkey(Keys.LeftControl, Keys.L)) editorManager.EditLoot();
        // Decals
        if (InputManager.Hotkey(Keys.LeftControl, Keys.D)) editorManager.EditDecals();
        // Save
        if (InputManager.Hotkey(Keys.LeftControl, Keys.E)) editorManager.SaveLevel();
        // Level info
        if (InputManager.Hotkey(Keys.LeftControl, Keys.I)) editorManager.SetSpawn();
        if (InputManager.Hotkey(Keys.LeftControl, Keys.T)) editorManager.SetTint();
        // Generate level
        if (InputManager.Hotkey(Keys.LeftControl, Keys.G))
        {
            editorManager.GenerateLevel();
            rebuildMinimap = true;
        }

        // Managers
        InputManager.Update();
        DebugManager.Update();
        CameraManager.Update(delta);

        TimerManager.Update(gameManager);
        gameManager.Update(delta);
        levelManager.Update(gameManager);
        uiManager.Update(gameManager);

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
        uiManager.Draw(GraphicsDevice, gameManager, new(0, 0));
        menuManager.Draw(gameManager);

        // Text info
        DebugManager.StartBenchmark("DebugTextDraw");
        if (Constants.TEXT_INFO)
            editorManager.DrawTextInfo();

        // Frame info
        if (Constants.FRAME_INFO)
            editorManager.DrawFrameInfo();
        DebugManager.EndBenchmark("DebugTextDraw");

        // Frame bar
        DebugManager.StartBenchmark("FrameBarDraw");
        if (Constants.FRAME_BAR)
            editorManager.DrawFrameBar();
        DebugManager.EndBenchmark("FrameBarDraw");

        // Minimap
        DrawMiniMap();

        // Cursor
        if (mouseTile != null)
        {
            TextureID texture = (TextureID)Enum.Parse(typeof(TextureID), Material.ToString());
            DrawTexture(spriteBatch, texture, mouseTile.Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle, source:new(Point.Zero, Constants.TilePixelSize), scale:4, color:Constants.SemiTransparent);
        }
        DrawTexture(spriteBatch, TextureID.CursorArrow, InputManager.MousePosition);

        // Final
        spriteBatch.End();
        base.Draw(gameTime);
    }
    public void DrawMiniMap()
    {
        DebugManager.StartBenchmark("DrawMinimap");

        // Frame
        gameManager.Batch.DrawRectangle(new(7, Constants.Window.Y - Constants.MapSize.Y - 13, Constants.MapSize.X + 6, Constants.MapSize.Y + 6), Color.Black, 3);

        // Draw minimap texture
        if (minimap != null)
            spriteBatch.Draw(minimap, new Rectangle(10, Constants.Window.Y - Constants.MapSize.Y - 10, Constants.MapSize.X, Constants.MapSize.Y), Color.White);

        // Player
        Point dest = CameraManager.TileCoord + new Point(10, Constants.Window.Y - Constants.MapSize.Y - 10);
        spriteBatch.DrawPoint(dest.ToVector2(), Color.Red, size: 2);

        DebugManager.EndBenchmark("DrawMinimap");
    }

    public void RebuildMiniMap()
    {
        minimap = new RenderTarget2D(GraphicsDevice, Constants.MapSize.X, Constants.MapSize.Y);
        GraphicsDevice.SetRenderTarget(minimap);
        GraphicsDevice.Clear(Color.Transparent);
        spriteBatch.Begin();

        for (int y = 0; y < Constants.MapSize.Y; y++)
        {
            for (int x = 0; x < Constants.MapSize.X; x++)
            {
                Tile tile = gameManager.LevelManager.GetTile(new Point(x, y))!;
                spriteBatch.DrawPoint(new(x, y), Constants.MiniMapColors[(int)tile.Type]);
            }
        }

        spriteBatch.End();
        GraphicsDevice.SetRenderTarget(null);
        rebuildMinimap = false;
    }

    public static byte IntToByte(int value)
    {
        if (value < 0 || value > 255)
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be between 0 and 255.");
        return (byte)value;
    }
    public Tile GetTile(Point coord)
    {
        if (coord.X < 0 || coord.X >= Constants.MapSize.X || coord.Y < 0 || coord.Y >= Constants.MapSize.Y)
            throw new ArgumentOutOfRangeException(nameof(coord), "Coordinates are out of bounds of the level.");
        return levelManager.Level.Tiles[coord.X + coord.Y * Constants.MapSize.X];
    }
    public void SetTile(Tile tile)
    {
        levelManager.Level.Tiles[tile.Location.X + tile.Location.Y * Constants.MapSize.X] = tile;
    }
}
