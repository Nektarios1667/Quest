using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Xna = Microsoft.Xna.Framework;
using Quest.Gui;
using System.Collections.Generic;
using MonoGame.Extended;
using System.IO;
using Quest.Tiles;
using MonoGUI;
using System.IO.Compression;
using System.Threading.Tasks;
using static Quest.TextureManager;

namespace Quest.Editor
{
    public class EditorWindow : Game
    {
        // Debug
        private int TilesDrawn;

        // Consts/readonlys
        private Xna.Vector2 MiddleCoord => Xna.Vector2.Ceiling((Constants.Window / 2) / Constants.TileSize);

        // Inputs
        private KeyboardState keyState;
        private KeyboardState previousKeyState;
        private MouseState mouseState;
        private MouseState previousMouseState;
        private Point mouseCoord;

        // Editing
        private Tile[] Tiles;
        private Xna.Vector2 Camera;
        private Xna.Vector2 tileSize = Constants.TileSize;
        private Xna.Vector2 tileSize2D = new(Constants.TileSize.X, Constants.TileSize.Y);
        private Dictionary<string, Texture2D> TileTextures = new();
        private TileType Material;
        private int Selection;
        private readonly Color highlightColor = new(1, 1, 1, .8f);
        private float modifier;
        private string LevelName = "new_level";
        private float time = 0;
        private Xna.Point Spawn = Constants.MapSize / new Point(2, 2);

        // Deltatime
        private float delta;
        // Devices
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        // Fonts
        public SpriteFont Arial { get; private set; }
        public SpriteFont ArialSmall { get; private set; }
        public SpriteFont ArialLarge { get; private set; }
        public EditorWindow()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                //PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
                //PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height,
                PreferredBackBufferWidth = (int)Constants.Window.X,
                PreferredBackBufferHeight = (int)Constants.Window.Y,
                IsFullScreen = false,
                SynchronizeWithVerticalRetrace = true,
            };
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = false;

