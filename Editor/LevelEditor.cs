using MonoGUI;
using System.Linq;
using System.Text;

namespace Quest.Editor;
public class LevelEditor : Game
{
    static readonly StringBuilder debugSb = new();
    // Devices and managers
    private readonly GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch = null!;
    private GameManager gameManager = null!;
    private OverlayManager uiManager = null!;
    private LevelManager levelManager = null!;
    private EditorManager editorManager = null!;
    private GUI gui = null!;
    private Matrix scale = Matrix.CreateScale(Constants.ScreenScale.X, Constants.ScreenScale.Y, 1f);

    // Editing
    private TileTypeID TileSelection;
    private DecalType DecalSelection;
    private BiomeType BiomeSelection;

    private Point mouseCoord;
    private Tile mouseTile = null!;
    private Point mouseSelectionCoord;
    private Point mouseSelection;
    private LevelGenerator levelGenerator = null!;
    private MouseMenu mouseMenu;
    private EditorTool currentTool = EditorTool.Tile;

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
            PreferredBackBufferWidth = Constants.ScreenResolution.X,
            PreferredBackBufferHeight = Constants.ScreenResolution.Y,
            IsFullScreen = false,
            SynchronizeWithVerticalRetrace = Constants.VSYNC,
            PreferHalfPixelOffset = false,
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        IsFixedTimeStep = Constants.FPS != -1;
        if (IsFixedTimeStep)
            TargetElapsedTime = TimeSpan.FromSeconds(1d / Constants.FPS);
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
        levelManager = new();
        uiManager = new(levelManager, null);
        gameManager = new(Content, spriteBatch, levelManager, uiManager);
        editorManager = new(GraphicsDevice, gameManager, levelManager, levelGenerator, spriteBatch, debugSb);
        StateManager.State = GameState.Editor;
        Logger.System("Initialized managers.");

        // Gui
        gui = new(this, spriteBatch, Arial);
        mouseMenu = new(gui, Point.Zero, new(100, 300), Color.White, Color.Black * 0.6f, GUI.NearBlack * 0.6f, border: 0, seperation: 1, borderColor: Color.Blue * 0.6f) { ItemBorder = 0 };
        mouseMenu.AddItem("Pick", () => { TileSelection = mouseTile.Type.ID; Logger.Log($"Picked tile '{TileSelection}' @ {mouseCoord.X}, {mouseCoord.Y}."); }, []);
        mouseMenu.AddItem("Open", editorManager.OpenLevelDialog, []);
        mouseMenu.AddItem("Fill", editorManager.FloodFill, []);
        mouseMenu.AddItem("Edit", editorManager.EditTile, []);

        MouseMenu newMenu = new(gui, Point.Zero, new(100, 80), Color.White, Color.Black * 0.6f, GUI.NearBlack * 0.6f, border: 0, seperation: 1, borderColor: Color.Blue * 0.6f) { ItemBorder = 0 };
        newMenu.AddItem("New NPC", editorManager.NewNPC, []);
        newMenu.AddItem("New Loot", editorManager.NewLoot, []);
        newMenu.AddItem("New Decal", editorManager.NewDecal, []);
        mouseMenu.AddItem("New...", null, []);
        mouseMenu.AddSubMenu("New...", newMenu);

        MouseMenu deleteMenu = new(gui, Point.Zero, new(150, 80), Color.White, Color.Black * 0.6f, GUI.NearBlack * 0.6f, border: 0, seperation: 1, borderColor: Color.Blue * 0.6f) { ItemBorder = 0 };
        deleteMenu.AddItem("Delete NPC", editorManager.DeleteNPC, []);
        deleteMenu.AddItem("Delete Loot", editorManager.DeleteLoot, []);
        deleteMenu.AddItem("Delete Decal", editorManager.DeleteDecal, []);
        mouseMenu.AddItem("Delete...", null, []);
        mouseMenu.AddSubMenu("Delete...", deleteMenu);

        mouseMenu.AddItem("Save", editorManager.SaveLevelDialog, []);
        mouseMenu.AddItem("Spawn", editorManager.SetSpawn, []);
        mouseMenu.AddItem("Tint", editorManager.SetTint, []);
        mouseMenu.AddItem("Generate", editorManager.GenerateLevel, []);
        mouseMenu.AddItem("Exit", Exit, []);
        gui.AddWidget(mouseMenu);

