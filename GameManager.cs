using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Quest.Gui;
using Quest.Tiles;

// TODO
// Check inventory placement when picking up items - DONE
// Lower range of item pickup - DONE
// Add debug toggle hotkeys f1-f6
// Add messages when items picked up

namespace Quest;


public interface IGameManager
{
    void DropLoot(Loot loot);
    float Delta { get; }
    void Notification(string message, Color? color = null, float duration = 4f);
    void AddLoot(Loot loot);
    bool IsKeyDown(Keys key);
    bool IsKeyPressed(Keys key);
    Inventory Inventory { get; }
    bool Playing { get; }
    Point PlayerFoot { get; }
    ContentManager Content { get; }
    SpriteFont PixelOperator { get; }
    float Time { get; }
    GuiManager Gui { get; }
    Vector2 Camera { get; set; }
    Vector2 CameraDest { get; set; }
    SpriteBatch Batch { get; }
    Rectangle TileTextureSource(Tile tile);
    void LoadLevel(string levelName);
    void LoadLevel(int levelIndex);
}
public class GameManager : IGameManager
{
    // Debug
    public Stopwatch Watch { get; private set; }
    public Tile? TileBelow { get; private set; }
    public Point TileCoord { get; private set; }
    public Point PlayerFoot { get; private set; }
    public Dictionary<string, double> FrameTimes { get; private set; }
    // Properties
    public void AddLoot(Loot loot) => Level.Loot.Add(loot);
    public bool IsKeyDown(Keys key) => Window.IsKeyDown(key);
    public bool IsKeyPressed(Keys key) => Window.IsKeyPressed(key);
    public Point CameraOffset => (CameraDest - Camera).ToPoint();
    public bool Playing => !Inventory.Opened;
    public Random Rand { get; private set; }
    public GuiManager Gui { get; private set; } // GUI handler
    public NotificationArea LootNotifications { get; private set; } // Loot pickup notifications
    public Window Window { get; private set; }
    public Vector2 Camera { get; set; } // Current camera position w/ smooth movement
    public Vector2 CameraDest { get; set; } // Where the camera is going
    public float Delta { get; private set; }
    public SpriteBatch Batch { get; private set; }
    public List<Level> Levels { get; private set; }
    public Level Level { get; private set; }
    public Dictionary<string, Texture2D> TileTextures { get; private set; }
    public SpriteFont PixelOperator { get; private set; }
    public SpriteFont PixelOperatorBold { get; private set; }
    public ContentManager Content => Window.Content;
    // Inventory
    public Inventory Inventory { get; set; }
    // Private
    private Point tileSize;
    public float Time { get; private set; }
    public static readonly Point lootStackOffset = new(4, 4);
    public GameManager(Window window, SpriteBatch spriteBatch)
    {
        // Initialize the game
        Window = window;
        Batch = spriteBatch;
        tileSize = Constants.TileSize;
        Levels = [];
        TileTextures = [];
        Camera = (Constants.MapSize * Constants.TileSize - Constants.Middle).ToVector2();
        CameraDest = Camera;
        Gui = new();
        Watch = new();
        FrameTimes = [];
        Rand = new();

        // Inventory
        Inventory = new(this, 6, 4);
        Inventory.SetSlot(0, new Item("Sword", "A sharp, pointy sword", max: 1));
        Inventory.SetSlot(1, new Item("Pickaxe", "Sturdy iron pickaxe for mining", max: 1));
        Inventory.SetSlot(2, new Item("ActivePalantir", "A seeing stone, used to communicate with Sauron", 1, max: 1));
        Inventory.SetSlot(3, new Item("InactivePalantir", "A seeing stone, used to communicate with Sauron", 1, max: 1));
        Inventory.SetSlot(4, new Item("PhiCoin", "A small copper coin", 20, max: 20));
        Inventory.SetSlot(5, new Item("DeltaCoin", "A shiny gold coin", 10, max: 20));
        Inventory.SetSlot(6, new Item("GammaCoin", "A rare diamond coin", 5, max: 20));

        // Loading
        PixelOperator = window.PixelOperator;
        PixelOperatorBold = window.PixelOperatorBold;

        // Widgets
        Gui.Widgets = [
            new StatusBar(new(10, Constants.Window.Y - 35), new(300, 25), Color.Green, Color.Red, 100, 100),
            LootNotifications = new NotificationArea(Constants.Middle - new Point(0, Constants.MageHalfSize.Y + 15), 5, PixelOperatorBold)
        ];

        // Level
        Level = new("null", [], new(0, 0), [], [], []); // Default level
    }
    public void Update(float deltaTime, MouseState previousMouseState, MouseState mouseState)
    {
        Delta = deltaTime;
        Time += deltaTime;

        UpdatePositions();
        UpdateCharacters(deltaTime);
        UpdateLoot(deltaTime);
        UpdateCamera(deltaTime);
        UpdateGui(deltaTime, previousMouseState, mouseState);
    }
    public void Draw()
    {
        DrawTiles();
        DrawDecals();
        DrawGui();
        DrawLoot();
        DrawCharacters();
        DrawPostProcessing();
    }
    // Draw split up methods
    #region
    public void DrawTiles()
    {
        // Tiles
        Watch.Restart();
        if (Level.Tiles == null || Level.Tiles.Length == 0) return;

        // Get bounds
        Point start = (Camera.ToPoint() - Constants.Middle) / Constants.TileSize;
        Point end = (Camera.ToPoint() + Constants.Middle) / Constants.TileSize;

        // Iterate
        for (int y = start.Y; y <= end.Y; y++)
        {
            for (int x = start.X; x <= end.X; x++)
            {
                GetTile(x, y)?.Draw(this);
            }
        }
        FrameTimes["TileDraws"] = Watch.Elapsed.TotalMilliseconds;
    }
    public void DrawDecals()
    {
        Watch.Restart();
        // Draw each decal
        foreach (Decal decal in Level.Decals)
            decal.Draw(this);
        FrameTimes["DecalDraws"] = Watch.Elapsed.TotalMilliseconds;
    }
    public void DrawGui()
    {
        // Widgets
        Watch.Restart();
        LootNotifications.Offset = (CameraDest - Camera).ToPoint();
        Gui.Draw(Batch);
        FrameTimes["GuiDraw"] = Watch.Elapsed.TotalMilliseconds;
        // Inventory
        Watch.Restart();
        Inventory.Draw();
        FrameTimes["InventoryDraw"] = Watch.Elapsed.TotalMilliseconds;
        // Debug
        Batch.DrawPoint(PlayerFoot.ToVector2() - Camera, Constants.DebugGreenTint, 3);
    }
    public void DrawLoot()
    {
        Watch.Restart();
        // Draw each
        for (int l = 0; l < Level.Loot.Count; l++)
        {
            Loot loot = Level.Loot[l];
            Point pos = loot.Location - Camera.ToPoint() + Constants.Middle;
            pos.Y += (int)(Math.Sin((Time - loot.Birth) * 2 % (Math.PI * 2)) * 6); // Bob up and down
            DrawTexture(Batch, loot.Texture, pos, scale: new(2));
            // Draw stacks if multiple
            if (loot.Item.Amount > 1)
                DrawTexture(Batch, loot.Texture, pos + lootStackOffset, scale: new(2));
            if (loot.Item.Amount > 2)
                DrawTexture(Batch, loot.Texture, pos + lootStackOffset, scale: new(2));
            // Draw hitbox if enabled
            if (Constants.DRAW_HITBOXES)
                Batch.FillRectangle(new(pos.ToVector2(), new(32, 32)), Constants.DebugPinkTint);
        }
        FrameTimes["DrawLoot"] = Watch.Elapsed.TotalMilliseconds;
    }
    public void DrawCharacters()
    {
        Watch.Restart();
        DrawPlayer();
        if (Constants.DRAW_HITBOXES)
            DrawPlayerHitbox();
        foreach (NPC npc in Level.NPCs) npc.Draw();
        FrameTimes["CharacterDraws"] = Watch.Elapsed.TotalMilliseconds;
    }
    public void DrawPlayer()
    {
        int sourceRow = 0;
        if (Window.moveX == 0 && Window.moveY == 0) sourceRow = 0;
        else if (Window.moveX < 0) sourceRow = 1;
        else if (Window.moveX > 0) sourceRow = 3;
        else if (Window.moveY > 0) sourceRow = 2;
        else if (Window.moveY < 0) sourceRow = 4;
        Rectangle source = new((int)(Time * (sourceRow == 0 ? 1.5f : 6)) % 4 * Constants.MageSize.X, sourceRow * Constants.MageSize.Y, Constants.MageSize.X, Constants.MageSize.Y);
        Point pos = Constants.Middle - Constants.MageHalfSize + CameraOffset;
        DrawTexture(Batch, TextureID.BlueMage, pos, source: source);
    }
    public void DrawPlayerHitbox()
    {
        Point[] points = new Point[4];
        for (int c = 0; c < Constants.PlayerCorners.Length; c++)
            points[c] = PlayerFoot + Constants.PlayerCorners[c] - Camera.ToPoint() + Constants.Middle;
        Batch.FillRectangle(new Rectangle(points[0].X, points[0].Y, points[1].X - points[0].X, points[2].Y - points[1].Y), Constants.DebugPinkTint);
    }
    public void DrawPostProcessing()
    {
        Watch.Restart();
        if (Level != null)
            Batch.FillRectangle(new(Vector2.Zero, Constants.Window), Level.Tint);
        if (Constants.DRAW_HITBOXES)
        {
            Batch.DrawCircle(new(Constants.Middle.ToVector2(), 3), 10, Constants.DebugGreenTint);
            Batch.DrawCircle(new(Constants.Middle.ToVector2() - new Vector2(0, Constants.MageHalfSize.Y + 12), 2), 10, Constants.DebugGreenTint);
        }
    }
    #endregion
    // Update split up methods
    #region
    public void UpdatePositions()
    {
        // Update player position
        PlayerFoot = CameraDest.ToPoint() + new Point(0, Constants.MageHalfSize.Y);
        TileCoord = PlayerFoot / Constants.TileSize;
        TileBelow = GetTile(TileCoord);
    }
    public void UpdateCharacters(float deltaTime)
    {
        foreach (NPC npc in Level.NPCs) npc.Update();
    }
    public void UpdateLoot(float deltaTime)
    {
        Watch.Restart();
        // Check if can pick up and search
        if (Inventory.IsFull()) return;
        for (int l = 0; l < Level.Loot.Count; l++)
        {
            Loot loot = Level.Loot[l];
            // Pick up loot
            if (PointTools.DistanceSquared(PlayerFoot, loot.Location + new Point(20, 20)) <= Constants.TileSize.X * Constants.TileSize.Y * .5f)
            {
                // Print notif before adding since amount will change
                LootNotifications.AddNotification($"+{loot.Item.DisplayText}");
                Inventory.AddItem(loot.Item);
                Level.Loot.RemoveAt(l);
            }
        }
        FrameTimes["UpdateLoot"] = Watch.Elapsed.TotalMilliseconds;
    }

