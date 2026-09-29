using MonoGUI;
using MonoGUI.Widgets;
using Quest.Editor.Generator;
using Quest.Editor.Managers;
using Quest.World;
using System.Linq;
using System.Text;

namespace Quest.Editor;

public class LevelEditor : Game, IAdjustableWindow
{

    readonly StringBuilder memoryDebugSb = new();
    // Devices and managers
    private readonly GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch = null!;
    private SpriteBatch minimapBatch = null!;
    private GameManager gameManager = null!;
    private LevelManager levelManager = null!;
    private EditorManager editorManager = null!;
    private EditorLevelManager editorLevelManager = null!;
    private EditorOverlayManager editorOverlayManager = null!;
    private GUI gui = null!;
    public RenderTarget2D Render = null!;

    // GUIs and menus
    private GUI SettingsMenu = null!;
    private Group tilesetGroup = null!;
    private Group itemsetGroup = null!;
    private RectangleShape tilesetHighlight = null!;
    private RectangleShape itemsetHighlight = null!;
    private RectangleShape toolHighlight = null!;

    // Editing
    private int TileSelectionIdx = 0;
    private int ItemSelectionIdx = 0;
    private TilesetTypes _tilesetSelection;
    private ItemsetTypes _itemsetSelection;
    private TilesetTypes TilesetSelection
    {
        get => _tilesetSelection;
        set
        {
            TileSelectionIdx = 0;
            _tilesetSelection = value;
        }
    }
    private ItemsetTypes ItemsetSelection
    {
        get => _itemsetSelection;
        set
        {
            ItemSelectionIdx = 0;
            _itemsetSelection = value;
        }
    }
    private byte[] _tileSelectionData = [];
    private byte[] TileSelectionData { 
        get => _tileSelectionData;
        set
        {
            _tileSelectionData = value;
            InvalidatePreviewTile();
        }
    }
    private TileTypeID TileSelection => Tilesets.TypeToArray[TilesetSelection][TileSelectionIdx];
    private ItemTypeID ItemSelection => Itemsets.TypeToArray[ItemsetSelection][ItemSelectionIdx];
    private DecalType DecalSelection;
    private BiomeType BiomeSelection;
    private byte LootAmountSelection = 1;
    private EnemyPresetType EnemySelection;
    private int NPCSelectionIdx;
    private TextureID NPCSelection => TypeTextures[TextureType.Character][NPCSelectionIdx];
    private Tile? PreviewTile;

    private Point mouseCoord;
    private Tile? mouseTile = null!;
    private DecalType? mouseDecal = null;
    private BiomeType? mouseBiome = null;
    private Point mouseSelectionCoord;
    private Point mouseSelection;
    private LevelGenerator levelGenerator = null!;
    private MouseMenu mouseMenu = null!;
    private EditorTool currentTool = EditorTool.None;
    private Rectangle[] noDrawZones = [];

    // Time
    private float delta = 0;

    // Textures
    public Texture2D CursorArrow { get; private set; } = null!;

