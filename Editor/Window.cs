using System.IO.Compression;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework.Input;
using static Quest.Managers.TextureManager;

// TODO Player not being drawn, dialog box not being drawn, update minimap on level change, inventory not being drawn

namespace Quest.Editor;
public class Window : Game
{
    static readonly StringBuilder debugSb = new();
    // Devices and managers
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private GameManager gameManager;
    private TimerManager timerManager;
    private UIManager uiManager;
    private LevelManager levelManager;
    private MenuManager menuManager;

    // Editing
    private TileType Material;
    private int Selection;
    private Point mouseCoord;
    private readonly Color highlightColor = new(1, 1, 1, .8f);
    private Tile mouseTile;

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
        TextureManager.LoadTextures(Content);

        // Managers
        timerManager = new();
        uiManager = new();
        levelManager = new();
        menuManager = new();
        gameManager = new(spriteBatch, new(0, 0), levelManager, uiManager);

        // Levels
        levelManager.ReadLevel(gameManager.UIManager, "island_house");
        levelManager.ReadLevel(gameManager.UIManager, "island_house_basement");
        levelManager.LoadLevel(gameManager, 0);
        ReadLevel("island_house");

        // Shaders
        Grayscale = Content.Load<Effect>("Shaders/Grayscale");

        // Other
        CursorArrow = Content.Load<Texture2D>("Images/Gui/CursorArrow");

        // Timer
        timerManager.NewTimer("frameTimeUpdate", 1, UpdateFrameTimes, int.MaxValue);
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

        // Open file
        if (InputManager.AllKeysDown(Keys.O, Keys.LeftControl))
        {
            string filename = Logger.Input("Open level file: ");
            try {
                levelManager.ReadLevel(uiManager, filename);
                Logger.Log($"Opened level '{filename}'.");
            } catch (Exception ex) {
                Logger.Error($"Failed to open level '{filename}': {ex.Message}");
            }
        }

        // Change material
        if (InputManager.ScrollWheelChange > 0 || InputManager.KeyPressed(Keys.OemCloseBrackets)) {
            Selection = (Selection + 1) % Constants.TileNames.Length;
            Material = (TileType)Enum.Parse(typeof(TileType), Constants.TileNames[Selection]);
            Logger.Log($"Material set to '{Material}'.");
        } if (InputManager.ScrollWheelChange < 0 || InputManager.KeyPressed(Keys.OemCloseBrackets)) {
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
                tile = new Door(mouseCoord, Constants.Key);
            else
                tile = LevelManager.TileFromId(Selection, mouseCoord);

            SetTile(tile);
            Logger.Log($"Set tile to '{Material}' @ {mouseCoord.X}, {mouseCoord.Y}.");
        }
        // Edit options
        if (InputManager.RMouseClicked) EditTile(mouseTile);

        // Erase (set to sky)
        if (InputManager.MMouseClicked && mouseTile.Type != TileType.Sky)
        {
            SetTile(new Sky(mouseCoord));
            Logger.Log($"Set tile to '{Material}' @ {mouseCoord.X}, {mouseCoord.Y}.");
        }

        // Fill
        if (InputManager.KeyPressed(Keys.F)) FloodFill();
        // NPCs
        if (InputManager.Hotkey(Keys.LeftControl, Keys.N)) EditNPCs();
        // Loot
        if (InputManager.Hotkey(Keys.LeftControl, Keys.L)) EditLoot();
        // Decals
        if (InputManager.Hotkey(Keys.LeftControl, Keys.D)) EditDecals();
        // Save
        if (InputManager.Hotkey(Keys.LeftControl, Keys.E)) SaveLevel();
        // Level info
        if (InputManager.Hotkey(Keys.LeftControl, Keys.I)) SetSpawn();
        if (InputManager.Hotkey(Keys.LeftControl, Keys.T)) SetTint();

        // Managers
        InputManager.Update();
        DebugManager.Update();
        CameraManager.Update(delta);

        timerManager.Update(gameManager);
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
        spriteBatch.FillRectangle(new(Constants.Window.X - 190, 0, 190, boxHeight), Color.Black * 0.8f);

        debugSb.Clear();
        foreach (var kv in frameTimes)
        {
            debugSb.Append(kv.Key);
            debugSb.Append(": ");
            debugSb.AppendFormat("{0:0.0}ms", kv.Value);
            debugSb.Append('\n');
        }