    public void UpdateCamera(float deltaTime)
    {
        // Lerp camera
        Watch.Restart();
        if (Vector2.DistanceSquared(Camera, CameraDest) < 4 * deltaTime * 60) Camera = CameraDest; // If close enough snap to destination
        if (CameraDest != Camera) // If not, lerp towards destination
            Camera = Vector2.Lerp(Camera, CameraDest, 1f - MathF.Pow(1f - Constants.CameraRigidity, deltaTime * 60f));
        FrameTimes["CameraUpdate"] = Watch.Elapsed.TotalMilliseconds;
    }
    public void UpdateGui(float deltaTime, MouseState previousMouseState, MouseState mouseState)
    {
        // Gui
        Watch.Restart();
        Gui.Update(deltaTime);
        FrameTimes["GuiUpdate"] = Watch.Elapsed.TotalMilliseconds;

        // Inventory
        Watch.Restart();
        Inventory.Update(previousMouseState, mouseState);
        FrameTimes["InventoryUpdate"] = Watch.Elapsed.TotalMilliseconds;
    }
    #endregion
    public void Notification(string message, Color? color = null, float duration = 4f)
    {
        // Add a notification to the loot notifications area
        LootNotifications.AddNotification(message, color ?? Color.White, duration);
    }
    // Movements
    public void Move(Vector2 move)
    {
        // Move
        if (move == Vector2.Zero) return;
        Vector2 finalMove = Vector2.Normalize(move) * Delta * Constants.PlayerSpeed;

        // Stuck in block
        if (IsColliding()) return;

        // Check bump
        Point nextPoint = (CameraDest + finalMove).ToPoint();
        Tile? nextTile = GetTile(nextPoint / Constants.TileSize);
        if (nextTile != null && !nextTile.IsWalkable)
            nextTile.OnPlayerCollide(this);

        // Check collision for x
        CameraDest += new Vector2(finalMove.X, 0);
        if (IsColliding()) CameraDest -= new Vector2(finalMove.X, 0);
        // Check collision for y
        CameraDest += new Vector2(0, finalMove.Y);
        if (IsColliding()) CameraDest -= new Vector2(0, finalMove.Y);

        // On tile enter
        UpdatePositions();
        if (TileBelow == null) return;
        TileBelow.OnPlayerEnter(this);

        // Debug
        if (Constants.COLLISION_DEBUG) TileBelow.Marked = true;
    }
    public void DropLoot(Loot loot) { Level.Loot.Add(loot); }
    // Levels
    public void LoadLevel(int levelIndex)
    {
        // Check index
        if (levelIndex < 0 || levelIndex >= Levels.Count)
            throw new ArgumentOutOfRangeException(nameof(levelIndex), "Invalid level index.");

        // Close dialogs
        foreach (NPC npc in Level.NPCs)
        {
            npc.DialogBox.IsVisible = false;
            npc.DialogBox.Displayed = "";
        }

        // Load the level data
        Level = Levels[levelIndex];

        // Spawn
        CameraDest = (Level.Spawn * tileSize).ToVector2();
        Camera = CameraDest;

        // Reset minimap for redraw
        Window.Minimap = null;
    }
    public void LoadLevel(string levelName)
    {
        for (int l = 0; l < Levels.Count; l++)
        {
            if (Levels[l].Name == levelName)
            {
                LoadLevel(l);
                return;
            }
        }
        // If not found throw an error
        Logger.Error($"Level '{levelName}' not found in stored levels. Make sure the level file has been read before loading.", true);
    }
    public void ReadLevel(string filename)
    {
        // Check file exists
        if (!File.Exists($"Levels/{filename}.lvl"))
            throw new FileNotFoundException("Level file not found.", filename);

        // Check if already read
        foreach (Level level in Levels)
            if (level.Name == filename)
                return;

        // Get data
        string data = File.ReadAllText($"Levels/{filename}.lvl");
        string[] lines = data.Split('\n');

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

        // Spawn
        Point spawn = new(reader.ReadByte(), reader.ReadByte());
        CameraDest = ((spawn - Constants.MiddleCoord) * tileSize).ToVector2();
        Camera = CameraDest;

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
                tile = TileFromId(type, new(i % Constants.MapSize.X, i / Constants.MapSize.X));
            int idx = tile.Location.X + tile.Location.Y * Constants.MapSize.X;
            tilesBuffer[idx] = tile;
        }

