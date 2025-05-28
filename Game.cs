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

namespace Quest
{
    public class GameHandler
    {
        // TODO REMOVE THIS AND REPLACE WITH NPCS
        public Widget[] Widgets { get; private set; }
        // Debug
        public Tile TileBelow { get; private set; }
        public Xna.Vector2 Coord { get; private set; }
        // Properties
        public GuiHandler Gui { get; private set; } // GUI handler
        public Window Window { get; private set; }
        public int TilesDrawn { get; private set; } // For debugging
        public Xna.Vector2 Camera { get; private set; }
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
            Camera = Xna.Vector2.Zero;
            string dialog = "This is some example dialog! The NPC can talk to the player through this box which appears when near. The quick brown fox jumped over the lazy dog.";
            Gui = new();
            Gui.Widgets = [new Dialog(Gui, new(Constants.Middle.X - 600, 800), new(1200, 100), new(194, 125, 64), Color.Black, dialog, Window.PixelOperator, borderColor:new(36, 19, 4))];
        }
        public void Update(float deltaTime)
        {
            // Update the game state
            Delta = deltaTime;
            Gui.Update(deltaTime);
        }
        public void Draw()
        {
            // Tiles
            if (Tiles == null || Tiles.Length == 0) return;

            TilesDrawn = 0;
            foreach (Tile tile in Tiles)
            {
                // Draw each tile using the sprite batch
                Xna.Vector2 dest = tile.Location * tileSize - Camera;
                dest += Constants.Middle;
                // Check x in bounds
                if (dest.X + Constants.TileSize.X < 0 || dest.X > Constants.Window.X) continue;
                if (dest.Y + Constants.TileSize.Y*2 < 0 || dest.Y > Constants.Window.Y) continue;
                // Draw
                dest.Round();
                Texture2D texture = TileTextures[tile.Type.ToString()];
                Batch.Draw(texture, dest, tile.Location == Coord ? Color.Red : Color.White);
                TilesDrawn++;
            }

            // Widgets
            Gui.Draw(Batch);
        }
        // Movements
        public void Move(Xna.Vector2 move)
        {
            // Move
            if (move == Xna.Vector2.Zero) return;
            Xna.Vector2 finalMove = Xna.Vector2.Normalize(move) * Delta * Constants.PlayerSpeed;

            // Check collision for x
            Camera += new Vector2(finalMove.X, 0);
            if (!IsTileWalkable()) Camera -= new Vector2(finalMove.X, 0);
            // Check collision for y
            Camera += new Vector2(0, finalMove.Y);
            if (!IsTileWalkable()) Camera -= new Vector2(0, finalMove.Y);

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
            int l = 0;
            foreach (Level level in Levels)
            {
                if (level.Name == levelName) { LoadLevel(l); }
                l++;
            }
        }
        public void ReadLevel(string filename)
        {
            // Check exists
            if (!File.Exists($"Levels/{filename}.lvl"))
                throw new FileNotFoundException("Level file not found.", filename);

            // Get data
            string data = File.ReadAllText($"Levels/{filename}.lvl");
            string[] lines = data.Split('\n');

            // Make buffer
            List<Tile> tilesBuffer = [];

            // Parse
            uint width; uint height; uint count;
            using (BinaryReader reader = new(File.Open($"Levels/{filename}.lvl", FileMode.Open)))
            {
                count = reader.ReadUInt16();
                width = reader.ReadByte();
                height = reader.ReadByte();
                for (int i = 0; i < count; i++)
                {
                    // Read tile data
                    int x = reader.ReadByte();
                    int y = reader.ReadByte();
                    int type = reader.ReadByte();
                    // Check if valid tile type
                    if (type < 0 || type >= Enum.GetValues(typeof(Tile.TileType)).Length)
                        throw new ArgumentException($"Invalid tile type {type} in level file.");
                    // Create tile and add to buffer
                    Tile tile = new Tile(new Xna.Vector2(x, y), (Tile.TileType)type);
                    tilesBuffer.Add(tile);
                }
            }

            // If successful add level
            if (tilesBuffer.Count == 0)
                throw new ArgumentException("No tiles found in level file.");
            Level created = new Level(filename, tilesBuffer.ToArray(), new Xna.Vector2(width, height));
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
            return TileBelow.IsWalkable;
        }
    }
}