    // Movements
    public int moveX = 0;
    public int moveY = 0;
    // Render targets
    public LevelEditor()
    {
        graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = SettingsManager.ScreenResolution.X,
            PreferredBackBufferHeight = SettingsManager.ScreenResolution.Y,
            IsFullScreen = false,
            SynchronizeWithVerticalRetrace = SettingsManager.VSYNC,
            PreferHalfPixelOffset = false,
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        Window.Title = $"Quest Level Editor";
        IsFixedTimeStep = SettingsManager.FPS != -1;
        if (IsFixedTimeStep)
            TargetElapsedTime = TimeSpan.FromSeconds(1d / SettingsManager.FPS);

        Logger.System("Initialized level editor window object.");
    }
    public void SetVsync(bool enabled)
    {
        graphics.SynchronizeWithVerticalRetrace = enabled;
        graphics.ApplyChanges();
    }
    public void ResetRender()
    {
        Render?.Dispose();
        Render = new RenderTarget2D(
            GraphicsDevice,
            Constants.NativeResolution.X, Constants.NativeResolution.Y,
            false,
            SurfaceFormat.Color,
            DepthFormat.None,
            0,
            RenderTargetUsage.PreserveContents
        );
    }
    public void SetResolution(int width, int height)
    {
        graphics.PreferredBackBufferWidth = width;
        graphics.PreferredBackBufferHeight = height;

        // Update stored resolution and scale
        SettingsManager.SetScreenResolution(width, height);

        // Recreate native render target (keep it at Constants.NativeResolution)
        ResetRender();

        graphics.ApplyChanges();
    }
    public void SetFullscreen(bool enabled)
    {
        graphics.IsFullScreen = enabled;
        if (graphics.IsFullScreen)
        {
            graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
        }

        graphics.ApplyChanges();
    }
    protected override void Initialize()
    {
        base.Initialize();

        gameManager.StateManager.State = GameState.Editor;
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        minimapBatch = new SpriteBatch(GraphicsDevice);

        // Textures
        LoadTextures(Content);

        // Managers
        levelGenerator = new(42, 1f / 64);
        levelManager = new();
        gameManager = new(spriteBatch, levelManager, null, null, null); // No WeatherManager or OverlayManager
        gameManager.StateManager.State = GameState.Editor;
        editorManager = new(gameManager);
        editorLevelManager = new(gameManager, levelGenerator);
        editorOverlayManager = new(gameManager, spriteBatch, GraphicsDevice);
        levelManager.LevelLoaded += (_) => editorOverlayManager.InvalidateMinimap();
        TimerManager.SetTimer("EditorOverlayInvalidateMinimap", 0.1f, false, editorOverlayManager.InvalidateMinimap, int.MaxValue);
        levelManager.LevelLoaded += (Level level) => Window.Title = $"Quest Level Editor - {level.LevelPath}";
        Window.Title = "Quest Level Editor";

        gameManager.StateManager.State = GameState.Editor;
        Logger.System("Initialized managers.");

        // Create native-resolution render target for the editor
        ResetRender();

        // Settings gui
        SettingsMenu = SettingsManager.CreateSettingsMenu(this, this, gameManager, spriteBatch, Content);
        SettingsMenu.LoadContent();

        // Editor gui
        // Mouse menu
        gui = new(this, spriteBatch, Arial);

        mouseMenu = new(gui, Point.Zero, new(100, 300), Color.White, Color.Black * 0.6f, GUI.NearBlack * 0.6f, border: 0, seperation: 1, borderColor: Color.Blue * 0.6f) { ItemBorder = 0 };
        mouseMenu.AddItem("Pick", PickTile, []);
        mouseMenu.AddItem("Open", editorLevelManager.OpenLevelDialog, []);
        mouseMenu.AddItem("Fill", editorManager.FloodFill, []);
        mouseMenu.AddItem("Draw Biome", () => editorManager.ShowBiomeMarkers = !editorManager.ShowBiomeMarkers, []);

        MouseMenu editMenu = new(gui, Point.Zero, new(120, 65), Color.White, Color.Black * 0.6f, GUI.NearBlack * 0.6f, border: 0, seperation: 1, borderColor: Color.Blue * 0.6f) { ItemBorder = 0 };
        editMenu.AddItem("Edit NPC", editorManager.EditNPC, []);
        editMenu.AddItem("Edit Enemy", editorManager.EditEnemy, []);
        editMenu.AddItem("Edit Tile", editorManager.EditTile, []);
        mouseMenu.AddItem("Edit...", null, []);
        mouseMenu.AddSubMenu("Edit...", editMenu);

        MouseMenu newMenu = new(gui, Point.Zero, new(120, 145), Color.White, Color.Black * 0.6f, GUI.NearBlack * 0.6f, border: 0, seperation: 1, borderColor: Color.Blue * 0.6f) { ItemBorder = 0 };
        newMenu.AddItem("New NPC", () => editorManager.NewNPC(), []);
        newMenu.AddItem("New Enemy", () => editorManager.NewEnemy(), []);
        newMenu.AddItem("New Loot", editorManager.NewLoot, []);
        newMenu.AddItem("New Decal", editorManager.NewDecal, []);
        newMenu.AddItem("New Script", editorManager.NewScript, []);
        newMenu.AddItem("New Waypoint", editorManager.NewWaypoint, []);
        newMenu.AddItem("New Transition", editorManager.NewTransition, []);
        mouseMenu.AddItem("New...", null, []);
        mouseMenu.AddSubMenu("New...", newMenu);

        MouseMenu deleteMenu = new(gui, Point.Zero, new(150, 125), Color.White, Color.Black * 0.6f, GUI.NearBlack * 0.6f, border: 0, seperation: 1, borderColor: Color.Blue * 0.6f) { ItemBorder = 0 };
        deleteMenu.AddItem("Delete NPC", editorManager.DeleteNPC, []);
        deleteMenu.AddItem("Delete Enemy", editorManager.DeleteEnemy, []);
        deleteMenu.AddItem("Delete Loot", editorManager.DeleteLoot, []);
        deleteMenu.AddItem("Delete Decal", editorManager.DeleteDecal, []);
        deleteMenu.AddItem("Delete Script", editorManager.DeleteScript, []);
        deleteMenu.AddItem("Delete Transition", editorManager.DeleteTransition, []);
        mouseMenu.AddItem("Delete...", null, []);
        mouseMenu.AddSubMenu("Delete...", deleteMenu);

        mouseMenu.AddItem("Save", editorLevelManager.SaveLevelDialog, []);
        mouseMenu.AddItem("Spawn", editorManager.SetSpawn, []);
        mouseMenu.AddItem("Tint", editorManager.SetTint, []);
        mouseMenu.AddItem("Generate", editorLevelManager.GenerateLevel, []);
        mouseMenu.AddItem("Settings", () => gameManager.StateManager.State = GameState.Settings, []);
        mouseMenu.AddItem("Exit", Exit, []);
        gui.AddWidget(mouseMenu);

        // Mouse select
        Button noDrawSelect = new(gui, new(Constants.Middle.X - 300, 10), new(90, 30), Color.White, Color.Black * 0.6f, ColorTools.NearBlack * 0.6f, () => currentTool = EditorTool.None, [], "Cursor", border: 0);
        Button tileDrawSelect = new(gui, new(Constants.Middle.X - 200, 10), new(90, 30), Color.White, Color.Black * 0.6f, ColorTools.NearBlack * 0.6f, () => currentTool = EditorTool.Tile, [], "Tiles", border: 0);
        Button decalDrawSelect = new(gui, new(Constants.Middle.X - 100, 10), new(90, 30), Color.White, Color.Black * 0.6f, ColorTools.NearBlack * 0.6f, () => currentTool = EditorTool.Decal, [], "Decals", border: 0);
        Button biomeDrawSelect = new(gui, new(Constants.Middle.X, 10), new(90, 30), Color.White, Color.Black * 0.6f, ColorTools.NearBlack * 0.6f, () => currentTool = EditorTool.Biome, [], "Biomes", border: 0);
        Button lootDrawSelect = new(gui, new(Constants.Middle.X + 100, 10), new(90, 30), Color.White, Color.Black * 0.6f, ColorTools.NearBlack * 0.6f, () => currentTool = EditorTool.Loot, [], "Loot", border: 0);
        Button enemyDrawSelect = new(gui, new(Constants.Middle.X + 200, 10), new(90, 30), Color.White, Color.Black * 0.6f, ColorTools.NearBlack * 0.6f, () => currentTool = EditorTool.Enemy, [], "Enemies", border: 0);
        Button npcDrawSelect = new(gui, new(Constants.Middle.X + 300, 10), new(90, 30), Color.White, Color.Black * 0.6f, ColorTools.NearBlack * 0.6f, () => currentTool = EditorTool.NPC, [], "NPCs", border: 0);
        
        toolHighlight = new(gui, new(Constants.Middle.X - 100, 10), new(90, 30), Color.Transparent, Color.White, 2);
        gui.AddWidgets(noDrawSelect, tileDrawSelect, decalDrawSelect, biomeDrawSelect, lootDrawSelect, enemyDrawSelect, npcDrawSelect, toolHighlight);

        // Settings button
        Button settingsButton = new(gui, new(Constants.NativeResolution.X - 100, Constants.NativeResolution.Y - 40), new(90, 30), Color.White, Color.Black * 0.6f, ColorTools.NearBlack * 0.6f, () => gameManager.StateManager.State = GameState.Settings, [], "Settings", border: 0);
        gui.AddWidget(settingsButton);

        // Palette selection
        tilesetGroup = new(gui);
        var tilesets = Enum.GetValues<TilesetTypes>();
        for (int t = 0; t < tilesets.Length; t++)
        {
            var tileset = tilesets[t];
            Button tilesetButton = new(gui, new(10, t * 35 + 10), new(100, 30), Color.White, Color.Black * 0.6f, ColorTools.NearBlack * 0.6f, () => TilesetSelection = tileset, [], tileset.ToString(), border: 0);
            tilesetGroup.AddWidget(tilesetButton);
        }
        tilesetHighlight = new(gui, new(10, (int)TilesetSelection * 35 + 10), new(100, 30), Color.Transparent, Color.White, 2);
        tilesetGroup.AddWidget(tilesetHighlight);

        itemsetGroup = new(gui);
        var itemsets = Enum.GetValues<ItemsetTypes>();
        for (int i = 0; i < itemsets.Length; i++)
        {
            var itemset = itemsets[i];
            Button itemsetButton = new(gui, new(10, i * 35 + 10), new(100, 30), Color.White, Color.Black * 0.6f, ColorTools.NearBlack * 0.6f, () => ItemsetSelection = itemset, [], itemset.ToString(), border: 0);
            itemsetGroup.AddWidget(itemsetButton);
        }
        itemsetHighlight = new(gui, new(10, (int)ItemsetSelection * 35 + 10), new(100, 30), Color.Transparent, Color.White, 2);
        itemsetGroup.AddWidget(itemsetHighlight);

        gui.AddWidgets(tilesetGroup, itemsetGroup);

        // Add no draw zones from buttons
        List<Rectangle> rects = [];
        foreach (var widget in gui.Widgets.Concat(tilesetGroup.Widgets).Concat(itemsetGroup.Widgets).Concat(SettingsMenu.Widgets))
        {
            if (widget is Button button)
                rects.Add(button.Rect);
        }
        noDrawZones = [.. rects];

        gui.LoadContent();
        Logger.System("Initialized GUI.");

        // Other
        CursorArrow = Content.Load<Texture2D>("Images/Gui/CursorArrow");

        // Timer
        TimerManager.NewTimer("FrameTimeUpdate", 1, false, editorOverlayManager.UpdateFrameTimes, int.MaxValue);

        // Final
        Logger.System("Level editor finished initializing.");
    }