        // NPCs
        int npcCount = reader.ReadByte();
        for (int n = 0; n < npcCount; n++)
        {
            string name = reader.ReadString();
            string dialog = reader.ReadString();
            Point location = new(reader.ReadByte(), reader.ReadByte());
            byte scale = reader.ReadByte();
            int texId = reader.ReadByte();
            TextureID texture = (TextureID)texId;
            npcBuffer.Add(new NPC(this, texture, location, name, dialog, Color.White, scale / 10f));
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
            lootBuffer.Add(new Loot(new Item(name, description, amount, max), location, Time));
        }

        // Decals
        byte decalCount = reader.ReadByte();
        for (int n = 0; n < decalCount; n++)
        {
            DecalType type = (DecalType)reader.ReadByte();
            Point location = new(reader.ReadByte(), reader.ReadByte());
            decalBuffer.Add(DecalFromId(reader.ReadByte(), location));
        }

        // Check null
        if (tilesBuffer == null)
            throw new ArgumentException("No tiles found in level file.");
        // Check size
        if (tilesBuffer.Length != Constants.MapSize.X * Constants.MapSize.Y)
            throw new ArgumentException($"Invalid level size - expected {Constants.MapSize.X}x{Constants.MapSize.X} tiles.");

        // Make and add the level
        Level created = new(filename, tilesBuffer, spawn, [.. npcBuffer], [.. lootBuffer], [.. decalBuffer]);
        Levels.Add(created);
    }
    // Utilities
    #region
    public bool IsColliding()
    {
        // Check if level loaded
        if (Level == null) return false;
        // Check 4 corners
        UpdatePositions();
        for (int o = 0; o < Constants.PlayerCorners.Length; o++)
        {
            // Check if the player collides with a tile
            Point coord = (PlayerFoot + Constants.PlayerCorners[o]) / Constants.TileSize;
            TileBelow = GetTile(coord);
            if (TileBelow == null || !TileBelow.IsWalkable) return true;
        }
        return false;
    }
    public static Tile TileFromId(int id, Point location)
    {
        // Create a tile from an id
        TileType type = (TileType)id;
        return type switch
        {
            TileType.Sky => new Sky(location),
            TileType.Grass => new Grass(location),
            TileType.Water => new Water(location),
            TileType.StoneWall => new StoneWall(location),
            TileType.Stairs => new Stairs(location, "_null", Constants.MiddleCoord),
            TileType.Flooring => new Flooring(location),
            TileType.Sand => new Sand(location),
            TileType.Dirt => new Dirt(location),
            TileType.Darkness => new Darkness(location),
            _ => new Tile(location), // Default tile
        };
    }
    public static Decal DecalFromId(int id, Point location)
    {
        // Create a decal from an id
        DecalType type = (DecalType)id;
        return type switch
        {
            DecalType.Torch => new Torch(location),
            _ => new Decal(location), // Default tile
        };
    }
    public int Flatten(Point pos) { return pos.X + pos.Y * Constants.MapSize.X; }
    public int Flatten(int x, int y) { return x + y * Constants.MapSize.X; }
    public int Flatten(Vector2 pos) { return Flatten((int)pos.X, (int)pos.Y); }
    public Tile? GetTile(Point coord)
    {
        if (coord.X < 0 || coord.X >= Constants.MapSize.X || coord.Y < 0 || coord.Y >= Constants.MapSize.Y)
            return null;
        return Level.Tiles[coord.X + coord.Y * Constants.MapSize.X];
    }
    public Tile? GetTile(int x, int y)
    {
        return GetTile(new Point(x, y));
    }
    public Rectangle TileTextureSource(Tile tile)
    {

        int mask = TileConnectionsMask(tile);

        int srcX = mask % Constants.TileMapDim.X * Constants.TilePixelSize.X;
        int srcY = mask / Constants.TileMapDim.X * Constants.TilePixelSize.Y;

        return new(srcX, srcY, Constants.TilePixelSize.X, Constants.TilePixelSize.Y);
    }
    public int TileConnectionsMask(Tile tile)
    {
        int mask = 0;
        int x = tile.Location.X;
        int y = tile.Location.Y;

        Tile? left = GetTile(x - 1, y);
        Tile? right = GetTile(x + 1, y);
        Tile? down = GetTile(x, y + 1);
        Tile? up = GetTile(x, y - 1);

        if (left?.Type == tile.Type) mask |= 1; // left
        if (down?.Type == tile.Type) mask |= 2; // down
        if (right?.Type == tile.Type) mask |= 4; // right
        if (up?.Type == tile.Type) mask |= 8; // up

        return mask;
    }
    #endregion
}
