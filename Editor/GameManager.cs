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
using Xna = Microsoft.Xna.Framework;

namespace Quest.Editor;

public class GameManager : IGameManager
{
    // Debug
    public Stopwatch Watch { get; private set; }
    public Dictionary<string, double> FrameTimes { get; private set; }
    // Properties
    public void AddLoot(Loot loot) => Loot.Add(loot);
    public bool IsKeyDown(Keys key) => Window.IsKeyDown(key);
    public bool IsKeyPressed(Keys key) => Window.IsKeyPressed(key);
    public bool Playing => true;
    public Point PlayerFoot => CameraDest.ToPoint() + Constants.MageDrawShift + new Point(0, Constants.MageHalfSize.Y);
    public Point Coord => Camera.ToPoint() / Constants.TileSize;
    public ContentManager Content => Window.Content;
    public GuiManager Gui { get; private set; } // GUI handler
    public EditorWindow Window { get; private set; }
    public Vector2 Camera { get; set; }
    public Vector2 CameraDest { get; set; }
    public float Delta { get; private set; }
    public SpriteBatch Batch { get; private set; }
    public Tile[] Tiles { get; set; }
    public List<Decal> Decals { get; set; }
    public List<Loot> Loot { get; set; }
    public List<NPC> NPCs { get; set; }
    public SpriteFont PixelOperator { get; private set; }
    public Inventory Inventory { get; set; }
    public float Time { get; private set; }
    // Thowaways for IGameManager interface
    public void Notification(string message, Color? color = null, float duration = 4) { }
    // Private
    private Point tileSize;
    public static readonly Point lootStackOffset = new(4, 4);
    public GameManager(EditorWindow window, SpriteBatch spriteBatch)
    {
        // Initialize the game
        Window = window;
        Batch = spriteBatch;
        tileSize = Constants.TileSize;
        Tiles = new Tile[Constants.MapSize.X * Constants.MapSize.Y];
        for (int y = 0; y < Constants.MapSize.Y; y++)
        {
            for (int x = 0; x < Constants.MapSize.X; x++)
            {
                Tiles[x + y * Constants.MapSize.X] = new Sky(new Xna.Point(x, y));
            }
        }
        Camera = new Vector2(128 * Constants.TileSize.X, 128 * Constants.TileSize.Y) - Constants.Middle.ToVector2();
        Gui = new();
        Watch = new();
        FrameTimes = [];
        NPCs = [];
        Decals = [];
        Loot = [];
        Inventory = new(this, 0, 0);

        // Load
        PixelOperator = window.Content.Load<SpriteFont>("Fonts/PixelOperator");
    }
    public void Update(float deltaTime, MouseState previousMouseState, MouseState mouseState)
    {
        // Update the game state
        Delta = deltaTime;
        Time += deltaTime;

        // NPCs
        UpdateNPCs();

        // Gui
        UpdateGui(deltaTime, previousMouseState, mouseState);
    }

    public void UpdateGui(float deltaTime, MouseState previousMouseState, MouseState mouseState)
    {
        // Gui
        Watch.Restart();
        Gui.Update(deltaTime);
        FrameTimes["GuiUpdate"] = Watch.Elapsed.TotalMilliseconds;
    }
    public void UpdateNPCs()
    {
        foreach (NPC npc in NPCs) npc.Update();
    }
    public void Draw()
    {
        // Tiles
        DrawTiles();

        // Decals
        DrawDecals();

        // Gui
        DrawGui();

        // Loot
        DrawLoot();

        // NPCs
        DrawNPCs();
    }
    public void DrawLoot()
    {
        Watch.Restart();
        // Draw each
        for (int l = 0; l < Loot.Count; l++)
        {
            Loot loot = Loot[l];
            Rectangle rect = new(loot.Location - Camera.ToPoint() + Constants.Middle, new(32, 32));
            rect.Y += (int)(Math.Sin((Time - loot.Birth) * 2 % (Math.PI * 2)) * 6); // Bob up and down
            DrawTexture(Batch, loot.Texture, rect, scale: new(2));
            // Draw stacks if multiple
            if (loot.Item.Amount > 1)
                DrawTexture(Batch, loot.Texture, new(rect.Location + lootStackOffset, rect.Size), scale: new(2));
            if (loot.Item.Amount > 2)
                DrawTexture(Batch, loot.Texture, new(rect.Location + lootStackOffset + lootStackOffset, rect.Size), scale: new(2));
            // Draw hitbox if enabled
            if (Constants.DRAW_HITBOXES)
                Batch.FillRectangle(rect, Constants.DebugPinkTint);
        }
        FrameTimes["DrawLoot"] = Watch.Elapsed.TotalMilliseconds;
    }