            Logger.Log("Created EditorWindow.");
        }

        protected override void Initialize()
        {
            // Defaults
            keyState = Keyboard.GetState();
            previousKeyState = keyState;
            mouseState = Mouse.GetState();
            previousMouseState = mouseState;
            Tiles = new Tile[Constants.MapSize.X * Constants.MapSize.Y];
            for (int t = 0; t < Constants.MapSize.X * Constants.MapSize.Y; t++)
            {
                Tile tile = new Sky(new(t % Constants.MapSize.X, t / Constants.MapSize.Y));
                SetTile(tile);
            }
            Camera = new Xna.Vector2(128 * Constants.TileSize.X, 128 * Constants.TileSize.Y) - Constants.Middle;
            Material = TileType.Water;
            Selection = (int)Material;

            Logger.Log("Initialized EditorWindow.");
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Fonts
            Arial = Content.Load<SpriteFont>("Fonts/Arial");
            ArialSmall = Content.Load<SpriteFont>("Fonts/ArialSmall");
            ArialLarge = Content.Load<SpriteFont>("Fonts/ArialLarge");
            Logger.Log("Loaded fonts.");
        }

        protected override void Update(GameTime gameTime)
        {
            // Inputs
            keyState = Keyboard.GetState();
            mouseState = Mouse.GetState();
            mouseCoord = Vector2.Floor((mouseState.Position.ToVector2() + Camera) / tileSize2D).ToPoint();

            // Exit
            if (IsKeyDown(Keys.Escape))
            {
                Logger.Log($"Program closed.");
                Exit();
            }

            // Delta time
            delta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Time
            time += delta;

            // Movement
            if (IsKeyDown(Keys.LeftShift)) modifier = 0.5f;
            else if (IsKeyDown(Keys.LeftControl)) modifier = 2f;
            else modifier = 1f;

            if (IsAnyKeyDown(Keys.A, Keys.Left)) Camera.X -= 600 * delta * modifier;
            if (IsAnyKeyDown(Keys.D, Keys.Right)) Camera.X += 600 * delta * modifier;
            if (IsAnyKeyDown(Keys.S, Keys.Down)) Camera.Y += 600 * delta * modifier;
            if (IsAnyKeyDown(Keys.W, Keys.Up)) Camera.Y -= 600 * delta * modifier;
            Camera = Vector2.Clamp(Camera, new Xna.Vector2(0, 0), (tileSize * Constants.MapSize.ToVector2()) - Constants.Window);

            // Open file
            if (IsAllKeysDown(Keys.O, Keys.LeftControl))
            {
                // Input file name
                string filename = Logger.Input("Open level file: ");
                if (!string.IsNullOrEmpty(filename))
                {
                    try
                    {
                        ReadLevel(filename);
                        LevelName = filename;
                        Logger.Log($"Opened level '{filename}'.");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to open level '{filename}': {ex.Message}");
                    }
                }
            }

            // Change material
            if (mouseState.ScrollWheelValue > previousMouseState.ScrollWheelValue)
            {
                Selection = (Selection + 1) % Constants.TileNames.Length;
                Material = (TileType)Enum.Parse(typeof(TileType), Constants.TileNames[Selection]);
                Logger.Log($"Material set to '{Material}'.");
            }
            if (mouseState.ScrollWheelValue < previousMouseState.ScrollWheelValue)
            {
                Selection = (Selection - 1) % Constants.TileNames.Length;
                if (Selection < 0) Selection += Constants.TileNames.Length;
                Material = (TileType)Enum.Parse(typeof(TileType), Constants.TileNames[Selection]);
                Logger.Log($"Material set to '{Material}'.");
            }

            // Draw
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (GetTile(mouseCoord).Type != Material)
                {
                    // Add tile
                    Tile tile;
                    if (Selection == (int)TileType.Stairs)
                        tile = new Stairs(mouseCoord, "_null", Constants.MiddleCoord);
                    else
                        tile = GameManager.TileFromId(Selection, mouseCoord);

                    SetTile(tile);
                    Logger.Log($"Set tile to '{Material}' @ {mouseCoord.X}, {mouseCoord.Y}.");
                }
            }
            // Edit options
            if (mouseState.RightButton == ButtonState.Pressed)
            {
                Tile tileBelow = Tiles[mouseCoord.X + mouseCoord.Y * Constants.MapSize.X];
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

            // Save
            if (IsKeyPressed(Keys.E) && IsKeyDown(Keys.LeftControl)) SaveLevel(this);

            // Level info
            if (IsKeyPressed(Keys.L) && IsKeyDown(Keys.LeftControl))
            {
                SetSpawn();
            }

            // Set previous key state
            previousKeyState = keyState;
            previousMouseState = mouseState;

            // Final
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Clear
            GraphicsDevice.Clear(Color.Magenta);
            spriteBatch.Begin(samplerState:SamplerState.PointClamp);

            // Tiles
            TilesDrawn = 0;
            for (int t = 0; t < Tiles.Length; t++)
            {
                Tile tile = Tiles[t];
                // Draw each tile using the sprite batch
                Xna.Vector2 dest = tile.Location.ToVector2() * tileSize - Camera;
                // Check x in bounds
                if (dest.X + Constants.TileSize.X < 0 || dest.X > Constants.Window.X) continue;
                if (dest.Y + Constants.TileSize.Y * 2 < 0 || dest.Y > Constants.Window.Y) continue;
                dest.Round();
                // Draw
                TextureID texture = (TextureID)(Enum.TryParse(typeof(TextureID), tile.GetType().ToString(), out var tileTex) ? tileTex : TextureID.Null);
                Color color = tile.Type == TileType.Water ? Color.Lerp(Color.LightBlue, Color.Blue, 0.1f * (float)Math.Sin(time + tile.Location.X + tile.Location.Y)) : Color.White;
                Rectangle rect = new((int)dest.X, (int)dest.Y, (int)tileSize.X, (int)tileSize.Y);
                DrawTexture(spriteBatch, texture, rect, source:TileTextureSource(tile), scale: new(4));
                TilesDrawn++;
            }

            // Spawn
            Xna.Vector2 spawnPos = Spawn.ToVector2() * tileSize - Camera;
            spriteBatch.FillRectangle(new(spawnPos + tileSize/4, tileSize / 2), Color.Magenta);

            // Cursor
            Vector2 cursorPos = mouseCoord.ToVector2() * tileSize - Camera;
            spriteBatch.FillRectangle(new(cursorPos, tileSize), Color.White);
            TextureID ghostTile = (TextureID)(Enum.TryParse(typeof(TextureID), Material.ToString(), out var tex) ? tex : TextureID.Null);
            DrawTexture(spriteBatch, ghostTile, new(cursorPos.ToPoint(), Constants.TileSize.ToPoint()), source:Constants.ZeroSource, color:highlightColor, scale:new(4));

            // Text gui
            spriteBatch.DrawString(Arial, $"FPS: {1f / delta:0.0}\nCamera: {Camera}\nMaterial: {Material} [{Selection}]\nTiles Drawn: {TilesDrawn}\nCoord: {mouseCoord}", new Vector2(10, 10), Color.Black);

            // Final
            spriteBatch.End();
            base.Draw(gameTime);
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
            Spawn = new(reader.ReadByte(), reader.ReadByte());
            Camera = (Spawn - Constants.MiddleCoord).ToVector2() * tileSize;

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
                    tile = GameManager.TileFromId(type, new(i % Constants.MapSize.X, i / Constants.MapSize.X));
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
            Tiles = tilesBuffer;
        }
        public void SetTile(Tile tile)
        {
            Tiles[tile.Location.X + tile.Location.Y * Constants.MapSize.X] = tile;
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
        public static void SaveLevel(EditorWindow editor)
        {
            // Input
            string name = Logger.Input("Export file name: ");

            // Parse
            Directory.CreateDirectory("Levels");
            using FileStream fileStream = File.Create($"Levels/{name}.lvl");
            using GZipStream gzipStream = new(fileStream, CompressionLevel.Optimal);
            using BinaryWriter writer = new(gzipStream);

            // Write spawn
            writer.Write(IntToByte(editor.Spawn.X));
            writer.Write(IntToByte(editor.Spawn.Y));

            // Dimensions
            for (int i = 0; i < Constants.MapSize.X * Constants.MapSize.Y; i++)
            {
                Tile tile = editor.Tiles[i];
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
            }
        }
        // Key presses
        public static byte IntToByte(int value)
        {
            if (value < 0 || value > 255)
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be between 0 and 255.");
            return (byte)value;
        }
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

            if (x > 0 && GetTile(x - 1, y).Type == tile.Type) mask |= 1; // left
            if (y < Constants.MapSize.Y - 1 && GetTile(x, y + 1).Type == tile.Type) mask |= 2; // down
            if (x < Constants.MapSize.X - 1 && GetTile(x + 1, y).Type == tile.Type) mask |= 4; // right
            if (y > 0 && GetTile(x, y - 1).Type == tile.Type) mask |= 8; // up

            return mask;
        }
        public Tile GetTile(Xna.Point coord)
        {
            if (coord.X < 0 || coord.X >= Constants.MapSize.X || coord.Y < 0 || coord.Y >= Constants.MapSize.Y)
                throw new ArgumentOutOfRangeException(nameof(coord), "Coordinates are out of bounds of the level.");
            return Tiles[coord.X + coord.Y * Constants.MapSize.X];
        }
        public Tile GetTile(int x, int y)
        {
            return GetTile(new Point(x, y));
        }
    }
}
