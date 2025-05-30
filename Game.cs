using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Xna = Microsoft.Xna.Framework;
using System.IO;
using System.Reflection.Metadata;
using Microsoft.Xna.Framework.Content;
using Quest.Gui;
using Quest.Tiles;
using System.Diagnostics;
using System.IO.Compression;

namespace Quest
{
    public class GameHandler
    {
        // TODO REMOVE THIS AND REPLACE WITH NPCS
        public Widget[] Widgets { get; private set; }
        // Debug
        public Stopwatch Watch { get; private set; }
        public Tile TileBelow { get; private set; }
        public Xna.Vector2 Coord { get; private set; }
        public Dictionary<string, double> FrameTimes { get; private set; }
        // Properties
        public GuiHandler Gui { get; private set; } // GUI handler
        public Window Window { get; private set; }
        public int TilesDrawn { get; private set; } // For debugging
        public Xna.Vector2 Camera { get; set; }
        public float Delta { get; private set; }
        public SpriteBatch Batch { get; private set; }
        public List<Level> Levels { get; private set; }
        public Level Level { get; private set; }
        public Dictionary<string, Texture2D> TileTextures { get; private set; }
        public Tile[] Tiles { get; private set; }
        private Xna.Vector2 tileSize;
        public GameHandler(Window window, SpriteBatch spriteBatch)
        {
            // Initialize the game
            Window = window;
            Batch = spriteBatch;
            tileSize = Constants.TileSize;
            Levels = [];
            TileTextures = new();
            Tiles = [];
            Camera = new Xna.Vector2(128 * Constants.TileSize.X, 128 * Constants.TileSize.Y) - Constants.Middle;
            string dialog = "This is some example dialog! The NPC can talk to the player through this box which appears when near. The quick brown fox jumped over the lazy dog.";
            Gui = new();
            Gui.Widgets = [new Dialog(Gui, new(Constants.Middle.X - 600, 800), new(1200, 100), new(194, 125, 64), Color.Black, dialog, Window.PixelOperator, borderColor: new(36, 19, 4))];
            Watch = new();
            FrameTimes = new()
            {
                { "GuiUpdate", 0 },
                { "TileDraws", 0 },
                { "GuiDraw", 0 },
            };
        }
        public void Update(float deltaTime)
        {
            // Update the game state
            Delta = deltaTime;
            Watch.Restart();
            Gui.Update(deltaTime);
            FrameTimes["GuiUpdate"] = Watch.Elapsed.TotalMilliseconds;
        }
        public void Draw()
        {
            // Tiles
            Watch.Restart();
            if (Tiles == null || Tiles.Length == 0) return;

            TilesDrawn = 0;
            for (int t = 0; t < Tiles.Length; t++)
            {
                // Check null
                if (Tiles[t] == null) continue;
                Tile tile = Tiles[t];
                // Draw each tile using the sprite batch
                Xna.Vector2 dest = tile.Location.ToVector2() * tileSize - Camera;
                dest += Constants.Middle;
                // Check x in bounds
                if (dest.X + Constants.TileSize.X < 0 || dest.X > Constants.Window.X) continue;
                if (dest.Y + Constants.TileSize.Y * 2 < 0 || dest.Y > Constants.Window.Y) continue;
                // Draw
                dest.Round();
                Texture2D texture = TileTextures[tile.Type.ToString()];
                Batch.Draw(texture, dest, tile.Location.ToVector2() == Coord ? Color.White : Color.White);
                TilesDrawn++;
            }
            FrameTimes["TileDraws"] = Watch.Elapsed.TotalMilliseconds;

            // Widgets
            Watch.Restart();
            Gui.Draw(Batch);
            FrameTimes["GuiDraw"] = Watch.Elapsed.TotalMilliseconds;
        }
        // Movements
        public void Move(Xna.Vector2 move)
        {
            // Move
            if (move == Xna.Vector2.Zero) return;
            Xna.Vector2 finalMove = Xna.Vector2.Normalize(move) * Delta * Constants.PlayerSpeed;

            // Allow escaping
            if (!IsTileWalkable())
            {
                Camera += finalMove;
                TileBelow?.OnPlayerEnter(this);
                return;
            }

            // Check collision for x
            Camera += new Vector2(finalMove.X, 0);
            if (!IsTileWalkable()) Camera -= new Vector2(finalMove.X, 0);
            // Check collision for y
            Camera += new Vector2(0, finalMove.Y);
            if (!IsTileWalkable()) Camera -= new Vector2(0, finalMove.Y);

            // On tile enter
            Coord = Vector2.Round(Camera / Constants.TileSize);
            TileBelow = Tiles[(int)(Coord.X + Coord.Y * Level.Dimensions.X)];
            TileBelow.OnPlayerEnter(this);
        }
        public void Move(float x, float y)
        {
            Move(new Xna.Vector2(x, y));
        }
        // Loading and reading
        public void LoadContent(ContentManager content)
        {
            // Dynamically load images
            foreach (string filename in Constants.TileNames)
            {
                if (!string.IsNullOrEmpty(filename))
                {
                    Texture2D texture = content.Load<Texture2D>($"Images/Tiles/{filename}");
                    TileTextures[filename] = texture;
                }
            }

            // Load gui
            Gui.LoadContent(content);
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
            Level created = new(filename, tilesBuffer, new(Constants.MapSize.X, Constants.MapSize.Y));
            Levels.Add(created);
        }
        // Utilities
        public bool IsTileWalkable()
        {
            // Check if level loaded
            if (Level == null) return false;
            // Out of bounds
            Coord = Vector2.Round(Camera / Constants.TileSize);
            if (Coord.X < 0 || Coord.X > Level.Dimensions.X || Coord.Y < 0 || Coord.Y > Level.Dimensions.X) return false;
            // Check if the tile is walkable
            TileBelow = Tiles[(int)(Coord.X + Coord.Y * Level.Dimensions.X)];
            return TileBelow != null && TileBelow.IsWalkable;
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
                TileType.Wall => new Wall(location),
                TileType.Stairs => new Stairs(location, "_null", Constants.MiddleCoord),
                _ => new Tile(location), // Default tile
            };
        }
    }
}
