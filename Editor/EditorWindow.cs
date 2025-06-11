using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Quest.Tiles;
using Xna = Microsoft.Xna.Framework;

namespace Quest.Editor;

public class EditorWindow : Game
{
    // Inputs
    private KeyboardState keyState;
    private KeyboardState previousKeyState;
    private MouseState mouseState;
    private MouseState previousMouseState;
    public bool LMouseClick => mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;
    public bool LMouseDown => mouseState.LeftButton == ButtonState.Pressed;
    public bool LMouseRelease => mouseState.LeftButton == ButtonState.Released && previousMouseState.LeftButton == ButtonState.Pressed;
    public bool RMouseClick => mouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released;
    public bool RMouseDown => mouseState.RightButton == ButtonState.Pressed;
    public bool RMouseRelease => mouseState.RightButton == ButtonState.Released && previousMouseState.RightButton == ButtonState.Pressed;
    // Editing
    private TileType Material;
    private int Selection;
    private Point mouseCoord;
    private Point Spawn;
    private readonly Color highlightColor = new(1, 1, 1, .8f);
    // Debug
    private float delta;
    private Dictionary<string, double> frameTimes;
    // Devices
    private GraphicsDeviceManager graphics;
    protected SpriteBatch spriteBatch;
    public GameManager GameManager;

    // Fonts
    public SpriteFont Arial { get; private set; }
    public SpriteFont ArialSmall { get; private set; }
    public SpriteFont ArialLarge { get; private set; }
    public SpriteFont PixelOperator { get; private set; }
    // Movements
    public int moveX;
    public int moveY;

