using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Xna = Microsoft.Xna.Framework;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Quest.Gui;
using Quest.Tiles;
using System.Diagnostics;
using System.IO.Compression;
using MonoGame.Extended;
using Microsoft.Xna.Framework.Input;
using static Quest.TextureManager;

namespace Quest
{
    public class GameManager
    {
        public NPC[] NPCs { get; private set; }
        // Debug
        public Stopwatch Watch { get; private set; }
        public Tile? TileBelow { get; private set; }
        public Xna.Vector2 Coord { get; private set; }
        public Dictionary<string, double> FrameTimes { get; private set; }
        // Properties
        public GuiManager Gui { get; private set; } // GUI handler
        public Window Window { get; private set; }
        public Xna.Vector2 Camera { get; set; } // Current camera position w/ smooth movement
        public Xna.Vector2 CameraDest { get; set; } // Where the camera is going
        public float Delta { get; private set; }
        public SpriteBatch Batch { get; private set; }
        public List<Level> Levels { get; private set; }
        public Level? Level { get; private set; }
        public Dictionary<string, Texture2D> TileTextures { get; private set; }
        public Tile[] Tiles { get; private set; }
        // Inventory
        public Inventory Inventory { get; set; }
        // Mage sprites
        public Point MageSize { get; private set; }
        public Point MageHalfSize { get; private set; }
        // Private
        private Xna.Vector2 tileSize;
        public float Time { get; private set; }
        public GameManager(Window window, SpriteBatch spriteBatch)
        {
            // Initialize the game
            Window = window;
            Batch = spriteBatch;
            tileSize = Constants.TileSize;
            Levels = [];
            TileTextures = [];
            Tiles = [];
            Camera = new Vector2(128 * Constants.TileSize.X, 128 * Constants.TileSize.Y) - Constants.Middle;
            CameraDest = Camera;
            Gui = new();
            Gui.Widgets = [
                new StatusBar(new(10, Constants.Window.Y - 35), new(300, 25), Color.Green, Color.Red, 100, 100),
            ];
            Watch = new();
            FrameTimes = [];
            Inventory = new(this, 6, 4);
            Inventory.SetSlot(0, new Item("Sword", "A sharp, pointy sword", max:1));
            Inventory.SetSlot(1, new Item("Pickaxe", "Sturdy iron pickaxe for mining", max:1));

            // Characters textures
            MageSize = GetTexture(TextureID.BlueMage).Texture.Bounds.Size / new Point(4, 5);
            MageHalfSize = new(MageSize.X / 2, MageSize.Y / 2);

            // NPCS
            NPCs = [new(this, TextureID.WhiteMage, new(128, 131), "Saruman", " I have looked upon the Eye of Sauron. It is too strong for us. We cannot hope to match him. ... We must join with Sauron.", scale: 2)];
        }
        public void Update(float deltaTime, MouseState previousMouseState, MouseState mouseState)
        {
            // Update the game state
            Delta = deltaTime;
            Time += deltaTime;

            // Characters
            UpdateCharacters(deltaTime);

            // Camera
            UpdateCamera(deltaTime);

            // Gui
            UpdateGui(deltaTime, previousMouseState, mouseState);
        }
        public void UpdateCharacters(float deltaTime)
        {
            foreach (NPC npc in NPCs) npc.Update();
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
        public void Draw()
        {
            // Tiles
            DrawTiles();

            // Gui
            DrawGui();

            // Characters
            DrawCharacters();
        }
        public void DrawGui()
        {
            // Widgets
            Watch.Restart();
            Gui.Draw(Batch);
            FrameTimes["GuiDraw"] = Watch.Elapsed.TotalMilliseconds;
            // Inventory
            Watch.Restart();
            Inventory.Draw();
            FrameTimes["InventoryDraw"] = Watch.Elapsed.TotalMilliseconds;
        }
        public void DrawCharacters()
        {
            DrawPlayer();
            if (Constants.DRAW_HITBOXES)
                DrawPlayerHitbox();
            foreach (NPC npc in NPCs) npc.Draw();
        }
        public void DrawTiles()
        {
            // Tiles
            Watch.Restart();
            if (Tiles == null || Tiles.Length == 0) return;

            // Get bounds
            Point start = ((Camera - Constants.Middle) / Constants.TileSize).ToPoint();
            Point end = ((Camera + Constants.Middle) / Constants.TileSize).ToPoint();

            // Iterate
            for (int y = start.Y; y <= end.Y; y++) {
                for (int x = start.X; x <= end.X; x++) {
                    Tile? tile = GetTile(x, y);
                    if (tile == null) continue;
                    tile.Draw(this);
                }
            }
            FrameTimes["TileDraws"] = Watch.Elapsed.TotalMilliseconds;
        }
        // Movements
        public void Move(Xna.Vector2 move)
        {
            // Move
            if (move == Vector2.Zero) return;
            Xna.Vector2 finalMove = Vector2.Normalize(move) * Delta * Constants.PlayerSpeed;

            // Allow escaping
            if (!CollideCheck())
            {
                CameraDest += finalMove;
                TileBelow?.OnPlayerEnter(this);
                return;
            }

            // Check collision for x
            CameraDest += new Vector2(finalMove.X, 0);
            if (!CollideCheck()) CameraDest -= new Vector2(finalMove.X, 0);
            // Check collision for y
            CameraDest += new Vector2(0, finalMove.Y);
            if (!CollideCheck()) CameraDest -= new Vector2(0, finalMove.Y);

            // On tile enter
            Coord = Vector2.Floor((new Vector2(CameraDest.X, CameraDest.Y + MageHalfSize.Y) / Constants.TileSize));
            TileBelow = GetTile((int)Coord.X, (int)Coord.Y);
            if (TileBelow == null) return;
            TileBelow.OnPlayerEnter(this);
            // Debug
            if (Constants.COLLISION_DEBUG) TileBelow.Marked = true;
        }
        public void Move(float x, float y)
        {
            Move(new Xna.Vector2(x, y));
        }
        // Levels
        public void LoadLevel(int levelIndex)
        {
            // Load the level
            if (levelIndex < 0 || levelIndex >= Levels.Count)
                throw new ArgumentOutOfRangeException(nameof(levelIndex), "Invalid level index.");
            // Load the level data
            Tiles = Levels[levelIndex].Tiles;
            Level = Levels[levelIndex];
            // Spawn
            CameraDest = Level.Spawn.ToVector2() * tileSize;
            Camera = CameraDest;
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
            // If not found, throw an error
            throw new ArgumentException($"Level '{levelName}' not found in stored levels. Make sure the level file has been read before loading.");
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
            // Make buffer
            tilesBuffer = new Tile[Constants.MapSize.X * Constants.MapSize.Y];

            // Spawn
            Xna.Point spawn = new(reader.ReadByte(), reader.ReadByte());
            CameraDest = (spawn - Constants.MiddleCoord).ToVector2() * tileSize;
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
                else // Regular tile
                    tile = TileFromId(type, new(i % Constants.MapSize.X, i / Constants.MapSize.X));
                int idx = (int)(tile.Location.X + tile.Location.Y * Constants.MapSize.X);
                tilesBuffer[idx] = tile;
            }

            // Check null
            if (tilesBuffer == null)
                throw new ArgumentException("No tiles found in level file.");
            // Check size
            if (tilesBuffer.Length != Constants.MapSize.X * Constants.MapSize.Y)
                throw new ArgumentException($"Invalid level size - expected {Constants.MapSize.X}x{Constants.MapSize.X} tiles.");

            // Make and add the level
            Level created = new(filename, tilesBuffer, spawn);
            Levels.Add(created);
        }
        // Utilities
        public bool CollideCheck()
        {
            // Check if level loaded
            if (Level == null) return false;
            // Check 4 corners
            Coord = Vector2.Round(CameraDest / Constants.TileSize);
            for (int o = 0; o < Constants.PlayerCorners.Length; o++)
            {
                // Check if the player collides with a tile
                Vector2 coord = (CameraDest + Constants.PlayerCorners[o]) / Constants.TileSize;
                TileBelow = GetTile((int)Math.Floor(coord.X), (int)Math.Floor(coord.Y));
                if (TileBelow == null || !TileBelow.IsWalkable) return false;
            }
            return true;
        }
        public static Tile TileFromId(int id, Xna.Point location)
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
                _ => new Tile(location), // Default tile
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

