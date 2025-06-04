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
        private SpriteBatch spriteBatch;
        public GameHandler GameHandler;
        // Textures
        public Texture2D CursorArrow { get; private set; }

        // Fonts
        public SpriteFont Arial { get; private set; }
        public SpriteFont ArialSmall { get; private set; }
        public SpriteFont ArialLarge { get; private set; }
        public SpriteFont PixelOperator { get; private set; }
        // Robot sprites
        public Texture2D BlueMage { get; private set; }
        public Point MageSize { get; private set; }
        public Point MageHalfSize { get; private set; }
        // Movements
        private int moveX;
        private int moveY;
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

            // Handlers
            GameHandler = new GameHandler(this, spriteBatch);
            GameHandler.ReadLevel("island_house");
            GameHandler.ReadLevel("island_house_basement");
            GameHandler.LoadLevel(0);
            GameHandler.LoadContent(Content);

            // Characters
            BlueMage = Content.Load<Texture2D>("Images/Characters/BlueMage");
            MageSize = new(BlueMage.Width / 4, BlueMage.Height / 5);
            MageHalfSize = new(MageSize.X / 2, MageSize.Y / 2);

            // Shaders
            Grayscale = Content.Load<Effect>("Shaders/Grayscale");

            // Other
            CursorArrow = Content.Load<Texture2D>("Images/Gui/CursorArrow");
        }

        protected override void Update(GameTime gameTime)
        {
            GameHandler.Watch.Restart();

            // Inputs
            keyState = Keyboard.GetState();
            mouseState = Mouse.GetState();
            // Exit
            if (IsKeyDown(Keys.Escape)) Exit();

            // Delta debugUpdateTime
            delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Inventory
            if (IsKeyPressed(Keys.I))
                GameHandler.Inventory.Opened = !GameHandler.Inventory.Opened;

            // Movement
            if (!GameHandler.Inventory.Opened)
            {
                // Movement
                moveX = 0; moveY = 0;
                moveX += IsAnyKeyDown(Keys.A, Keys.Left) ? -Constants.PlayerSpeed : 0;
                moveX += IsAnyKeyDown(Keys.D, Keys.Right) ? Constants.PlayerSpeed : 0;
                moveY += IsAnyKeyDown(Keys.W, Keys.Up) ? -Constants.PlayerSpeed : 0;
                moveY += IsAnyKeyDown(Keys.S, Keys.Down) ? Constants.PlayerSpeed : 0;
                GameHandler.Move(moveX, moveY);
            }

            // Time
            GameHandler.FrameTimes["InputUpdate"] = GameHandler.Watch.Elapsed.TotalMilliseconds;

            // Game updates
            if (!GameHandler.Inventory.Opened)
                // Update
                GameHandler.Update(delta, previousMouseState, mouseState);
            else // Only update gui
                GameHandler.UpdateGui(delta, previousMouseState, mouseState);

            // Set previous key state
            GameHandler.Watch.Restart();
            previousKeyState = keyState;
            previousMouseState = mouseState;

            // Final
            debugUpdateTime += delta;
            GameHandler.FrameTimes["OtherUpdates"] = GameHandler.Watch.Elapsed.TotalMilliseconds;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Clear and start shader gui
            GraphicsDevice.Clear(Color.Magenta);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            // Draw game
            GameHandler.DrawTiles();

            // Player
            GameHandler.Watch.Restart();
            DrawPlayer();
            if (Constants.DRAW_HITBOXES)
                DrawPlayerHitbox();
            GameHandler.FrameTimes["PlayerDraw"] = GameHandler.Watch.Elapsed.TotalMilliseconds;

            // Inventory darkening
            if (GameHandler.Inventory.Opened)
                spriteBatch.FillRectangle(new(Vector2.Zero, Constants.Window), Constants.DarkenScreen);

            // Close
            spriteBatch.End();

            // Non shader draws
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            // Gui
            GameHandler.DrawGui();

            // Text info
            GameHandler.Watch.Restart();
            if (Constants.TEXT_INFO)
            {
                // Background
                spriteBatch.FillRectangle(new(0, 0, 200, 140), Color.Black * .8f);
                spriteBatch.DrawString(Arial, $"FPS: {(cacheDelta != 0 ? 1f / cacheDelta : 0):0.0}\nTime: {GameHandler.Time:0.00}\nCamera: {GameHandler.Camera.X:0.0},{GameHandler.Camera.Y:0.0}\nTile Below: {(GameHandler.TileBelow == null ? "none" : GameHandler.TileBelow.Type)}\nCoord: {GameHandler.Coord}\nLevel: {GameHandler.Level.Name}", new Vector2(10, 10), Color.White);
            }

            // Frame info
            if (Constants.FRAME_INFO)
            {
                spriteBatch.FillRectangle(new(Constants.Window.X - 190, 0, 190, GameHandler.FrameTimes.Count * 20), Color.Black * .8f);
                string frameString = string.Join("\n", frameTimes.Select(kv => $"{kv.Key}: {kv.Value:0.0}ms"));
                spriteBatch.DrawString(Arial, frameString, new Vector2(Constants.Window.X - 180, 10), Color.White);
            }
            GameHandler.FrameTimes["DebugTextDraw"] = GameHandler.Watch.Elapsed.TotalMilliseconds;

            // Frame bar
            GameHandler.Watch.Restart();
            if (Constants.FRAME_BAR)
                DrawFrameBar();
            GameHandler.FrameTimes["FrameBarDraw"] = GameHandler.Watch.Elapsed.TotalMilliseconds;

            // Cursor
            spriteBatch.Draw(CursorArrow, mouseState.Position.ToVector2(), LMouseDown ? Color.DarkGray : Color.White);

            // Final
            spriteBatch.End();
            base.Draw(gameTime);
        }
        // Utilities
        public bool TryDraw(Texture2D texture, Rectangle rect, Rectangle? sourceRect = null, Color color = default, float rotation = 0, Vector2 origin = default, Vector2 scale = default, SpriteEffects spriteEffect = default, float depth = 0)
        {
            // Defaults
            if (scale == default) scale = Vector2.One;
            if (color == default) color = Color.White;

            // Missing texture
            if (texture == null)
            {
                spriteBatch.FillRectangle(new(rect.X, rect.Y, rect.Width / 2, rect.Height / 2), Color.Magenta); // top left
                spriteBatch.FillRectangle(new(rect.X + rect.Width / 2, rect.Y, rect.Width / 2, rect.Height / 2), Color.Black); // top right
                spriteBatch.FillRectangle(new(rect.X, rect.Y + rect.Height / 2, rect.Width / 2, rect.Height / 2), Color.Black); // bottom left
                spriteBatch.FillRectangle(new(rect.X + rect.Width / 2, rect.Y + rect.Height / 2, rect.Width / 2, rect.Height / 2), Color.Magenta); // bottom right
                return false;
            }
            spriteBatch.Draw(texture, rect.Location.ToVector2(), sourceRect, color, rotation, origin, scale, spriteEffect, depth);
            return true;
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
        public void DrawFrameBar()
        {
            // Update info twice a second
            if (debugUpdateTime >= .5)
            {
                cacheDelta = delta;
                frameTimes = new Dictionary<string, double>(GameHandler.FrameTimes);
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
        public void DrawPlayerHitbox()
        {
            Vector2[] points = new Vector2[4];
            for (int c = 0; c < Constants.PlayerCorners.Length; c++)
                points[c] = Constants.Middle + Constants.PlayerCorners[c];
            spriteBatch.DrawLine(points[0], points[1], Color.Lime, 1); // Top line
            spriteBatch.DrawLine(points[2], points[3], Color.Lime, 1); // Bottom line
            spriteBatch.DrawLine(points[0], points[2], Color.Lime, 1); // Left line
            spriteBatch.DrawLine(points[1], points[3], Color.Lime, 1); // Right line
            spriteBatch.DrawLine(points[0], points[3], Color.Lime, 1); // Top left to bottom right
            spriteBatch.DrawLine(points[1], points[2], Color.Lime, 1); // Top right to bottom left
        }
        public void DrawPlayer()
        {
            int sourceRow = 0;
            if (moveX == 0 && moveY == 0) sourceRow = 0;
            else if (moveX < 0) sourceRow = 1;
            else if (moveX > 0) sourceRow = 3;
            else if (moveY > 0) sourceRow = 2;
            else if (moveY < 0) sourceRow = 4;
            Rectangle source = new((int)(GameHandler.Time * (sourceRow == 0 ? 1.5f : 6)) % 4 * MageSize.X, sourceRow * MageSize.Y, MageSize.X, MageSize.Y);
            Rectangle rect = new(Constants.Middle.ToPoint() - MageHalfSize, MageSize);
            TryDraw(BlueMage, rect, sourceRect: source);
        }
    }
}