    // Debug
    private static readonly Color[] colors = {
        Color.Purple, new(255, 128, 128), new(128, 255, 128), new(255, 255, 180), new(128, 255, 255),
        Color.Brown, Color.Gray, new(192, 128, 64), new(64, 128, 192), new(192, 192, 64),
        new(64, 192, 128), new(192, 64, 128), new(160, 80, 0), new(80, 160, 0), new(0, 160, 80),
        new(160, 0, 80), new(96, 96, 192), new(192, 96, 96), new(96, 192, 96), new(192, 192, 96)
    };
    private float debugUpdateTime;
    private float cacheDelta;
    public EditorWindow()
    {
        graphics = new GraphicsDeviceManager(this)
        {
            //PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
            //PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height,
            PreferredBackBufferWidth = Constants.Window.X,
            PreferredBackBufferHeight = Constants.Window.Y,
            IsFullScreen = false,
            SynchronizeWithVerticalRetrace = Constants.VSYNC,
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = false;
        IsFixedTimeStep = false;
    }

    protected override void Initialize()
    {
        // Defaults
        keyState = Keyboard.GetState();
        previousKeyState = keyState;
        mouseState = Mouse.GetState();
        frameTimes = [];
        debugUpdateTime = 0;
        cacheDelta = 0f;

        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        // Fonts
        Arial = Content.Load<SpriteFont>("Fonts/Arial");
        ArialSmall = Content.Load<SpriteFont>("Fonts/ArialSmall");
        ArialLarge = Content.Load<SpriteFont>("Fonts/ArialLarge");
        PixelOperator = Content.Load<SpriteFont>("Fonts/PixelOperator");

        // Textures
        LoadTextures(Content);

        // Managers
        GameManager = new(this, spriteBatch);
    }

    protected override void Update(GameTime gameTime)
    {
        GameManager.Watch.Restart();

        // Inputs
        keyState = Keyboard.GetState();
        mouseState = Mouse.GetState();
        mouseCoord = (mouseState.Position + GameManager.Camera.ToPoint() - Constants.Middle) / Constants.TileSize;

        // Exit
        if (IsKeyDown(Keys.Escape)) Exit();

        // Delta debugUpdateTime
        delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Movement
        int speedup = IsKeyDown(Keys.LeftAlt) ? 5 : 2;
        moveX = 0; moveY = 0;
        moveX += IsAnyKeyDown(Keys.A, Keys.Left) ? -Constants.PlayerSpeed : 0;
        moveX += IsAnyKeyDown(Keys.D, Keys.Right) ? Constants.PlayerSpeed : 0;
        moveY += IsAnyKeyDown(Keys.W, Keys.Up) ? -Constants.PlayerSpeed : 0;
        moveY += IsAnyKeyDown(Keys.S, Keys.Down) ? Constants.PlayerSpeed : 0;
        GameManager.Camera += new Vector2(moveX, moveY) * delta * speedup;

        // Time
        GameManager.FrameTimes["InputUpdate"] = GameManager.Watch.Elapsed.TotalMilliseconds;
        GameManager.Update(delta, previousMouseState, mouseState);

        // Open file
        if (IsAllKeysDown(Keys.O, Keys.LeftControl))
        {
            // Input file name
            string filename = Logger.Input("Open level file: ");
            if (!string.IsNullOrEmpty(filename))
            {
                //try
                //{
                    ReadLevel(filename);
                    Logger.Log($"Opened level '{filename}'.");
                //}
                //catch (Exception ex)
                //{
                //    Logger.Error($"Failed to open level '{filename}': {ex.Message}");
                //    throw ex;
                //}
            }
        }

        // Change material
        if (mouseState.ScrollWheelValue > previousMouseState.ScrollWheelValue || IsKeyPressed(Keys.OemCloseBrackets))
        {
            Selection = (Selection + 1) % Constants.TileNames.Length;
            Material = (TileType)Enum.Parse(typeof(TileType), Constants.TileNames[Selection]);
            Logger.Log($"Material set to '{Material}'.");
        }
        if (mouseState.ScrollWheelValue < previousMouseState.ScrollWheelValue || IsKeyPressed(Keys.OemCloseBrackets))
        {
            Selection = (Selection - 1) % Constants.TileNames.Length;
            if (Selection < 0) Selection += Constants.TileNames.Length;
            Material = (TileType)Enum.Parse(typeof(TileType), Constants.TileNames[Selection]);
            Logger.Log($"Material set to '{Material}'.");
        }

        // Draw
        if (LMouseDown)
        {
            if (GetTile(mouseCoord).Type != Material)
            {
                // Add tile
                Tile tile;
                if (Selection == (int)TileType.Stairs)
                    tile = new Stairs(mouseCoord, "_null", Constants.MiddleCoord);
                else if (Selection == (int)TileType.Door)
                    tile = new Door(mouseCoord, Constants.Key);
                else
                    tile = Quest.GameManager.TileFromId(Selection, mouseCoord);

                SetTile(tile);
                Logger.Log($"Set tile to '{Material}' @ {mouseCoord.X}, {mouseCoord.Y}.");
            }
        }
        // Edit options
        if (RMouseDown)
        {
            Tile tileBelow = GameManager.Tiles[mouseCoord.X + mouseCoord.Y * Constants.MapSize.X];
            EditTile(tileBelow);
        }
        // Erase (set to sky)
        if (mouseState.MiddleButton == ButtonState.Pressed)
        {
            if (GetTile(mouseCoord).Type != TileType.Sky)
            {
                // Add tile
                Tile tile = new Sky(mouseCoord);
                SetTile(tile);
                Logger.Log($"Set tile to '{Material}' @ {mouseCoord.X}, {mouseCoord.Y}.");
            }
        }

        // Fill
        if (IsKeyPressed(Keys.F))
            FloodFill();

        // NPCs
        if (HotKeyPressed(Keys.LeftControl, Keys.N))
            EditNPCs();

        // Loot
        if (HotKeyPressed(Keys.LeftControl, Keys.L))
            EditLoot();

        // Decals
        if (HotKeyPressed(Keys.LeftControl, Keys.D))
            EditDecals();

        // Save
        if (HotKeyPressed(Keys.LeftControl, Keys.E)) SaveLevel(this);

        // Level info
        if (HotKeyPressed(Keys.LeftControl, Keys.I)) SetSpawn();

        // Debug
        if (IsKeyPressed(Keys.F1))
        {
            Constants.COLLISION_DEBUG = !Constants.COLLISION_DEBUG;
            Logger.System($"COLLISION_DEBUG set to: {Constants.COLLISION_DEBUG}");
        }
        if (IsKeyPressed(Keys.F2))
        {
            Constants.TEXT_INFO = !Constants.TEXT_INFO;
            Logger.System($"TEXT_INFO set to: {Constants.TEXT_INFO}");
        }
        if (IsKeyPressed(Keys.F3))
        {
            Constants.FRAME_INFO = !Constants.FRAME_INFO;
            Logger.System($"FRAME_INFO set to: {Constants.FRAME_INFO}");
        }
        if (IsKeyPressed(Keys.F4))
        {
            Constants.LOG_INFO = !Constants.LOG_INFO;
            Logger.System($"LOG_INFO set to: {Constants.LOG_INFO}");
        }
        if (IsKeyPressed(Keys.F5))
        {
            Constants.FRAME_BAR = !Constants.FRAME_BAR;
            Logger.System($"FRAME_BAR set to: {Constants.FRAME_BAR}");
        }
        if (IsKeyPressed(Keys.F6))
        {
            Constants.DRAW_HITBOXES = !Constants.DRAW_HITBOXES;
            Logger.System($"DRAW_HITBOXES set to: {Constants.DRAW_HITBOXES}");
        }

        // Set previous key state
        previousKeyState = keyState;
        previousMouseState = mouseState;

        // Set previous key state
        GameManager.Watch.Restart();
        previousKeyState = keyState;
        previousMouseState = mouseState;

        // Final
        debugUpdateTime += delta;
        GameManager.FrameTimes["OtherUpdates"] = GameManager.Watch.Elapsed.TotalMilliseconds;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        // Clear and start shader gui
        GraphicsDevice.Clear(Color.Magenta);
        spriteBatch.Begin(blendState: BlendState.NonPremultiplied, samplerState: SamplerState.PointClamp);

        // Draw game
        GameManager.Draw();

        // Cursor
        Point cursorPos = mouseCoord * Constants.TileSize - GameManager.Camera.ToPoint() + Constants.Middle;
        spriteBatch.FillRectangle(new(cursorPos.ToVector2(), Constants.TileSize), Color.White);
        TextureID ghostTile = (TextureID)(Enum.TryParse(typeof(TextureID), Material.ToString(), out var tex) ? tex : TextureID.Null);
        DrawTexture(spriteBatch, ghostTile, cursorPos, source: Constants.ZeroSource, color: highlightColor, scale: new Vector2(4));

        // Text info
        GameManager.Watch.Restart();
        if (Constants.TEXT_INFO)
        {
            // Background
            spriteBatch.FillRectangle(new(0, 0, 200, 140), Color.Black * .8f);
            spriteBatch.DrawString(Arial, $"FPS: {(cacheDelta != 0 ? 1f / cacheDelta : 0):0.0}\nTime: {GameManager.Time:0.00}\nCamera: {GameManager.Camera.X:0.0},{GameManager.Camera.Y:0.0}\nMouseCoord: {mouseCoord.X},{mouseCoord.Y}", new Vector2(10, 10), Color.White);
        }

        // Frame info
        if (Constants.FRAME_INFO)
        {
            spriteBatch.FillRectangle(new(Constants.Window.X - 190, 0, 190, GameManager.FrameTimes.Count * 20), Color.Black * .8f);
            string frameString = string.Join("\n", frameTimes.Select(kv => $"{kv.Key}: {kv.Value:0.0}ms"));
            spriteBatch.DrawString(Arial, frameString, new Vector2(Constants.Window.X - 180, 10), Color.White);
        }
        GameManager.FrameTimes["DebugTextDraw"] = GameManager.Watch.Elapsed.TotalMilliseconds;

        // Frame bar
        GameManager.Watch.Restart();
        if (Constants.FRAME_BAR)
            DrawFrameBar();
        GameManager.FrameTimes["FrameBarDraw"] = GameManager.Watch.Elapsed.TotalMilliseconds;

        // Minimap
        DrawMiniMap();

        // Cursor
        DrawTexture(spriteBatch, TextureID.CursorArrow, mouseState.Position);

        // Final
        spriteBatch.End();
        base.Draw(gameTime);
    }
    // Key presses
    public bool HotKeyPressed(Keys modifier, Keys key) { return IsKeyDown(modifier) && IsKeyPressed(key); }
    public bool IsKeyDown(Keys key) => keyState.IsKeyDown(key);
    public bool IsAnyKeyDown(params Keys[] keys)
    {
        foreach (Keys key in keys) { if (keyState.IsKeyDown(key)) return true; }
        return false;
    }
    public bool IsAllKeysDown(params Keys[] keys)
    {
        foreach (Keys key in keys) { if (!keyState.IsKeyDown(key)) return false; }
        return true;
    }
    public bool IsKeyPressed(Keys key) => keyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key);
    public bool IsAnyKeyPressed(params Keys[] keys)
    {
        foreach (Keys key in keys) { if (keyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key)) return true; }
        return false;
    }
    public bool IsAllKeysPressed(params Keys[] keys)
    {
        foreach (Keys key in keys) { if (!(keyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key))) return false; }
        return true;
    }
    // For cleaner code
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
                SetTile(Quest.GameManager.TileFromId(Selection, current.Location));
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
                Tile tile = GetTile(new Point(x, y));
                spriteBatch.DrawPoint(new(10 + x, Constants.Window.Y - Constants.MapSize.Y - 10 + y), Constants.MiniMapColors[(int)tile.Type]);
            }
        }
        // Player
        Point dest = GameManager.Coord + new Point(10, Constants.Window.Y - Constants.MapSize.Y - 10);
        spriteBatch.DrawPoint(dest.ToVector2(), Color.Red, size: 2);
    }
    public void DrawFrameBar()
    {
        // Update info twice a second
        if (debugUpdateTime >= .5)
        {
            cacheDelta = delta;
            frameTimes = new Dictionary<string, double>(GameManager.FrameTimes);
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

        // Spawn
        Spawn = new(reader.ReadByte(), reader.ReadByte());
        GameManager.Camera = (Spawn * Constants.TileSize).ToVector2();

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
                tile = Quest.GameManager.TileFromId(type, new(i % Constants.MapSize.X, i / Constants.MapSize.X));
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
            npcBuffer.Add(new NPC(GameManager, texture, location, name, dialog, Color.White, scale / 10f));
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
            lootBuffer.Add(new Loot(new Item(name, description, amount, max), location, GameManager.Time));
        }

        // Decals
        byte decalCount = reader.ReadByte();
        for (int n = 0; n < decalCount; n++)
        {
            DecalType type = (DecalType)reader.ReadByte();
            Point location = new(reader.ReadUInt16(), reader.ReadUInt16());
            decalBuffer.Add(GameManager.DecalFromId(reader.ReadByte(), location));
        }

        // Check null
        if (tilesBuffer == null)
            throw new ArgumentException("No tiles found in level file.");
        // Check size
        if (tilesBuffer.Length != Constants.MapSize.X * Constants.MapSize.Y)
            throw new ArgumentException($"Invalid level size - expected {Constants.MapSize.X}x{Constants.MapSize.X} tiles.");

        // Make and add the level
        GameManager.NPCs = npcBuffer;
        GameManager.Tiles = tilesBuffer;
    }
    public static void SaveLevel(EditorWindow editor)
    {
        // Input
        string name = Logger.Input("Export file name: ");

        // Parse
        Directory.CreateDirectory("..\\..\\..\\Levels");
        using FileStream fileStream = File.Create($"..\\..\\..\\Levels/{name}.lvl");
        using GZipStream gzipStream = new(fileStream, CompressionLevel.Optimal);
        using BinaryWriter writer = new(gzipStream);

        // Write spawn
        writer.Write(IntToByte(editor.Spawn.X));
        writer.Write(IntToByte(editor.Spawn.Y));

        // Tiles
        for (int i = 0; i < Constants.MapSize.X * Constants.MapSize.Y; i++)
        {
            Tile tile = editor.GameManager.Tiles[i];
            // Write tile data
            writer.Write(IntToByte((int)tile.Type));
            // Extra properties
            if (tile is Stairs stairs)
            {
                // Write destination
                writer.Write(stairs.DestLevel);
                writer.Write(IntToByte(stairs.DestPosition.X));
                writer.Write(IntToByte(stairs.DestPosition.Y));
            } else if (tile is Door door)
            {
                // Write door key
                writer.Write(door.Key.Name);
                writer.Write(door.Key.Description);
            }
        }

        // NPCs
        writer.Write((byte)Math.Min(editor.GameManager.NPCs.Count, 255));
        for (int n = 0; n < Math.Min(editor.GameManager.NPCs.Count, 255); n++)
        {
            NPC npc = editor.GameManager.NPCs[n];
            // Write NPC data
            writer.Write(npc.Name);
            writer.Write(npc.Dialog);
            writer.Write(IntToByte(npc.Location.X));
            writer.Write(IntToByte(npc.Location.Y));
            writer.Write(IntToByte((int)(npc.Scale * 10)));
            writer.Write(IntToByte((int)npc.Texture));
        }

        // Floor loot
        writer.Write((byte)Math.Min(editor.GameManager.Loot.Count, 255));
        for (int n = 0; n < Math.Min(editor.GameManager.Loot.Count, 255); n++)
        {
            Loot loot = editor.GameManager.Loot[n];
            // Write NPC data
            writer.Write(loot.Item.Name);
            writer.Write(loot.Item.Description);
            writer.Write(loot.Item.Amount);
            writer.Write(loot.Item.Max);
            writer.Write((UInt16)loot.Location.X);
            writer.Write((UInt16)loot.Location.Y);
        }

        // Decals
        writer.Write((byte)Math.Min(editor.GameManager.Decals.Count, 255));
        for (int n = 0; n < Math.Min(editor.GameManager.Decals.Count, 255); n++)
        {
            Decal decal = editor.GameManager.Decals[n];
            // Write decal data
            writer.Write((byte)decal.Type);
            writer.Write(IntToByte(decal.Location.X));
            writer.Write(IntToByte(decal.Location.Y));
        }
    }
    public void SetTile(Tile tile)
    {
        GameManager.Tiles[tile.Location.X + tile.Location.Y * Constants.MapSize.X] = tile;
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
    public void SetSpawn()
    {
        // Destination position
        string resp = Logger.Input($"Spawn position [{Spawn.X}, {Spawn.Y}]: ");
        if (resp != "")
        {
            string[] parts = resp.Split(',');
            if (parts.Length == 2 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
                if (x >= 0 && x < Constants.MapSize.X && y >= 0 && y < Constants.MapSize.Y)
                    Spawn = new(x, y);
                else
                    Logger.Error($"Position out of bounds - must be within the map size {Constants.MapSize.X}x{Constants.MapSize.Y}.");
            else
                Logger.Error("Invalid position format - use 'x,y'.");
        }
    }
    public void EditNPCs()
    {
        if (IsKeyDown(Keys.LeftShift)) // Delete
        {
            foreach (NPC npc in GameManager.NPCs)
            {
                if (npc.Location == mouseCoord)
                {
                    GameManager.NPCs.Remove(npc);
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
            GameManager.NPCs.Add(new NPC(GameManager, texture, mouseCoord, name, dialog, Color.White, scale));
        }
    }
    public void EditDecals()
    {
        if (IsKeyDown(Keys.LeftShift)) // Delete
        {
            foreach (Decal decal in GameManager.Decals)
            {
                if (decal.Location == mouseCoord)
                {
                    GameManager.Decals.Remove(decal);
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
            GameManager.Decals.Add(GameManager.DecalFromId(decal, mouseCoord));
        }
    }
    public void EditLoot()
    {
        if (IsKeyDown(Keys.LeftShift)) // Delete
        {
            foreach (Loot loot in GameManager.Loot)
            {
                if (loot.Location == mouseCoord)
                {
                    GameManager.Loot.Remove(loot);
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
            GameManager.Loot.Add(new Loot(new Item(name, description, amount, max), mouseState.Position + GameManager.Camera.ToPoint() - Constants.Middle, GameManager.Time));
        }
    }
    public Tile GetTile(Xna.Point coord)
    {
        if (coord.X < 0 || coord.X >= Constants.MapSize.X || coord.Y < 0 || coord.Y >= Constants.MapSize.Y)
            throw new ArgumentOutOfRangeException(nameof(coord), "Coordinates are out of bounds of the level.");
        return GameManager.Tiles[coord.X + coord.Y * Constants.MapSize.X];
    }
    public Tile GetTile(int x, int y)
    {
        return GetTile(new Point(x, y));
    }
    // Key presses
    public static byte IntToByte(int value)
    {
        if (value < 0 || value > 255)
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be between 0 and 255.");
        return (byte)value;
    }
}