        Button tileDrawSelect = new(gui, new(Constants.Middle.X - 100, 10), new(90, 30), Color.White, Color.Black * 0.6f, ColorTools.NearBlack * 0.6f, () => currentTool = EditorTool.Tile, [], "Tiles", border: 0);
        Button decalDrawSelect = new(gui, new(Constants.Middle.X, 10), new(90, 30), Color.White, Color.Black * 0.6f, ColorTools.NearBlack * 0.5f, () => currentTool = EditorTool.Decal, [], "Decals", border: 0);
        Button biomeDrawSelect = new(gui, new(Constants.Middle.X + 100, 10), new(90, 30), Color.White, Color.Black * 0.6f, ColorTools.NearBlack * 0.6f, () => currentTool = EditorTool.Biome, [], "Biomes", border: 0);
        gui.AddWidgets(tileDrawSelect, decalDrawSelect, biomeDrawSelect);

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
        if (InputManager.Hotkey(Keys.LeftControl, Keys.Escape)) Exit();

        // Delta
        delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Mouse
        mouseCoord = (InputManager.MousePosition + CameraManager.Camera.ToPoint() - Constants.Middle) / Constants.TileSize;
        mouseCoord.X = Math.Clamp(mouseCoord.X, 0, Constants.MapSize.X - 1);
        mouseCoord.Y = Math.Clamp(mouseCoord.Y, 0, Constants.MapSize.Y - 1);
        mouseTile = GetTile(mouseCoord);

        // Movement
        DebugManager.StartBenchmark("InputUpdate");
        int speedup = InputManager.KeyDown(Keys.LeftControl) ? 0 : (InputManager.KeyDown(Keys.LeftAlt) ? 6 : 2);
        moveX = 0; moveY = 0;
        moveX += InputManager.AnyKeyDown(Keys.A, Keys.Left) ? -Constants.PlayerSpeed : 0;
        moveX += InputManager.AnyKeyDown(Keys.D, Keys.Right) ? Constants.PlayerSpeed : 0;
        moveY += InputManager.AnyKeyDown(Keys.W, Keys.Up) ? -Constants.PlayerSpeed : 0;
        moveY += InputManager.AnyKeyDown(Keys.S, Keys.Down) ? Constants.PlayerSpeed : 0;
        CameraManager.CameraDest += new Vector2(moveX, moveY) * delta * speedup;
        DebugManager.EndBenchmark("InputUpdate");

        // Manager
        editorManager.Update(TileSelection, BiomeSelection, currentTool, delta, mouseTile, mouseCoord, mouseSelection, mouseSelectionCoord);

        // Change material
        if (InputManager.ScrolledUp || InputManager.KeyPressed(Keys.OemOpenBrackets))
        {
            if (currentTool == EditorTool.Tile) NumberTools.CycleUp(ref TileSelection);
            else if (currentTool == EditorTool.Decal) NumberTools.CycleUp(ref DecalSelection);
            else if (currentTool == EditorTool.Biome) NumberTools.CycleUp(ref BiomeSelection);
        }
        if (InputManager.ScrolledDown || InputManager.KeyPressed(Keys.OemCloseBrackets))
        {
            if (currentTool == EditorTool.Tile) NumberTools.CycleDown(ref TileSelection);
            else if (currentTool == EditorTool.Decal) NumberTools.CycleDown(ref DecalSelection);
            else if (currentTool == EditorTool.Biome) NumberTools.CycleDown(ref BiomeSelection);
        }

