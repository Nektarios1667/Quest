using System.Text;
using MonoGUI;

namespace Quest.Editor;
public class LevelEditor : Game
{
    static readonly StringBuilder debugSb = new();
    // Devices and managers
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch = null!;
    private GameManager gameManager = null!;
    private UIManager uiManager = null!;
    private LevelManager levelManager = null!;
    private MenuManager menuManager = null!;
    private EditorManager editorManager = null!;
    private GUI gui = null!;

    // Editing
    private TileType Material;
    private int Selection;
    private Point mouseCoord;
    private Tile mouseTile = null!;
    private Point mouseSelectionCoord;
    private Point mouseSelection;
    private LevelGenerator levelGenerator = null!;
    private MouseMenu mouseMenu;

    // Time
    private float delta = 0;

    // Textures
    public Texture2D CursorArrow { get; private set; } = null!;

    // Movements
    public int moveX = 0;
    public int moveY = 0;
    // Shaders
    public Effect Grayscale { get; private set; } = null!;
    public Effect Light { get; private set; } = null!;
    // Render targets
    public LevelEditor()
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
        Logger.System("Initialized level editor window object.");
    }

    protected override void Initialize()
    {
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
        gameManager = new(Content, spriteBatch, levelManager, uiManager);
        editorManager = new(GraphicsDevice, gameManager, levelManager, levelGenerator, spriteBatch, debugSb);
        StateManager.State = GameState.Editor;
        Logger.System("Initialized managers.");

        // Gui
        gui = new(this, spriteBatch, Arial);
        mouseMenu = new(gui, Point.Zero, new(100, 300), Color.White, GUI.NearBlack, Color.Gray, border:1, seperation:3, borderColor:Color.White);
        mouseMenu.AddItem("Pick", () => { Selection = (int)mouseTile.Type; Material = mouseTile.Type; Logger.Log($"Picked tile '{Material}' @ {mouseCoord.X}, {mouseCoord.Y}."); }, []);
        mouseMenu.AddItem("Open", editorManager.OpenFile, []);
        mouseMenu.AddItem("Fill", editorManager.FloodFill, []);
        mouseMenu.AddItem("Edit", editorManager.EditTile, []);

        MouseMenu newMenu = new(gui, Point.Zero, new(100, 80), Color.White, GUI.NearBlack, Color.Gray, border: 1, seperation: 3, borderColor: Color.White);
        newMenu.AddItem("New NPC", editorManager.NewNPC, []);
        newMenu.AddItem("New Loot", editorManager.NewLoot, []);
        newMenu.AddItem("New Decal", editorManager.NewDecal, []);
        mouseMenu.AddItem("New...", null, []);
        mouseMenu.AddSubMenu("New...", newMenu);

        MouseMenu deleteMenu = new(gui, Point.Zero, new(150, 80), Color.White, GUI.NearBlack, Color.Gray, border: 1, seperation: 3, borderColor: Color.White);
        deleteMenu.AddItem("Delete NPC", editorManager.DeleteNPC, []);
        deleteMenu.AddItem("Delete Loot", editorManager.DeleteLoot, []);
        deleteMenu.AddItem("Delete Decal", editorManager.DeleteDecal, []);
        mouseMenu.AddItem("Delete...", null, []);
        mouseMenu.AddSubMenu("Delete...", deleteMenu);

        mouseMenu.AddItem("Save", editorManager.SaveLevel, []);
        mouseMenu.AddItem("Spawn", editorManager.SetSpawn, []);
        mouseMenu.AddItem("Tint", editorManager.SetTint, []);
        mouseMenu.AddItem("Generate", editorManager.GenerateLevel, []);
        mouseMenu.AddItem("Exit", Exit, []);
        gui.Widgets.Add(mouseMenu);

        gui.LoadContent(Content, "Images/Gui");
        Logger.System("Initialized GUI.");

        // Shaders
        Grayscale = Content.Load<Effect>("Shaders/Grayscale");

        // Other
        CursorArrow = Content.Load<Texture2D>("Images/Gui/CursorArrow");

        // Timer
        TimerManager.NewTimer("FrameTimeUpdate", 1, editorManager.UpdateFrameTimes, int.MaxValue);

        // Final
        Logger.System("Level editor finished initializing.");
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
        editorManager.Update(Material, delta, mouseTile, mouseCoord, mouseSelection, mouseSelectionCoord);

        // Change material
        if (InputManager.ScrollWheelChange > 0 || InputManager.KeyPressed(Keys.OemCloseBrackets))
        {
            Selection = (Selection + 1) % Constants.TileTypeNames.Length;
            Material = (TileType)Enum.Parse(typeof(TileType), Constants.TileTypeNames[Selection]);
            Logger.Log($"Material set to '{Material}'.");
        }
        if (InputManager.ScrollWheelChange < 0 || InputManager.KeyPressed(Keys.OemCloseBrackets))
        {
            Selection = (Selection - 1) % Constants.TileTypeNames.Length;
            if (Selection < 0) Selection += Constants.TileTypeNames.Length;
            Material = (TileType)Enum.Parse(typeof(TileType), Constants.TileTypeNames[Selection]);
            Logger.Log($"Material set to '{Material}'.");
        }

        // Draw
        if (InputManager.LMouseDown && mouseTile.Type != Material && !mouseMenu.Visible)
        {
            // Add tile
            Tile tile;
            if (Selection == (int)TileType.Stairs)
                tile = new Stairs(mouseCoord, "", Constants.MiddleCoord);
            else if (Selection == (int)TileType.Door)
                tile = new Door(mouseCoord, "");
            else
                tile = LevelManager.TileFromId(Selection, mouseCoord);

            editorManager.SetTile(tile);
            Logger.Log($"Set tile to '{Material}' @ {mouseCoord.X}, {mouseCoord.Y}.");
        }

        // Edit options
        if (InputManager.Hotkey(Keys.LeftControl, Keys.M)) editorManager.EditTile();

        // Erase (set to sky)
        if (InputManager.RMouseDown) MouseSelect();

        // Pick
        if (InputManager.MMouseClicked) PickTile();
        // Open file
        if (InputManager.Hotkey(Keys.LeftControl, Keys.O)) editorManager.OpenFile();
        // Fill
        if (InputManager.KeyPressed(Keys.F)) editorManager.FloodFill();
        // NPCs
        if (InputManager.Hotkey(Keys.LeftControl, Keys.N)) { MouseSelect(); editorManager.EditNPCs(); }
        // Loot
        if (InputManager.Hotkey(Keys.LeftControl, Keys.L)) { MouseSelect(); editorManager.EditLoot(); }
        // Decals
        if (InputManager.Hotkey(Keys.LeftControl, Keys.D)) { MouseSelect(); editorManager.EditDecals(); }
        // Save
        if (InputManager.Hotkey(Keys.LeftControl, Keys.E)) editorManager.SaveLevel();
        // Level info
        if (InputManager.Hotkey(Keys.LeftControl, Keys.S)) editorManager.SetSpawn();
        if (InputManager.Hotkey(Keys.LeftControl, Keys.T)) editorManager.SetTint();
        // Generate level
        if (InputManager.Hotkey(Keys.LeftControl, Keys.G)) editorManager.GenerateLevel();

        // Managers
        if (!PopupFactory.PopupOpen) InputManager.Update();
        DebugManager.Update();
        CameraManager.Update(delta);
        // Gui
        gui.Update(delta, InputManager.MouseState, InputManager.KeyboardState);

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
        uiManager.Draw(GraphicsDevice, gameManager, null);
        menuManager.Draw(gameManager);

        // Text info
        DebugManager.StartBenchmark("DebugTextDraw");
        if (DebugManager.TextInfo)
            editorManager.DrawTextInfo();

        // Frame info
        if (DebugManager.FrameInfo)
            editorManager.DrawFrameInfo();
        DebugManager.EndBenchmark("DebugTextDraw");

        // Frame bar
        DebugManager.StartBenchmark("FrameBarDraw");
        if (DebugManager.FrameBar)
            editorManager.DrawFrameBar();
        DebugManager.EndBenchmark("FrameBarDraw");

        // Minimap
        editorManager.DrawMiniMap();

        // Ghost tile
        if (mouseTile != null)
        {
            TextureID texture = (TextureID)Enum.Parse(typeof(TextureID), Material.ToString());
            DrawTexture(spriteBatch, texture, mouseTile.Location * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle, source:new(Point.Zero, Constants.TilePixelSize), scale:4, color:Constants.SemiTransparent);
        }

        // Tile info
        if (mouseTile is Stairs stair)
            DrawBottomInfo($"[Stairs] dest: '{stair.DestLevel}' @ {stair.DestPosition.X},{stair.DestPosition.Y}");
        else if (mouseTile is Door door)
            DrawBottomInfo($"[Door] key: '{door.Key}'");
        else if (mouseTile is Chest chest)
            DrawBottomInfo($"[Chest] gen: '{chest.LootGeneratorFileName}'");
        
        // Gui
        gui.Draw();

        // Cursor
        DrawTexture(spriteBatch, TextureID.CursorArrow, InputManager.MousePosition);

        // Final
        spriteBatch.End();
        base.Draw(gameTime);
    }
    public void PickTile()
    {
        Selection = (int)mouseTile.Type;
        Material = (TileType)Enum.Parse(typeof(TileType), Constants.TileTypeNames[Selection]);
        Logger.Log($"Picked tile '{Material}' @ {mouseCoord.X}, {mouseCoord.Y}.");
    }
    public void MouseSelect()
    {
        mouseSelection = CameraManager.Camera.ToPoint() + InputManager.MousePosition - Constants.Middle;
        mouseSelectionCoord = mouseCoord;
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
    public void DrawBottomInfo(string text)
    {
        Vector2 textSize = Arial.MeasureString(text);
        Vector2 pos = new(Constants.Middle.X - textSize.X / 2, Constants.Window.Y - textSize.Y - 3);
        spriteBatch.FillRectangle(new(pos - Vector2.One * 4, textSize + Vector2.One * 8), Color.Gray * 0.5f);
        spriteBatch.DrawRectangle(new(pos - Vector2.One * 4, textSize + Vector2.One * 8), Color.Black * 0.5f);
        spriteBatch.DrawString(Arial, text, pos, Color.Black);
    }
}
