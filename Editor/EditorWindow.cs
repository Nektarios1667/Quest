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

        // Gui
        private GUI Gui { get; set; }
        private Popup LevelPopup { get; set; }
        private Input LevelInput { get; set; }

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

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Fonts
            Arial = Content.Load<SpriteFont>("Fonts/Arial");
            ArialSmall = Content.Load<SpriteFont>("Fonts/ArialSmall");
            ArialLarge = Content.Load<SpriteFont>("Fonts/ArialLarge");

            // Dynamically load images
            foreach (string filename in Constants.TileNames)
            {
                if (!string.IsNullOrEmpty(filename))
                {
                    Texture2D texture = Content.Load<Texture2D>($"Images/Tiles/{filename}");
                    TileTextures[filename] = texture;
                }
            }

            // Gui
            Gui = new(this, spriteBatch);
            Gui.LoadContent(Content);
            LevelPopup = new Popup(Gui, Constants.Middle - new Vector2(300, 200), new(600, 400), Color.Gray, LevelName, titleFont:Arial, barColor:new(80, 80, 80));
            LevelInput = new Input(Gui, LevelPopup.Location + new Vector2(5, 60), new(250, 30), Color.Black, Color.DarkGray, new(180, 180, 180), font: Arial);
            LevelPopup.Widgets = [
                new Label(Gui, LevelPopup.Location + new Vector2(5, 40), Color.Black, "Level:", font:Arial),
                LevelInput,
                new Button(Gui, LevelPopup.Location + new Vector2(5, 110), new(100, 40), Color.White, Color.Green, Color.Lime, SaveLevel, args:[this], text:"Save")
            ];
            Gui.Widgets = [LevelPopup];
            LevelPopup.Visible = false;
        }

        protected override void Update(GameTime gameTime)
        {
            // Inputs
            keyState = Keyboard.GetState();
            mouseState = Mouse.GetState();
            mouseCoord = Xna.Vector2.Floor((mouseState.Position.ToVector2() + Camera) / tileSize2D).ToPoint();

            // Exit
            if (IsKeyDown(Keys.Escape)) Exit();

            // Delta time
            delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Editor
            if (!LevelPopup.Visible)
            {
                // Movement
                if (IsKeyDown(Keys.LeftShift)) modifier = 0.5f;
                else if (IsKeyDown(Keys.LeftControl)) modifier = 2f;
                else modifier = 1f;

                if (IsAnyKeyDown(Keys.A, Keys.Left)) Camera.X -= 600 * delta * modifier;
                if (IsAnyKeyDown(Keys.D, Keys.Right)) Camera.X += 600 * delta * modifier;
                if (IsAnyKeyDown(Keys.S, Keys.Down)) Camera.Y += 600 * delta * modifier;
                if (IsAnyKeyDown(Keys.W, Keys.Up)) Camera.Y -= 600 * delta * modifier;
                Camera = Vector2.Clamp(Camera, new Xna.Vector2(0, 0), (tileSize * Constants.MapSize.ToVector2()) - Constants.Window);

                // Change material
                if (mouseState.ScrollWheelValue > previousMouseState.ScrollWheelValue)
                {
                    Selection = (Selection + 1) % Constants.TileNames.Length;
                    Material = (TileType)Enum.Parse(typeof(TileType), Constants.TileNames[Selection]);
                }
                if (mouseState.ScrollWheelValue < previousMouseState.ScrollWheelValue)
                {
                    Selection = (Selection - 1) % Constants.TileNames.Length;
                    if (Selection < 0) Selection += Constants.TileNames.Length;
                    Material = (TileType)Enum.Parse(typeof(TileType), Constants.TileNames[Selection]);
                }

                // Draw
                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    // Add tile
                    Tile tile;
                    if (Selection == (int)TileType.Stairs)
                        tile = new Stairs(mouseCoord, "_null", Constants.MiddleCoord);
                    else 
                        tile = GameHandler.TileFromId(Selection, mouseCoord);

                    SetTile(tile);
                }
                // Edit options
                if (mouseState.RightButton == ButtonState.Pressed)
                {
                    Tile tileBelow = Tiles.FirstOrDefault(t => t.Location == mouseCoord);
                    if (tileBelow is Stairs stairs)
                    {
                        // Destination level
                        Console.WriteLine($"__Editing Stairs__\nDest level [{stairs.DestLevel}]: ");
                        string resp = Console.ReadLine();
                        if (resp != "") stairs.DestLevel = resp;

                        // Destination position
                        Console.WriteLine($"\nDest position [{stairs.DestPosition.X}, {stairs.DestPosition.Y}]: ");
                        resp = Console.ReadLine();
                        if (resp != "")
                        {
                            string[] parts = resp.Split(',');
                            if (parts.Length == 2 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
                                if (x >= 0 && x < Constants.MapSize.X && y >= 0 && y < Constants.MapSize.Y)
                                    stairs.DestPosition = new(x, y);
                                else
                                    Console.WriteLine($"Position out of bounds - must be within the map size {Constants.MapSize.X}x{Constants.MapSize.Y}.");
                            else
                                Console.WriteLine("Invalid position format - use 'x,y'.");
                        }
                    }
                }
            }

            // Save
            if (IsKeyPressed(Keys.S) && IsKeyDown(Keys.LeftControl)) LevelPopup.Visible = true;

            // Gui
            Gui.Update(delta, mouseState, keyState);

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
            spriteBatch.Begin();

            // Tiles
            TilesDrawn = 0;
            foreach (Tile tile in Tiles)
            {
                // Draw each tile using the sprite batch
                Xna.Vector2 dest = tile.Location.ToVector2() * tileSize - Camera;
                // Check x in bounds
                if (dest.X + Constants.TileSize.X < 0 || dest.X > Constants.Window.X) continue;
                if (dest.Y + Constants.TileSize.Y * 2 < 0 || dest.Y > Constants.Window.Y) continue;
                // Draw
                dest.Round();
                Texture2D texture = TileTextures[tile.Type.ToString()];
                spriteBatch.Draw(texture, dest, Color.White);
                TilesDrawn++;
            }

            // Cursor
            if (!LevelPopup.Visible)
            {
                Vector2 cursorPos = mouseCoord.ToVector2() * tileSize - Camera;
                spriteBatch.FillRectangle(new(cursorPos, tileSize), Color.White);
                spriteBatch.Draw(TileTextures[Material.ToString()], cursorPos, highlightColor);
            }

            // Text gui
            spriteBatch.DrawString(Arial, $"FPS: {1f / delta:0.0}\nCamera: {Camera}\nMaterial: {Material} [{Selection}]\nTiles Drawn: {TilesDrawn}\nCoord: {mouseCoord}", new Vector2(10, 10), Color.Black);

            // Gui
            Gui.Draw();

            // Final
            spriteBatch.End();
            base.Draw(gameTime);
        }
        public void SetTile(Tile tile)
        {
            Tiles[tile.Location.X + tile.Location.Y * Constants.MapSize.X] = tile;
        }
        public static void SaveLevel(EditorWindow editor)
        {
            string name = editor.LevelInput.Text;
            editor.LevelName = name;
            editor.LevelPopup.Title = name;
            editor.LevelInput.SetText("");

            // Parse
            Directory.CreateDirectory("Levels");
            using FileStream fileStream = File.Create($"Levels/{name}.lvl");
            using GZipStream gzipStream = new(fileStream, CompressionLevel.Optimal);
            using BinaryWriter writer = new(gzipStream);
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
    }
}