        spriteBatch.DrawString(TextureManager.Arial, debugSb.ToString(), new Vector2(Constants.Window.X - 180, 10), Color.White);
    }
    public void DrawTextInfo()
    {
        spriteBatch.FillRectangle(new(0, 0, 200, 180), Color.Black * 0.8f);

        debugSb.Clear();
        debugSb.Append("FPS: ");
        debugSb.AppendFormat("{0:0.0}", cacheDelta != 0 ? 1f / cacheDelta : 0);
        debugSb.Append("\nTime: ");
        debugSb.AppendFormat("{0:0.00}", gameManager.TotalTime);
        debugSb.Append("\nCamera: ");
        debugSb.AppendFormat("{0:0.0},{1:0.0}", CameraManager.Camera.X, CameraManager.Camera.Y);
        debugSb.Append("\nTile Below: ");
        debugSb.Append(mouseTile == null ? "none" : mouseTile.Type);
        debugSb.Append("\nCoord: ");
        debugSb.AppendFormat("{0:0.0},{1:0.0}", CameraManager.TileCoord.X, CameraManager.TileCoord.Y);
        debugSb.Append("\nLevel: ");
        debugSb.Append(levelManager.Level?.Name);
        debugSb.Append("\nInventory: ");
        debugSb.Append(gameManager.Inventory.Opened);
        debugSb.Append("\nGUI: ");
        debugSb.Append(uiManager.Gui.Widgets.Count);

        spriteBatch.DrawString(TextureManager.Arial, debugSb.ToString(), new Vector2(10, 10), Color.White);
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
            spriteBatch.DrawString(TextureManager.Arial, process.Key, new(Constants.Window.X - TextureManager.Arial.MeasureString(process.Key).X - 5, Constants.Window.Y - 20 * c - 60), colors[c]);
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
    public void SetTile(Tile tile)
    {
        levelManager.Level.Tiles[tile.Location.X + tile.Location.Y * Constants.MapSize.X] = tile;
    }
    public static void EditTile(Tile tile)
    {
        if (tile is Stairs stairs)
        {
            // Destination level
            Logger.Print("__Editing Stairs__");
            string resp = Logger.Input($"Dest level [{stairs.DestLevel}]: ");
            if (resp != "") stairs.DestLevel = resp;

            // Destination position
            resp = Logger.Input($"Dest position [{stairs.DestPosition.X}, {stairs.DestPosition.Y}]: ");
            if (resp != "")
            {
                string[] parts = resp.Split(',');
                if (parts.Length == 2 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
                    if (x >= 0 && x < Constants.MapSize.X && y >= 0 && y < Constants.MapSize.Y)
                        stairs.DestPosition = new(x, y);
                    else
                        Logger.Error($"Position out of bounds - must be within the map size {Constants.MapSize.X}x{Constants.MapSize.Y}.");
                else
                    Logger.Error("Invalid position format - use 'x,y'.");
            }
        }
        else if (tile is Door door)
        {
            Logger.Print("__Editing Door__");
            string name = Logger.Input($"Key name [{door.Key.Name}]: ");
            string description = Logger.Input($"Key description [{door.Key.Description[..Math.Min(12, door.Key.Description.Length - 1)]}...]: ");
            door.Key = new Item(name, description, 1, 1);
        }
    }
    public void FloodFill()
    {
        // Fill with current material
        int count = 0;
        Tile tileBelow = GetTile(mouseCoord);
        if (tileBelow.Type != Material)
        {
            Queue<Tile> queue = new();
            HashSet<Point> visited = []; // Track visited tiles
            queue.Enqueue(tileBelow);
            count++;
            while (queue.Count > 0)
            {
                Tile current = queue.Dequeue();
                if (current.Type == Material || visited.Contains(current.Location)) continue; // Skip if already filled
                count++;
                SetTile(LevelManager.TileFromId(Selection, current.Location));
                visited.Add(current.Location); // Mark as visited
                // Check neighbors
                foreach (Point neighbor in Constants.NeighborTiles)
                {
                    Point neighborCoord = current.Location + neighbor;
                    if (neighborCoord.X < 0 || neighborCoord.X >= Constants.MapSize.X || neighborCoord.Y < 0 || neighborCoord.Y >= Constants.MapSize.Y) continue;
                    Tile neighborTile = GetTile(neighborCoord);
                    if (neighborTile.Type == tileBelow.Type && neighborTile.Type != Material)
                    {
                        queue.Enqueue(neighborTile);
                    }
                }
            }
            Logger.Log($"Filled {count} tiles with '{Material}' starting from {mouseCoord.X}, {mouseCoord.Y}.");
        }
    }
    public void DrawMiniMap()
    {
        spriteBatch.DrawRectangle(new(7, Constants.Window.Y - Constants.MapSize.Y - 13, Constants.MapSize.X + 6, Constants.MapSize.Y + 6), Color.Black, 3);
        for (int y = 0; y < Constants.MapSize.Y; y++)
        {
            for (int x = 0; x < Constants.MapSize.X; x++)
            {
                // Get tile
                Tile tile = GetTile(new Point(x, y))!;
                spriteBatch.DrawPoint(new(10 + x, Constants.Window.Y - Constants.MapSize.Y - 10 + y), Constants.MiniMapColors[(int)tile.Type]);
            }
        }
        // Player
        Point dest = CameraManager.TileCoord + new Point(10, Constants.Window.Y - Constants.MapSize.Y - 10);
        spriteBatch.DrawPoint(dest.ToVector2(), Color.Red, size: 2);
    }
    public void SetSpawn()
    {
        // Destination position
        string resp = Logger.Input($"Spawn position [{levelManager.Level.Spawn.X}, {levelManager.Level.Spawn.Y}]: ");
        if (resp != "")
        {
            string[] parts = resp.Split(',');
            if (parts.Length == 2 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
                if (x >= 0 && x < Constants.MapSize.X && y >= 0 && y < Constants.MapSize.Y)
                    levelManager.Level.Spawn = new(x, y);
                else
                    Logger.Error($"Position out of bounds - must be within the map size {Constants.MapSize.X}x{Constants.MapSize.Y}.");
            else
                Logger.Error("Invalid position format - use 'x,y'.");
        }
    }
    public void SetTint()
    {
        Color current = levelManager.Level.Tint;
        Color tint = Logger.InputColor($"Tint (R,G,B,A) [{current.R},{current.G},{current.B},{current.A}]: ", Color.Transparent);
        levelManager.Level.Tint = tint;
    }
    public void EditNPCs()
    {
        if (InputManager.KeyDown(Keys.LeftShift)) // Delete
        {
            foreach (NPC npc in levelManager.Level.NPCs)
            {
                if (npc.Location == mouseCoord)
                {
                    levelManager.Level.NPCs.Remove(npc);
                    Logger.Log($"Deleted NPC '{npc.Name}' @ {mouseCoord.X}, {mouseCoord.Y}.");
                    break;
                }
            }
        }
        else // New
        {
            Logger.Print("__NPC__");
            string name = Logger.Input("Name: ");
            string dialog = Logger.Input("Dialog: ");
            int scale = Logger.InputInt("Size: ", fallback: 1);
            if (scale <= 0 || scale > 25.5)
            {
                Logger.Warning("Scale must be between 1 and 25.5- setting to 1");
                scale = 1;
            }
            TextureID texture = Logger.InputTexture("Texture: ", fallback: TextureID.PurpleWizard);
            levelManager.Level.NPCs.Add(new NPC(uiManager, texture, mouseCoord, name, dialog, Color.White, scale));
        }
    }
    public void EditDecals()
    {
        if (InputManager.KeyDown(Keys.LeftShift)) // Delete
        {
            foreach (Decal decal in levelManager.Level.Decals)
            {
                if (decal.Location == mouseCoord)
                {
                    levelManager.Level.Decals.Remove(decal);
                    Logger.Log($"Deleted decal  @ {mouseCoord.X}, {mouseCoord.Y}.");
                    break;
                }
            }
        }
        else // New
        {
            Logger.Print("__Decal__");
            string name = Logger.Input("Decal: ");
            int decal = (int)(Enum.TryParse<DecalType>(name, true, out var dec) ? dec : DecalType.Torch);
            levelManager.Level.Decals.Add(LevelManager.DecalFromId(decal, mouseCoord));
        }
    }
    public void EditLoot()
    {
        if (InputManager.KeyDown(Keys.LeftShift)) // Delete
        {
            foreach (Loot loot in levelManager.Level.Loot)
            {
                if (loot.Location == mouseCoord)
                {
                    levelManager.Level.Loot.Remove(loot);
                    Logger.Log($"Deleted loot '{loot.Item.DisplayText}' @ {mouseCoord.X}, {mouseCoord.Y}.");
                    break;
                }
            }
        }
        else // New
        {
            Logger.Print("__Loot__");
            string name = Logger.Input("Name: ");
            string description = Logger.Input("Description: ");
            byte amount = Logger.InputByte("Amount: ", fallback: 1);
            byte max = Logger.InputByte("Max: ", fallback: Constants.MaxStack);
            levelManager.Level.Loot.Add(new Loot(new Item(name, description, amount, max), InputManager.MousePosition + CameraManager.Camera.ToPoint() - Constants.Middle, gameManager.TotalTime));
        }
    }
    public void ReadLevel(string filename)
    {
        // Check exists
        if (!File.Exists($"Levels/{filename}.lvl"))
            throw new FileNotFoundException("Level file not found.", filename);

        // Parse
        Tile[] tilesBuffer;
        using FileStream fileStream = File.OpenRead($"Levels/{filename}.lvl");
        using GZipStream gzipStream = new(fileStream, CompressionMode.Decompress);
        using BinaryReader reader = new(gzipStream);
        // Make buffers
        tilesBuffer = new Tile[Constants.MapSize.X * Constants.MapSize.Y];
        List<NPC> npcBuffer = [];
        List<Loot> lootBuffer = [];
        List<Decal> decalBuffer = [];

        // Tint
        Color tint = new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());

        // Spawn
        Point spawn = new(reader.ReadByte(), reader.ReadByte());
        CameraManager.Camera = (spawn * Constants.TileSize).ToVector2();

        // Tiles
        for (int i = 0; i < Constants.MapSize.X * Constants.MapSize.Y; i++)
        {
            // Read tile data
            int type = reader.ReadByte();
            // Check if valid tile type
            if (type < 0 || type >= Enum.GetValues(typeof(TileType)).Length)
                throw new ArgumentException($"Invalid tile type {type} @ {i % Constants.MapSize.X}, {i / Constants.MapSize.X} in level file.");
            // Extra properties
            Tile tile;
            if (type == (int)TileType.Stairs)
                tile = new Stairs(new(i % Constants.MapSize.X, i / Constants.MapSize.X), reader.ReadString(), new(reader.ReadByte(), reader.ReadByte()));
            else if (type == (int)TileType.Door)
                tile = new Door(new(i % Constants.MapSize.X, i / Constants.MapSize.X), new Item(reader.ReadString(), reader.ReadString(), 1, 1));
            else // Regular tile
                tile = LevelManager.TileFromId(type, new(i % Constants.MapSize.X, i / Constants.MapSize.X));
            int idx = tile.Location.X + tile.Location.Y * Constants.MapSize.X;
            tilesBuffer[idx] = tile;
        }

        // NPCs
        byte npcCount = reader.ReadByte();
        for (int n = 0; n < npcCount; n++)
        {
            string name = reader.ReadString();
            string dialog = reader.ReadString();
            Point location = new(reader.ReadByte(), reader.ReadByte());
            int scale = reader.ReadByte();
            TextureID texture = (TextureID)reader.ReadByte();
            npcBuffer.Add(new NPC(uiManager, texture, location, name, dialog, Color.White, scale / 10f));
        }

        // Loot
        byte lootCount = reader.ReadByte();
        for (int n = 0; n < lootCount; n++)
        {
            string name = reader.ReadString();
            string description = reader.ReadString();
            byte amount = reader.ReadByte();
            byte max = reader.ReadByte();
            Point location = new(reader.ReadUInt16(), reader.ReadUInt16());
            lootBuffer.Add(new Loot(new Item(name, description, amount, max), location, gameManager.TotalTime));
        }

        // Decals
        byte decalCount = reader.ReadByte();
        for (int n = 0; n < decalCount; n++)
        {
            DecalType type = (DecalType)reader.ReadByte();
            Point location = new(reader.ReadUInt16(), reader.ReadUInt16());
            decalBuffer.Add(LevelManager.DecalFromId(reader.ReadByte(), location));
        }

        // Check null
        if (tilesBuffer == null)
            throw new ArgumentException("No tiles found in level file.");
        // Check size
        if (tilesBuffer.Length != Constants.MapSize.X * Constants.MapSize.Y)
            throw new ArgumentException($"Invalid level size - expected {Constants.MapSize.X}x{Constants.MapSize.X} tiles.");

        // Make and add the level
        levelManager.LoadLevelObject(gameManager, new(filename, tilesBuffer, spawn, npcBuffer, lootBuffer, decalBuffer, [], tint));
    }
    public void SaveLevel()
    {
        // Input
        string name = Logger.Input("Export file name: ");

        // Parse
        Directory.CreateDirectory("..\\..\\..\\Levels");
        using FileStream fileStream = File.Create($"..\\..\\..\\Levels/{name}.lvl");
        using GZipStream gzipStream = new(fileStream, CompressionLevel.Optimal);
        using BinaryWriter writer = new(gzipStream);

        // Write tint
        writer.Write(levelManager.Level.Tint.R);
        writer.Write(levelManager.Level.Tint.G);
        writer.Write(levelManager.Level.Tint.B);
        writer.Write(levelManager.Level.Tint.A);

        // Write spawn
        writer.Write(IntToByte(levelManager.Level.Spawn.X));
        writer.Write(IntToByte(levelManager.Level.Spawn.Y));

        // Tiles
        for (int i = 0; i < Constants.MapSize.X * Constants.MapSize.Y; i++)
        {
            Tile tile = levelManager.Level.Tiles[i];
            // Write tile data
            writer.Write(IntToByte((int)tile.Type));
            // Extra properties
            if (tile is Stairs stairs)
            {
                // Write destination
                writer.Write(stairs.DestLevel);
                writer.Write(IntToByte(stairs.DestPosition.X));
                writer.Write(IntToByte(stairs.DestPosition.Y));
            }
            else if (tile is Door door)
            {
                // Write door key
                writer.Write(door.Key.Name);
                writer.Write(door.Key.Description);
            }
        }

        // NPCs
        writer.Write((byte)Math.Min(levelManager.Level.NPCs.Count, 255));
        for (int n = 0; n < Math.Min(levelManager.Level.NPCs.Count, 255); n++)
        {
            NPC npc = levelManager.Level.NPCs[n];
            // Write NPC data
            writer.Write(npc.Name);
            writer.Write(npc.Dialog);
            writer.Write(IntToByte(npc.Location.X));
            writer.Write(IntToByte(npc.Location.Y));
            writer.Write(IntToByte((int)(npc.Scale * 10)));
            writer.Write(IntToByte((int)npc.Texture));
        }

        // Floor loot
        writer.Write((byte)Math.Min(levelManager.Level.Loot.Count, 255));
        for (int n = 0; n < Math.Min(levelManager.Level.Loot.Count, 255); n++)
        {
            Loot loot = levelManager.Level.Loot[n];
            // Write NPC data
            writer.Write(loot.Item.Name);
            writer.Write(loot.Item.Description);
            writer.Write(loot.Item.Amount);
            writer.Write(loot.Item.Max);
            writer.Write((UInt16)loot.Location.X);
            writer.Write((UInt16)loot.Location.Y);
        }

        // Decals
        writer.Write((byte)Math.Min(levelManager.Level.Decals.Count, 255));
        for (int n = 0; n < Math.Min(levelManager.Level.Decals.Count, 255); n++)
        {
            Decal decal = levelManager.Level.Decals[n];
            // Write decal data
            writer.Write((byte)decal.Type);
            writer.Write(IntToByte(decal.Location.X));
            writer.Write(IntToByte(decal.Location.Y));
        }
    }
    public Tile GetTile(Point coord)
    {
        if (coord.X < 0 || coord.X >= Constants.MapSize.X || coord.Y < 0 || coord.Y >= Constants.MapSize.Y)
            throw new ArgumentOutOfRangeException(nameof(coord), "Coordinates are out of bounds of the level.");
        return levelManager.Level.Tiles[coord.X + coord.Y * Constants.MapSize.X];
    }
    public static byte IntToByte(int value)
    {
        if (value < 0 || value > 255)
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be between 0 and 255.");
        return (byte)value;
    }
}
