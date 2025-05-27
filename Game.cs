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
        private Xna.Vector3 tileSize;
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
                Xna.Vector2 dest = new(tile.Location.X * tileSize.X - Camera.X, (tile.Location.Y * tileSize.Y) - (tile.Location.Z * tileSize.Z) - Camera.Y);
                dest += Constants.Middle;
                // Check x in bounds
                if (dest.X + Constants.TileSize.X < 0 || dest.X > Constants.Window.X) continue;
                if (dest.Y + Constants.TileSize.Y*2 < 0 || dest.Y > Constants.Window.Y) continue;
                // Draw
                dest.Round();
                Texture2D texture = TileTextures[tile.Type.ToString()];
                Batch.Draw(texture, dest, tile.Location == new Xna.Vector3(Coord, tile.Location.Z) ? Color.Red : Color.White);
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
            Camera += finalMove;
            // Check collision and undo
            if (!IsTileWalkable()) Camera -= finalMove;
        }
        public void Move(float x, float y)
        {
            Move(new Xna.Vector2(x, y));
        }
        // Loading and reading
        public void LoadContent(ContentManager content)
        {
            string[] tileNames = ["Water", "Grass", "Wall", "Door"];

            // Dynamically load images
            foreach (string filename in tileNames)
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
            List<Tile> tilesBuffer = new();

            // Parse
            int l = 0;
            int s = 0;
            for (l = 0; l < lines.Length; l++)
            {
                string line = lines[l];
                string[] squares = line.Split(' ');
                for (s = 0; s < squares.Length; s++)
                {
                    // Parse parts
                    string[] parts = squares[s].Split('.');
                    int type = int.Parse(parts[0]);
                    // Empty tile
                    if (type == -1) continue;
                    // Normal tile
                    int z = int.Parse(parts[1]);
                    Xna.Vector3 position = new(s, l, z);
                    tilesBuffer.Add(new Tile(position, (Tile.TileType)type));
                }
            }

            // If successful add level
            if (tilesBuffer.Count == 0)
                throw new ArgumentException("No tiles found in level file.");
            Level created = new Level(filename, tilesBuffer.ToArray(), new Xna.Vector2(s, l));
            Levels.Add(created);
        }
        // Utilities
        public bool IsTileWalkable()
        {
            // Check if level loaded
            if (Level == null) return false;
            // Out of bounds
            Coord = Vector2.Round(Camera / Constants.TileSize2D) + new Xna.Vector2(0, 1);
            if (Coord.X < 0 || Coord.X > Level.Dimensions.X || Coord.Y < 0 || Coord.Y > Level.Dimensions.X) return false;
            // Check if the tile is walkable
            TileBelow = Tiles.LastOrDefault(t => t.Projection == Coord);
            if (TileBelow.Projection != new Xna.Vector2(TileBelow.Location.X, TileBelow.Location.Y))
            {
                Console.WriteLine(TileBelow.Projection);
                Console.WriteLine(TileBelow.Location);
            }
            return TileBelow.IsWalkable;
        }
    }
}