        // Draw
        if (InputManager.LMouseDown && !mouseMenu.Visible)
        {
            // Add tile
            if (currentTool == EditorTool.Tile)
            {
                Tile tile;
                if (TileSelection == TileTypeID.Stairs)
                    tile = new Stairs(mouseCoord, "", Constants.MiddleCoord);
                else
                    tile = LevelManager.TileFromId(TileSelection, mouseCoord);

                editorManager.SetTile(tile);
            }
            // Add decal
            else if (currentTool == EditorTool.Decal && InputManager.LMouseClicked)
            {
                // Check existing decal
                Decal? current = levelManager.Level.Decals.FirstOrDefault(d => d.Location == mouseCoord);
                bool alreadyThere = current != null && current.Type == DecalSelection;
                if (current != null && current.Type != DecalSelection) levelManager.Level.Decals.Remove(current!); // Remove current one

                // Add
                if (!alreadyThere && levelManager.Level.Decals.Count < 255)
                    levelManager.Level.Decals.Add(LevelManager.DecalFromId(DecalSelection, mouseCoord));
            }
            // Set biome
            else if (currentTool == EditorTool.Biome)
            {
                int idx = LevelManager.Flatten(mouseCoord);
                levelManager.Level.Biome[idx] = BiomeSelection;
            }
        }

        // Edit options
        if (InputManager.Hotkey(Keys.LeftControl, Keys.M)) editorManager.EditTile();

        // Erase (set to sky)
        if (InputManager.RMouseDown) MouseSelect();

        // Pick
        if (InputManager.MMouseClicked) PickTile();
        // Open file
        if (InputManager.Hotkey(Keys.LeftControl, Keys.O)) editorManager.OpenLevelDialog();
        // Fill
        if (InputManager.KeyPressed(Keys.F)) editorManager.FloodFill();
        // NPCs
        if (InputManager.Hotkey(Keys.LeftControl, Keys.N)) { MouseSelect(); editorManager.NewNPC(); }
        if (InputManager.Hotkey(Keys.LeftControl, Keys.LeftShift, Keys.N)) { MouseSelect(); editorManager.DeleteNPC(); }
        // Loot
        if (InputManager.Hotkey(Keys.LeftControl, Keys.L)) { MouseSelect(); editorManager.NewLoot(); }
        if (InputManager.Hotkey(Keys.LeftControl, Keys.LeftShift, Keys.L)) { MouseSelect(); editorManager.DeleteLoot(); }
        // Decals
        if (InputManager.Hotkey(Keys.LeftControl, Keys.D)) { MouseSelect(); editorManager.NewDecal(); }
        if (InputManager.Hotkey(Keys.LeftControl, Keys.LeftShift, Keys.D)) { MouseSelect(); editorManager.DeleteDecal(); }
        // Save
        if (InputManager.Hotkey(Keys.LeftControl, Keys.E)) editorManager.SaveLevelDialog();
        // Level info
        if (InputManager.Hotkey(Keys.LeftControl, Keys.S)) editorManager.SetSpawn();
        if (InputManager.Hotkey(Keys.LeftControl, Keys.T)) editorManager.SetTint();
        // Generate level
        if (InputManager.Hotkey(Keys.LeftControl, Keys.G)) editorManager.GenerateLevel();
        // Resave level
        if (InputManager.Hotkey(Keys.LeftControl, Keys.R)) editorManager.ResaveLevel(levelManager.Level.LevelPath);
        // Resave world
        if (InputManager.Hotkey(Keys.LeftControl, Keys.LeftShift, Keys.R)) editorManager.ResaveWorld(levelManager.Level.World);
        // Tool select
        if (InputManager.KeyPressed(Keys.D1)) currentTool = EditorTool.Tile;
        if (InputManager.KeyPressed(Keys.D2)) currentTool = EditorTool.Decal;
        if (InputManager.KeyPressed(Keys.D3)) currentTool = EditorTool.Biome;
        // Script
        //if (InputManager.Hotkey(Keys.LeftControl, Keys.P)) editorManager.EditScript();

