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
        private List<Tile> Tiles;
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
            Tiles = [];
            Camera = new Vector2(5000, 5000) + Constants.Middle;
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
                Camera = Vector2.Clamp(Camera, new Xna.Vector2(0, 0), new Xna.Vector2(10000, 10000));

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
                    Tile tile = GameHandler.TileFromId(Selection, mouseCoord);
                    if (!Tiles.Any(t => t.Location == tile.Location))
                        Tiles.Add(tile);
                }
                if (mouseState.RightButton == ButtonState.Pressed)
                {
                    // Remove tile
                    Tile tile = GameHandler.TileFromId(Selection, mouseCoord);
                    Tiles.RemoveAll(t => t.Location == tile.Location);
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
            Xna.Vector2 cursorPos = mouseCoord.ToVector2() * tileSize - Camera;
            spriteBatch.FillRectangle(new(cursorPos, tileSize), Color.White);
            spriteBatch.Draw(TileTextures[Material.ToString()], cursorPos, highlightColor);

            // Text gui
            spriteBatch.DrawString(Arial, $"FPS: {1f / delta:0.0}\nCamera: {Camera}\nMaterial: {Material} [{Selection}]\nTiles Drawn: {TilesDrawn}\nCoord: {mouseCoord}", new Vector2(10, 10), Color.Black);

            // Gui
            Gui.Draw();

            // Final
            spriteBatch.End();
            base.Draw(gameTime);
        }
        public static void SaveLevel(EditorWindow editor)
        {
            string name = editor.LevelInput.Text;
            editor.LevelName = name;
            editor.LevelPopup.Title = name;
            editor.LevelInput.SetText("");
            Console.WriteLine(editor.LevelPopup.Title);
            // Auto collect data
            Tile left = null; Tile right = null; Tile up = null; Tile down = null;
            foreach (var tile in editor.Tiles)
            {
                if (left == null || tile.Location.X < left.Location.X) { left = tile; }
                if (right == null || tile.Location.X > right.Location.X) { right = tile; }
                if (up == null || tile.Location.Y < up.Location.Y) { up = tile; }
                if (down == null || tile.Location.Y > down.Location.Y) { down = tile; }
            }
            // Check
            int width = 0;
            int height = 0;
            if (left != null && right != null && up != null && down != null)
            {
                width = (right.Location.X - left.Location.X + 1);
                height = (down.Location.Y - up.Location.Y + 1);
            }

            // Parse
            Directory.CreateDirectory("Levels");
            using (BinaryWriter writer = new(File.Open($"Levels/{name}.lvl", FileMode.Create)))
            {
                // Dimensions
                writer.Write((ushort)editor.Tiles.Count);
                writer.Write(IntToByte(width));
                writer.Write(IntToByte(height));
                for (int i = 0; i < editor.Tiles.Count; i++)
                {
                    Tile tile = editor.Tiles[i];
                    // Write tile data
                    writer.Write(IntToByte(tile.Location.Y));
                    writer.Write(IntToByte((int)tile.Type));
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