            int srcX = (mask % Constants.TileMapDim.X) * (int)Constants.TilePixelSize.X;
            int srcY = (mask / Constants.TileMapDim.X) * (int)Constants.TilePixelSize.Y;

            return new(srcX, srcY, (int)Constants.TilePixelSize.X, (int)Constants.TilePixelSize.Y);
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
        public void DrawPlayerHitbox()
        {
            Vector2[] points = new Vector2[4];
            for (int c = 0; c < Constants.PlayerCorners.Length; c++)
                points[c] = Constants.Middle + Constants.PlayerCorners[c];
            Batch.FillRectangle(new Rectangle((int)points[0].X, (int)points[0].Y, (int)(points[2].X - points[0].X), (int)(points[2].Y - points[0].Y)), Constants.DebugPinkTint);
        }
        public void DrawPlayer()
        {
            int sourceRow = 0;
            if (Window.moveX == 0 && Window.moveY == 0) sourceRow = 0;
            else if (Window.moveX < 0) sourceRow = 1;
            else if (Window.moveX > 0) sourceRow = 3;
            else if (Window.moveY > 0) sourceRow = 2;
            else if (Window.moveY < 0) sourceRow = 4;
            Rectangle source = new((int)(Time * (sourceRow == 0 ? 1.5f : 6)) % 4 * MageSize.X, sourceRow * MageSize.Y, MageSize.X, MageSize.Y);
            Rectangle rect = new(Constants.Middle.ToPoint() - MageHalfSize, MageSize);
            DrawTexture(Batch, TextureID.BlueMage, rect, source: source);
        }
    }
}