    public void DrawNPCs()
    {
        Watch.Restart();
        foreach (NPC npc in NPCs)
            npc.Draw();
        FrameTimes["NPCDraws"] = Watch.Elapsed.TotalMilliseconds;
    }
    public void DrawDecals()
    {
        Watch.Restart();
        // Draw each decal
        foreach (Decal decal in Decals)
            decal.Draw(this);
        FrameTimes["DecalDraws"] = Watch.Elapsed.TotalMilliseconds;
    }

    public void DrawGui()
    {
        // Widgets
        Watch.Restart();
        Gui.Draw(Batch);
        FrameTimes["GuiDraw"] = Watch.Elapsed.TotalMilliseconds;
    }
    public void DrawTiles()
    {
        // Tiles
        Watch.Restart();
        if (Tiles == null || Tiles.Length == 0) return;

        // Get bounds
        Point start = (Camera.ToPoint() - Constants.Middle) / Constants.TileSize;
        Point end = (Camera.ToPoint() + Constants.Middle) / Constants.TileSize;

        // Iterate
        for (int y = start.Y; y <= end.Y; y++)
        {
            for (int x = start.X; x <= end.X; x++)
            {
                Tile? tile = GetTile(x, y);
                if (tile == null) continue;
                tile.Draw(this);
            }
        }
        FrameTimes["TileDraws"] = Watch.Elapsed.TotalMilliseconds;
    }
    public void ReadLevel(string filename)
    {
        // Check exists
        if (!File.Exists($"Levels/{filename}.lvl"))
            throw new FileNotFoundException("Level file not found.", filename);

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
        List<NPC> npcBuffer = new();

        // Spawn
        Point spawn = new(reader.ReadByte(), reader.ReadByte());
        Camera = ((spawn - Constants.MiddleCoord) * tileSize).ToVector2();

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
            else // Regular tile
                tile = Quest.GameManager.TileFromId(type, new(i % Constants.MapSize.X, i / Constants.MapSize.X));
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
            int scale = reader.ReadByte();
            Color color = new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
            npcBuffer.Add(new NPC(this, TextureID.GrayMage, location, name, dialog, color, scale / 10f));
        }

        // Check null
        if (tilesBuffer == null)
            throw new ArgumentException("No tiles found in level file.");
        // Check size
        if (tilesBuffer.Length != Constants.MapSize.X * Constants.MapSize.Y)
            throw new ArgumentException($"Invalid level size - expected {Constants.MapSize.X}x{Constants.MapSize.X} tiles.");

        NPCs = npcBuffer;
        Tiles = [.. tilesBuffer];
    }
    public void DropLoot(Loot loot) { Loot.Add(loot); }
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
    // Utilities
    public int Flatten(Xna.Point pos) { return pos.X + pos.Y * Constants.MapSize.X; }
    public int Flatten(int x, int y) { return x + y * Constants.MapSize.X; }
    public int Flatten(Xna.Vector2 pos) { return Flatten((int)pos.X, (int)pos.Y); }
    public Tile? GetTile(Xna.Point coord)
    {
        if (coord.X < 0 || coord.X >= Constants.MapSize.X || coord.Y < 0 || coord.Y >= Constants.MapSize.Y)
            return null;
        return Tiles[coord.X + coord.Y * Constants.MapSize.X];
    }
    public Tile? GetTile(int x, int y)
    {
        return GetTile(new Point(x, y));
    }
    public Rectangle TileTextureSource(Tile tile)
    {

        int mask = TileConnectionsMask(tile);

        int srcX = (mask % Constants.TileMapDim.X) * Constants.TilePixelSize.X;
        int srcY = (mask / Constants.TileMapDim.X) * Constants.TilePixelSize.Y;

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
    // Throwaway
    public void LoadLevel(int levelIndex) { }
    public void LoadLevel(string levelName) { }
}
