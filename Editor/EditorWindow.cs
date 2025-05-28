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
using System.Reflection.Metadata;
using MonoGame.Extended.Input;
using SharpDX.Direct3D9;
using SharpDX.Direct2D1.Effects;
using System.IO;

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
        private Xna.Vector2 mouseCoord;

        // Editing
        private List<Tile> Tiles;
        private Xna.Vector2 Camera;
        private Xna.Vector2 tileSize = Constants.TileSize;
        private Xna.Vector2 tileSize2D = new(Constants.TileSize.X, Constants.TileSize.Y);
        private Dictionary<string, Texture2D> TileTextures = new();
        private Tile.TileType Material;
        private int Selection;
        private readonly Color highlightColor = new(1, 1, 1, .8f);
        private float modifier;

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
            Material = Tile.TileType.Water;
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
        }

        protected override void Update(GameTime gameTime)
        {
            // Inputs
            keyState = Keyboard.GetState();
            mouseState = Mouse.GetState();
            mouseCoord = Xna.Vector2.Floor((mouseState.Position.ToVector2() + Camera) / tileSize2D);

            // Exit
            if (IsKeyDown(Keys.Escape)) Exit();

            // Delta time
            delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Movemen
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
                Material = (Tile.TileType)Enum.Parse(typeof(Tile.TileType), Constants.TileNames[Selection]);
            }
            if (mouseState.ScrollWheelValue < previousMouseState.ScrollWheelValue)
            {
                Selection = (Selection - 1) % Constants.TileNames.Length;
                if (Selection < 0) Selection += Constants.TileNames.Length;
                Material = (Tile.TileType)Enum.Parse(typeof(Tile.TileType), Constants.TileNames[Selection]);
            }

            // Draw
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                // Add tile
                Tile tile = new(mouseCoord, Material);
                if (!Tiles.Any(t => t.Location == tile.Location))
                    Tiles.Add(tile);
            }
            if (mouseState.RightButton == ButtonState.Pressed)
            {
                // Remove tile
                Tile tile = new(mouseCoord, Material);
                Tiles.RemoveAll(t => t.Location == tile.Location);
            }

            // Save
            if (IsKeyPressed(Keys.S) && IsKeyDown(Keys.LeftControl)) SaveLevel();

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
                Xna.Vector2 dest = tile.Location * tileSize - Camera;
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
            Xna.Vector2 cursorPos = mouseCoord * tileSize - Camera;
            spriteBatch.FillRectangle(new(cursorPos, tileSize), Color.White);
            spriteBatch.Draw(TileTextures[Material.ToString()], cursorPos, highlightColor);

            // Text gui
            spriteBatch.DrawString(Arial, $"FPS: {1f / delta:0.0}\nCamera: {Camera}\nMaterial: {Material} [{Selection}]\nTiles Drawn: {TilesDrawn}\nCoord: {mouseCoord}", new Vector2(10, 10), Color.Black);

            // Final
            spriteBatch.End();
            base.Draw(gameTime);
        }
        public void SaveLevel()
        {
            // Auto collect data
            Tile? left = null; Tile? right = null; Tile? up = null; Tile? down = null;
            foreach (var tile in Tiles)
            {
                if (left == null || tile.Location.X < left.Value.Location.X) { left = tile; }
                if (right == null || tile.Location.X > right.Value.Location.X) { right = tile; }
                if (up == null || tile.Location.Y < up.Value.Location.Y) { up = tile; }
                if (down == null || tile.Location.Y > down.Value.Location.Y) { down = tile; }
            }
            // Check
            if (left == null || right == null || up == null || down == null)
                throw new InvalidOperationException("No tiles to save.");

            int width = (int)(right.Value.Location.X - left.Value.Location.X + 1);
            int height = (int)(down.Value.Location.Y - up.Value.Location.Y + 1);

            // Parse
            using (BinaryWriter writer = new(File.Open("new_level.lvl", FileMode.Create)))
            {
                // Dimensions
                writer.Write((ushort)Tiles.Count);
                writer.Write(IntToByte(width));
                writer.Write(IntToByte(height));
                for (int i = 0; i < Tiles.Count; i++)
                {
                    Tile tile = Tiles[i];
                    // Write tile data
                    writer.Write(IntToByte((int)tile.Location.X));
                    writer.Write(IntToByte((int)tile.Location.Y));
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
