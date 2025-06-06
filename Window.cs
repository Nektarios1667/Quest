using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Xna = Microsoft.Xna.Framework;
using Quest.Gui;
using Quest.Tiles;
using MonoGame.Extended;
using System.Collections.Generic;
using static Quest.TextureManager;

namespace Quest
{
    public class Window : Game
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

        // DeltadebugUpdateTime
        private float delta;
        private Dictionary<string, double> frameTimes;
        // Devices
        private GraphicsDeviceManager graphics;
        protected SpriteBatch spriteBatch;
        public GameManager GameManager;
        // Textures
        public Texture2D CursorArrow { get; private set; }

        // Fonts
        public SpriteFont Arial { get; private set; }
        public SpriteFont ArialSmall { get; private set; }
        public SpriteFont ArialLarge { get; private set; }
        public SpriteFont PixelOperator { get; private set; }
        // Movements
        public int moveX;
        public int moveY;
        // Shaders
        public Effect Grayscale { get; private set; }

        // Debug
        private static readonly Color[] colors = {
            Color.Purple, new Color(255, 128, 128), new Color(128, 255, 128), new Color(255, 255, 180), new Color(128, 255, 255),
            Color.Brown, Color.Gray, new Color(192, 128, 64), new Color(64, 128, 192), new Color(192, 192, 64),
            new Color(64, 192, 128), new Color(192, 64, 128), new Color(160, 80, 0), new Color(80, 160, 0), new Color(0, 160, 80),
            new Color(160, 0, 80), new Color(96, 96, 192), new Color(192, 96, 96), new Color(96, 192, 96), new Color(192, 192, 96)
        };
        private float debugUpdateTime;
        private float cacheDelta;
        public Window()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                //PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
                //PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height,
                PreferredBackBufferWidth = (int)Constants.Window.X,
                PreferredBackBufferHeight = (int)Constants.Window.Y,
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
            GameManager = new GameManager(this, spriteBatch);
            GameManager.ReadLevel("island_house");
            GameManager.ReadLevel("island_house_basement");
            GameManager.LoadLevel(0);

            // Shaders
            Grayscale = Content.Load<Effect>("Shaders/Grayscale");

            // Other
            CursorArrow = Content.Load<Texture2D>("Images/Gui/CursorArrow");
        }

        protected override void Update(GameTime gameTime)
        {
            GameManager.Watch.Restart();

            // Inputs
            keyState = Keyboard.GetState();
            mouseState = Mouse.GetState();
            // Exit
            if (IsKeyDown(Keys.Escape)) Exit();

            // Delta debugUpdateTime
            delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Inventory
            if (IsKeyPressed(Keys.I))
                GameManager.Inventory.Opened = !GameManager.Inventory.Opened;

            // Movement
            if (!GameManager.Inventory.Opened)
            {
                // Movement
                moveX = 0; moveY = 0;
                moveX += IsAnyKeyDown(Keys.A, Keys.Left) ? -Constants.PlayerSpeed : 0;
                moveX += IsAnyKeyDown(Keys.D, Keys.Right) ? Constants.PlayerSpeed : 0;
                moveY += IsAnyKeyDown(Keys.W, Keys.Up) ? -Constants.PlayerSpeed : 0;
                moveY += IsAnyKeyDown(Keys.S, Keys.Down) ? Constants.PlayerSpeed : 0;
                GameManager.Move(moveX, moveY);
            }

            // Time
            GameManager.FrameTimes["InputUpdate"] = GameManager.Watch.Elapsed.TotalMilliseconds;

            // Game updates
            if (!GameManager.Inventory.Opened)
                // Update
                GameManager.Update(delta, previousMouseState, mouseState);
            else // Only update gui
                GameManager.Update(delta, previousMouseState, mouseState);

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
            GameManager.DrawTiles();
            GameManager.DrawCharacters();

            // Inventory darkening
            if (GameManager.Inventory.Opened)
                spriteBatch.FillRectangle(new(Vector2.Zero, Constants.Window), Constants.DarkenScreen, layerDepth:1);

            // Gui
            GameManager.DrawGui();

            // Minimap
            if (GameManager.Inventory.Opened)
                DrawMiniMap();

            // Text info
            GameManager.Watch.Restart();
            if (Constants.TEXT_INFO)
            {
                // Background
                spriteBatch.FillRectangle(new(0, 0, 200, 140), Color.Black * .8f);
                spriteBatch.DrawString(Arial, $"FPS: {(cacheDelta != 0 ? 1f / cacheDelta : 0):0.0}\nTime: {GameManager.Time:0.00}\nCamera: {GameManager.Camera.X:0.0},{GameManager.Camera.Y:0.0}\nTile Below: {(GameManager.TileBelow == null ? "none" : GameManager.TileBelow.Type)}\nCoord: {GameManager.Coord}\nLevel: {GameManager.Level.Name}" +
                    $"\nInventory: {GameManager.Inventory.Opened}", new Vector2(10, 10), Color.White);
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

            // Cursor
            Rectangle rect = new(mouseState.Position.X, mouseState.Position.Y, 30, 30);
            DrawTexture(spriteBatch, TextureID.CursorArrow, rect);

            // Final
            spriteBatch.End();
            base.Draw(gameTime);
        }
        // Key presses
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
        public void DrawMiniMap()
        {
            // Frame
            spriteBatch.DrawRectangle(new(7, Constants.Window.Y - Constants.MapSize.Y - 13, Constants.MapSize.X + 6, Constants.MapSize.Y + 6), Color.Black, 3);
            // Pixels
            for (int y = 0; y < Constants.MapSize.Y; y++)
            {
                for (int x = 0; x < Constants.MapSize.X; x++)
                {
                    // Get tile
                    Tile tile = GameManager.GetTile(new Point(x, y))!;
                    Color color = tile.Type switch
                    {
                        TileType.Sky => Constants.MinimapSky,
                        TileType.Grass => Constants.MinimapGrass,
                        TileType.Water => Constants.MinimapWater,
                        TileType.StoneWall => Constants.MinimapStoneWall,
                        TileType.Stairs => Constants.MinimapStairs,
                        TileType.Flooring => Constants.MinimapFlooring,
                        TileType.Sand => Constants.MinimapSand,
                        TileType.Dirt => Constants.MinimapDirt,
                        _ => Color.White
                    };
                    spriteBatch.DrawPoint(new(10 + x, Constants.Window.Y - Constants.MapSize.Y - 10 + y), color);
                }
            }
            // Player
            Vector2 dest = GameManager.Coord + new Vector2(10, Constants.Window.Y - Constants.MapSize.Y - 10);
            spriteBatch.DrawPoint(dest, Color.Red, size:2);
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
        // Editor
        protected void BaseUpdate(GameTime gameTime)
        {
            base.Update(gameTime);
        }
        protected void BaseDraw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
        protected void BaseInitialize()
        {
            base.Initialize();
        }
    }
}