    protected override void Update(GameTime gameTime)
    {
        DebugManager.Watch.Restart();

        // Exit
        if (InputManager.KeyPressed(Keys.LeftAlt) && InputManager.KeyPressed(Keys.Escape)) Exit();

        // Delta
        delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Mouse
        mouseCoord = CameraManager.ScreenToTile(InputManager.MousePosition);
        mouseCoord.X = Math.Clamp(mouseCoord.X, 0, Constants.MapSize.X - 1);
        mouseCoord.Y = Math.Clamp(mouseCoord.Y, 0, Constants.MapSize.Y - 1);
        mouseTile = levelManager.GetTile(mouseCoord)!;
        mouseDecal = levelManager.GetDecal(mouseCoord.ToByteCoord())?.Type;
        mouseBiome = levelManager.Level.Biome[LevelManager.Flatten(mouseCoord)];

        // Movement
        DebugManager.StartBenchmark("InputUpdate");
        int speedup = InputManager.BindDown(InputAction.FastMove) ? 8 : 3;
        moveX = 0; moveY = 0;
        moveX += InputManager.BindDown(InputAction.MoveLeft) ? -Constants.PlayerBaseSpeed : 0;
        moveX += InputManager.BindDown(InputAction.MoveRight) ? Constants.PlayerBaseSpeed : 0;
        moveY += InputManager.BindDown(InputAction.MoveUp) ? -Constants.PlayerBaseSpeed : 0;
        moveY += InputManager.BindDown(InputAction.MoveDown) ? Constants.PlayerBaseSpeed : 0;
        CameraManager.CameraDest += new Vector2(moveX, moveY) * delta * speedup;
        DebugManager.EndBenchmark("InputUpdate");

        // Manager
        editorManager.Update(TileSelection, TileSelectionData, BiomeSelection, currentTool, delta, mouseTile, mouseCoord, mouseSelection, mouseSelectionCoord);
        if (gameManager.StateManager.State == GameState.Settings)
            SettingsMenu.Update(delta, InputManager.MouseState, InputManager.KeyboardState);

        // Mouse selection coord
        if (InputManager.RMouseClicked) MouseSelect();

        // Change material
        if (InputManager.ScrolledUp || InputManager.BindPressed(InputAction.CycleToolNext))
        {
            if (currentTool == EditorTool.Tile) TileSelectionIdx = TileSelectionIdx - 1 + (TileSelectionIdx <= 0 ? Tilesets.TypeToArray[TilesetSelection].Length : 0);
            else if (currentTool == EditorTool.Decal) NumberTools.CycleDown(ref DecalSelection);
            else if (currentTool == EditorTool.Biome) NumberTools.CycleDown(ref BiomeSelection);
            else if (currentTool == EditorTool.Loot) ItemSelectionIdx = ItemSelectionIdx - 1 + (ItemSelectionIdx <= 0 ? Itemsets.TypeToArray[ItemsetSelection].Length : 0);
            else if (currentTool == EditorTool.Enemy) NumberTools.CycleDown(ref EnemySelection);
            else if (currentTool == EditorTool.NPC) NPCSelectionIdx = NPCSelectionIdx - 1 + (NPCSelectionIdx <= 0 ? TypeTextures[TextureType.Character].Length : 0);
            TileSelectionData = [];
        }
        if (InputManager.ScrolledDown || InputManager.BindPressed(InputAction.CycleToolPrevious))
        {
            if (currentTool == EditorTool.Tile) TileSelectionIdx = (TileSelectionIdx + 1) % Tilesets.TypeToArray[TilesetSelection].Length;
            else if (currentTool == EditorTool.Decal) NumberTools.CycleUp(ref DecalSelection);
            else if (currentTool == EditorTool.Biome) NumberTools.CycleUp(ref BiomeSelection);
            else if (currentTool == EditorTool.Loot) ItemSelectionIdx = (ItemSelectionIdx + 1) % Itemsets.TypeToArray[ItemsetSelection].Length;
            else if (currentTool == EditorTool.Enemy) NumberTools.CycleUp(ref EnemySelection);
            else if (currentTool == EditorTool.NPC) NPCSelectionIdx = (NPCSelectionIdx + 1) % TypeTextures[TextureType.Character].Length;
            TileSelectionData = [];
        }

        // Change item amount
        if (InputManager.BindPressed(InputAction.IncreaseLootTool))
            LootAmountSelection++;
        if (InputManager.BindPressed(InputAction.DecreaseLootTool))
            LootAmountSelection--;


        // Change tileset
        if (InputManager.BindPressed(InputAction.CycleTilesetNext))
        {
            if (currentTool == EditorTool.Loot) ItemsetSelection = NumberTools.CycleDown(ItemsetSelection);
            else TilesetSelection = NumberTools.CycleDown(TilesetSelection); // Non-ref version since TilesetSelection is a property
        }
        if (InputManager.BindPressed(InputAction.CycleTilesetPrevious))
        {
            if (currentTool == EditorTool.Loot) ItemsetSelection = NumberTools.CycleUp(ItemsetSelection);
            else TilesetSelection = NumberTools.CycleUp(TilesetSelection); // Non-ref version since TilesetSelection is a property
        }

        // Placing tiles
        UpdatePlacing();

        // Edit options
        if (InputManager.BindPressed(InputAction.EditTile)) editorManager.EditTile();

        // Pick
        if (InputManager.BindPressed(InputAction.PickTile)) PickTile();
        // Open file
        if (InputManager.BindPressed(InputAction.OpenLevel)) editorLevelManager.OpenLevelDialog();
        // Fill
        if (InputManager.BindPressed(InputAction.FloodFill)) editorManager.FloodFill();
        // NPCs
        if (InputManager.BindPressed(InputAction.DeleteNPC)) { MouseSelect(); editorManager.DeleteNPC(); }
        else if (InputManager.BindPressed(InputAction.EditNPC)) { MouseSelect(); editorManager.EditNPC(); }
        else if (InputManager.BindPressed(InputAction.NewNPC)) { MouseSelect(); editorManager.NewNPC(); }
        // Loot
        if (InputManager.BindPressed(InputAction.DeleteLoot)) { MouseSelect(); editorManager.DeleteLoot(); }
        else if (InputManager.BindPressed(InputAction.NewLoot)) { MouseSelect(); editorManager.NewLoot(); }
        // Decals
        if (InputManager.BindPressed(InputAction.DeleteDecal)) { MouseSelect(); editorManager.DeleteDecal(); }
        else if (InputManager.BindPressed(InputAction.NewDecal)) { MouseSelect(); editorManager.NewDecal(); }
        // Save
        if (InputManager.BindPressed(InputAction.SaveLevelAs)) editorLevelManager.SaveLevelAs();
        else if (InputManager.BindPressed(InputAction.SaveLevel)) editorLevelManager.SaveLevelDialog();
        // Level info
        if (InputManager.BindPressed(InputAction.SetSpawn)) editorManager.SetSpawn();
        if (InputManager.BindPressed(InputAction.SetTint)) editorManager.SetTint();
        // Generate level
        if (InputManager.BindPressed(InputAction.GenerateLevel)) editorLevelManager.GenerateLevel();
        // Resave
        if (InputManager.BindPressed(InputAction.ResaveWorld)) editorLevelManager.ResaveWorld(levelManager.Level.WorldName);
        else if (InputManager.BindPressed(InputAction.ResaveLevel)) editorLevelManager.ResaveLevel(levelManager.Level.LevelPath);
        // Tool select
        if (InputManager.BindPressed(InputAction.SelectTileTool)) currentTool = EditorTool.Tile;
        if (InputManager.BindPressed(InputAction.SelectDecalTool)) currentTool = EditorTool.Decal;
        if (InputManager.BindPressed(InputAction.SelectBiomeTool)) currentTool = EditorTool.Biome;
        if (InputManager.BindPressed(InputAction.SelectLootTool)) currentTool = EditorTool.Loot;
        if (InputManager.BindPressed(InputAction.SelectEnemyTool)) currentTool = EditorTool.Enemy;
        if (InputManager.BindPressed(InputAction.SelectNPCTool)) currentTool = EditorTool.NPC;
        if (InputManager.BindPressed(InputAction.DeselectTool)) currentTool = EditorTool.None;
        // Waypoints
        if (InputManager.BindPressed(InputAction.NewWaypoint)) { MouseSelect(); editorManager.NewWaypoint(); }
        if (InputManager.BindPressed(InputAction.DeleteWaypoint)) { MouseSelect(); editorManager.DeleteWaypoint(); }
        // Transitions
        if (InputManager.BindPressed(InputAction.NewTransition)) { MouseSelect(); editorManager.NewTransition(); }
        if (InputManager.BindPressed(InputAction.DeleteTransition)) { MouseSelect(); editorManager.DeleteTransition(); }
        // Script
        if (InputManager.BindPressed(InputAction.DeleteScript)) editorManager.DeleteScript();
        else if (InputManager.BindPressed(InputAction.NewScript)) editorManager.NewScript();
        // New Level
        if (InputManager.BindPressed(InputAction.NewLevel)) editorLevelManager.NewLevel();

        // Managers
        if (!PopupFactory.PopupOpen) InputManager.Update(this);
        DebugManager.Update(editorOverlayManager.GetDebugString().Replace("\n", "\r\n"), memoryDebugSb.ToString().Split("\n"));
        CameraManager.Update(gameManager, delta);
        CameraManager.CameraDest = Vector2.Clamp(CameraManager.CameraDest, Constants.Middle.ToVector2(), (Constants.MapSize * Constants.TileSize - Constants.Middle).ToVector2());

        // Gui
        gui.Update(delta, InputManager.MouseState, InputManager.KeyboardState);

        TimerManager.Update(gameManager);
        gameManager.Update(delta);
        levelManager.Update(gameManager);
        editorOverlayManager.Update();

        // Final
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Render to native-resolution render target (no scale matrix)
        GraphicsDevice.SetRenderTarget(Render);
        GraphicsDevice.Clear(Color.Magenta);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        Point mouseCoordDraw = CameraManager.TileToScreen(mouseCoord);

        // Draw game
        levelManager.Draw(gameManager);

        // Render biome markers
        DebugManager.StartBenchmark("DrawBiomes");
        if (editorManager.ShowBiomeMarkers)
            editorOverlayManager.DrawBiomes();
        DebugManager.EndBenchmark("DrawBiomes");

        // Render transitions
        DebugManager.StartBenchmark("DrawTransitions");
        editorOverlayManager.DrawTransitions();
        DebugManager.EndBenchmark("DrawTransitions");

        // Text info
        DebugManager.StartBenchmark("DebugTextDraw");
        editorOverlayManager.DrawTextInfo();

        // Program info
        Quest.Window.DrawMemoryInfo(memoryDebugSb, spriteBatch);

        // Frame info
        editorOverlayManager.DrawFrameInfo();
        DebugManager.EndBenchmark("DebugTextDraw");

        // Frame bar
        DebugManager.StartBenchmark("FrameBarDraw");
        if (DebugManager.FrameBar)
            editorOverlayManager.DrawFrameBar();
        DebugManager.EndBenchmark("FrameBarDraw");

        // Minimap
        if (!DebugManager.ProgramInfo)
        {
            editorOverlayManager.Minimap = OverlayManager.DrawMiniMap(GraphicsDevice, levelManager, editorOverlayManager.Minimap, spriteBatch, spriteBatch); // Same batch reused for minimap

            spriteBatch.Draw(editorOverlayManager.Minimap, new Vector2(8, Constants.NativeResolution.Y - Constants.MapSize.Y - 8), Color.White);
            spriteBatch.DrawRectangle(new(8, Constants.NativeResolution.Y - Constants.MapSize.Y - 8, Constants.MapSize.X, Constants.MapSize.Y), Color.Black, 3);
        }


        // Ghost tile cursor
        DrawGhostCursor(mouseCoordDraw);

        // Tile info
        if (currentTool == EditorTool.Tile)
            EditorOverlayManager.DrawTileOverlay(spriteBatch, PreviewTile, mouseTile);
        else if (currentTool == EditorTool.Decal)
            EditorOverlayManager.DrawDecalOverlay(spriteBatch, DecalSelection, mouseDecal);
        else if (currentTool == EditorTool.Biome)
            EditorOverlayManager.DrawBiomeOverlay(spriteBatch, BiomeSelection, mouseBiome);
        else if (currentTool == EditorTool.Loot)
            EditorOverlayManager.DrawLootOverlay(spriteBatch, ItemSelection, LootAmountSelection);
        else if (currentTool == EditorTool.Enemy)
            EditorOverlayManager.DrawEnemyOverlay(spriteBatch, EnemySelection);
        else if (currentTool == EditorTool.NPC)
            EditorOverlayManager.DrawNPCOverlay(spriteBatch, NPCSelection);

        // Gui
        if (gameManager.StateManager.State == GameState.Settings)
        {
            TextureManager.DrawTexture(spriteBatch, TextureID.QuestValleyBackground, Point.Zero, scale: MenuManager.MenuBackgroundScale);
            SettingsMenu.Draw();
        }
        else
        {
            // Main gui
            tilesetGroup.Visible = currentTool == EditorTool.Tile;
            tilesetHighlight.Location = new Point(10, (int)TilesetSelection * 35 + 10);
            itemsetGroup.Visible = currentTool == EditorTool.Loot;
            itemsetHighlight.Location = new Point(10, (int)ItemsetSelection * 35 + 10);

            toolHighlight.Visible = currentTool != EditorTool.None;
            toolHighlight.Location = new Point(Constants.Middle.X - 300 + (int)currentTool * 100, 10);

            gui.Draw();
        }

        // Cursor
        DrawTexture(spriteBatch, TextureID.CursorArrow, InputManager.MousePosition, color: Constants.SemiTransparent);

        // Finalize scene render
        spriteBatch.End();

        // Draw the native render to the backbuffer, scaling to current backbuffer size
        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Transparent);
        Rectangle dest = new(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        spriteBatch.Draw(Render, dest, Color.White);
        spriteBatch.End();

        base.Draw(gameTime);
    }
    public void DrawGhostCursor(Point mouseCoordDraw)
    {
        // Check skip
        if (gameManager.StateManager.State != GameState.Editor) return;

        // Tool ghosts
        if (currentTool == EditorTool.Tile)
        {
            // Check if preview tile needs to be updates
            if ((PreviewTile == null) ||
                (PreviewTile.Location.ToPoint() != mouseCoord) ||
                (PreviewTile.TypeID != TileSelection)
            )
                UpdatePreviewTile();


            // Render tile
            PreviewTile?.Draw(gameManager);
            spriteBatch.FillRectangle(new(mouseCoordDraw.ToVector2(), Constants.TileSize), Color.White * 0.2f);
        }
        else if (currentTool == EditorTool.Decal)
        {
            TextureID texture;
            if (InputManager.BindDown(InputAction.DeleteDecalMode))
                texture = TextureID.RedX;
            else
                texture = (TextureID)Enum.Parse(typeof(TextureID), DecalSelection.ToString());

            DrawTexture(spriteBatch, texture, mouseCoordDraw, source: new(Point.Zero, Constants.TilePixelSize), scale: Constants.TileSizeScale, color: Constants.SemiTransparent);
        }
        else if (currentTool == EditorTool.Biome)
        {
            DrawTexture(spriteBatch, TextureID.TileOutline, mouseCoordDraw, source: new(Point.Zero, Constants.TilePixelSize), scale: Constants.TileSizeScale, color: Biome.BiomeTileColors[(int)BiomeSelection]);
            Vector2 textCenter = Arial.MeasureString(BiomeSelection.ToString()) / 2;
            spriteBatch.DrawString(Arial, BiomeSelection.ToString(), (mouseCoordDraw + Constants.TileHalfSize).ToVector2(), Color.Black, MathHelper.PiOver4, textCenter, 1.0f, SpriteEffects.None, 1.0f);
        } else if (currentTool == EditorTool.Loot)
        {
            TextureID tex = ItemTypes.All[(int)ItemSelection].Texture;
            DrawTexture(spriteBatch, tex, InputManager.MousePosition, scale: new(2), color: Constants.SemiTransparent, source: new(Point.Zero, TextureManager.Metadata[tex].TileSize));
            spriteBatch.DrawString(PixelOperatorSmall, LootAmountSelection.ToString(), (InputManager.MousePosition + Loot.lootStackOffset * 5).ToVector2(), Color.White);
        } else if (currentTool == EditorTool.Enemy) {
            TextureID tex = Enemy.PresetTextures.GetValueOrDefault(EnemySelection);
            DrawTexture(spriteBatch, tex, InputManager.MousePosition, source: new(Point.Zero, TextureManager.Metadata[tex].TileSize), color: Constants.SemiTransparent, scale: new(2));
        }
        else if (currentTool == EditorTool.NPC)
        {
            DrawTexture(spriteBatch, NPCSelection, InputManager.MousePosition, source: new(Point.Zero, TextureManager.Metadata[NPCSelection].TileSize), color: Constants.SemiTransparent, scale: new(2));
        }
    }
    public void UpdatePlacing()
    {
        foreach (var noDraw in noDrawZones)
            if (noDraw.Contains(InputManager.MousePosition)) return;
 
        // Check skip
        if (!InputManager.LMouseDown || mouseMenu.Visible || gameManager.StateManager.State != GameState.Editor) return;

        // Add tile
        if (currentTool == EditorTool.Tile)
        {
            Tile tile = Tile.TileFromId(TileSelection, mouseCoord, LevelPath.Null);
            if (tile is IHasLevelData dataTile && TileSelectionData.Length > 0)
                dataTile.SetLevelData(TileSelectionData, levelManager.Level.LevelPath);

            editorManager.SetTile(tile);
        }
        // Add decal
        else if (currentTool == EditorTool.Decal)
        {
            Decal? current = levelManager.Level.Decals.TryGetValue(mouseCoord.ToByteCoord(), out var dec) ? dec : null;
            // Delete mode
            if (InputManager.BindDown(InputAction.DeleteDecalMode))
            {
                levelManager.Level.Decals.Remove(mouseCoord.ToByteCoord());
            }
            // Add mode
            else
            {
                // Check existing decal
                bool alreadyThere = current != null && current.Type == DecalSelection;
                if (current != null && current.Type != DecalSelection) levelManager.Level.Decals.Remove(mouseCoord.ToByteCoord()); // Remove current one

                // Add
                if (!alreadyThere && levelManager.Level.Decals.Count < ushort.MaxValue)
                    levelManager.Level.Decals[mouseCoord.ToByteCoord()] = LevelManager.DecalFromId(DecalSelection, mouseCoord);
            }
        }
        // Set biome
        else if (currentTool == EditorTool.Biome)
        {
            int idx = LevelManager.Flatten(mouseCoord);
            levelManager.Level.Biome[idx] = BiomeSelection;
        }
        // Make loot
        else if (InputManager.LMouseClicked && currentTool == EditorTool.Loot)
        {
            levelManager.Level.Loot.Add(new(new(ItemTypes.All[(int)ItemSelection], LootAmountSelection), CameraManager.ScreenToWorld(InputManager.MousePosition)));
        }
        // Make enemy
        else if (InputManager.LMouseClicked && currentTool == EditorTool.Enemy)
        {
            Enemy enemy = Enemy.GetPreset(EnemySelection, CameraManager.ScreenToWorld(InputManager.MousePosition).ToVector2());
            levelManager.Level.Enemies[enemy.UID] = enemy;
        }
        // Make NPC
        else if (InputManager.LMouseClicked && currentTool == EditorTool.NPC)
        {
            NPC npc = new(NPCSelection, CameraManager.ScreenToTile(InputManager.MousePosition), "NPC", "Hello!");
            levelManager.Level.NPCs[npc.UID] = npc;
        }
    }
    private void UpdatePreviewTile()
    {
        PreviewTile = Tile.TileFromId(TileSelection, mouseCoord, LevelPath.Null);

        if (PreviewTile is IHasLevelData dataTile && TileSelectionData.Length > 0)
            dataTile.SetLevelData(TileSelectionData, LevelPath.Null);
    }
    private void InvalidatePreviewTile() => PreviewTile = null;
    public void PickTile()
    {
        if (currentTool == EditorTool.Tile && mouseTile != null)
        {
            foreach (var (type, set) in Tilesets.TypeToArray)
            {
                int idx = Array.IndexOf(set, mouseTile.Type.ID);
                if (idx != -1)
                {
                    TilesetSelection = type;
                    TileSelectionIdx = idx;
                    if (mouseTile is IHasLevelData dataTile)
                        TileSelectionData = dataTile.GetLevelData();
                }
            }
        }
        else if (currentTool == EditorTool.Decal)
        {
            Decal? picked = levelManager.Level.Decals.TryGetValue(mouseCoord.ToByteCoord(), out var dec) ? dec : null;
            if (picked != null) DecalSelection = picked.Type;
        }
        else if (currentTool == EditorTool.Biome) BiomeSelection = levelManager.GetBiome(mouseCoord)!.Value;
        else if (currentTool == EditorTool.Loot)
        {
            Point mouseWorld = CameraManager.ScreenToWorld(InputManager.MousePosition);
            Loot? picked = levelManager.Level.Loot
                .Where(loot => Vector2.DistanceSquared(loot.Position.ToVector2(), mouseWorld.ToVector2()) < 900)
                .Select(loot => (Loot?)loot)
                .FirstOrDefault();
            if (picked.HasValue)
            {
                foreach (var (type, set) in Itemsets.TypeToArray)
                {
                    int idx = Array.IndexOf(set, picked.Value.Item.Type.TypeID);
                    if (idx != -1)
                    {
                        ItemsetSelection = type;
                        ItemSelectionIdx = idx;
                        break;
                    }
                }
            }
        }
    }
    public void MouseSelect()
    {
        mouseSelection = CameraManager.ScreenToWorld(InputManager.MousePosition);
        mouseSelectionCoord = mouseCoord;
        editorManager.Update(TileSelection, TileSelectionData, BiomeSelection, currentTool, delta, mouseTile, mouseCoord, mouseSelection, mouseSelectionCoord);
    }
    public static byte IntToByte(int value)
    {
        if (value < 0 || value > 255)
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be between 0 and 255.");
        return (byte)value;
    }

}