        // Managers
        if (!PopupFactory.PopupOpen) InputManager.Update(this);
        DebugManager.Update();
        CameraManager.Update(delta);
        CameraManager.CameraDest = Vector2.Clamp(CameraManager.CameraDest, Constants.Middle.ToVector2(), (Constants.MapSize * Constants.TileSize - Constants.Middle).ToVector2());
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
        spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: scale);

        Point mouseCoordDraw = mouseCoord * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle;

        // Draw game
        levelManager.Draw(gameManager);
        uiManager.Draw(GraphicsDevice, gameManager, null);

        // Render biome markers
        Point start = (CameraManager.Camera.ToPoint() - Constants.Middle) / Constants.TileSize;
        Point end = (CameraManager.Camera.ToPoint() + Constants.Middle) / Constants.TileSize;
        for (int y = start.Y; y <= end.Y; y++)
        {
            for (int x = start.X; x <= end.X; x++)
            {
                Point loc = new(x, y);
                Point dest = loc * Constants.TileSize - CameraManager.Camera.ToPoint() + Constants.Middle;
                BiomeType? biome = levelManager.GetBiome(loc);
                Color color = biome == null ? Color.Magenta : Biome.Colors[(int)biome];
                spriteBatch.Draw(Textures[TextureID.TileOutline], dest.ToVector2(), levelManager.BiomeTextureSource(loc), color, 0, Vector2.Zero, Constants.TileSizeScale, SpriteEffects.None, 1.0f);
            }
        }

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
        if (currentTool == EditorTool.Tile)
        {
            TextureID texture = (TextureID)Enum.Parse(typeof(TextureID), TileSelection.ToString());
            DrawTexture(spriteBatch, texture, mouseCoordDraw, source: new(Point.Zero, Constants.TilePixelSize), scale: Constants.TileSizeScale, color: Constants.SemiTransparent);
        }
        else if (currentTool == EditorTool.Decal)
        {
            TextureID texture = (TextureID)Enum.Parse(typeof(TextureID), DecalSelection.ToString());
            DrawTexture(spriteBatch, texture, mouseCoordDraw, source: new(Point.Zero, Constants.TilePixelSize), scale: Constants.TileSizeScale, color: Constants.SemiTransparent);
        }
        else if (currentTool == EditorTool.Biome)
        {
            DrawTexture(spriteBatch, TextureID.TileOutline, mouseCoordDraw, source: new(Point.Zero, Constants.TilePixelSize), scale: Constants.TileSizeScale, color: Biome.Colors[(int)BiomeSelection]);
            Vector2 textCenter = Arial.MeasureString(BiomeSelection.ToString()) / 2;
            spriteBatch.DrawString(Arial, BiomeSelection.ToString(), (mouseCoordDraw + Constants.TileHalfSize).ToVector2(), Color.Black, MathHelper.PiOver4, textCenter, 1.0f, SpriteEffects.None, 1.0f);
        }


        // Tile info
        if (mouseTile is Stairs stair)
            DrawBottomInfo($"[Stairs] dest: '{stair.DestLevel}' @ {stair.Dest}");
        else if (mouseTile is Door door)
            DrawBottomInfo($"[Door] key: {(door.Key == null ? "None" : $"'{door.Key.Name}' x{door.Key.Amount}")} consume: {door.ConsumeKey}");
        else if (mouseTile is Chest chest)
            DrawBottomInfo($"[Chest] gen: '{chest.LootGenerator.FileName}'");
        else if (mouseTile is Lamp lamp)
            DrawBottomInfo($"[Lamp] radius: {lamp.LightRadius}");
        else
            DrawBottomInfo($"[{mouseTile?.Type.Texture}]");

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
        if (currentTool == EditorTool.Tile) TileSelection = mouseTile.Type.ID;
        else if (currentTool == EditorTool.Decal)
        {
            Decal? picked = levelManager.Level.Decals.FirstOrDefault(d => d.Location == mouseCoord);
            if (picked != null) DecalSelection = picked.Type;
        }
        else if (currentTool == EditorTool.Biome) BiomeSelection = levelManager.GetBiome(mouseCoord)!.Value;
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
        text = $"{TileSelection} | {text}";
        Vector2 textSize = Arial.MeasureString(text);
        Vector2 pos = new(Constants.Middle.X - textSize.X / 2, Constants.NativeResolution.Y - textSize.Y - 3);
        spriteBatch.FillRectangle(new(pos - Vector2.One * 4, textSize + Vector2.One * 8), Color.Gray * 0.5f);
        spriteBatch.DrawRectangle(new(pos - Vector2.One * 4, textSize + Vector2.One * 8), Color.Black * 0.5f);
        spriteBatch.DrawString(Arial, text, pos, Color.Black);
    }
}
