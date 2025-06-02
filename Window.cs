using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Xna = Microsoft.Xna.Framework;
using Quest.Gui;
using Quest.Tiles;

namespace Quest
{
    public class Window : Game
    {
        // Inputs
        private KeyboardState keyState;
        private KeyboardState previousKeyState;
        private MouseState mouseState;

        // Deltatime
        private float delta;
        // Devices
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        public GameHandler GameHandler;

        // Fonts
        public SpriteFont Arial { get; private set; }
        public SpriteFont ArialSmall { get; private set; }
        public SpriteFont ArialLarge { get; private set; }
        public SpriteFont PixelOperator { get; private set; }
        // Robot sprites
        public Texture2D RobotIdle { get; private set; }
        public Texture2D RobotUp { get; private set; }
        public Texture2D RobotDown { get; private set; }
        public Texture2D RobotLeft { get; private set; }
        public Texture2D RobotRight { get; private set; }

        // Movements
        private int moveX;
        private int moveY;

        // Vectors
        public Window()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                //PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width,
                //PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height,
                PreferredBackBufferWidth = (int)Constants.Window.X,
                PreferredBackBufferHeight = (int)Constants.Window.Y,
                IsFullScreen = false,
                //SynchronizeWithVerticalRetrace = true,
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

            // Robot sprites
            RobotIdle = Content.Load<Texture2D>("Images/Robot/Robot_F");
            RobotDown = Content.Load<Texture2D>("Images/Robot/Robot_D");
            RobotUp = Content.Load<Texture2D>("Images/Robot/Robot_U");
            RobotLeft = Content.Load<Texture2D>("Images/Robot/Robot_L");
            RobotRight = Content.Load<Texture2D>("Images/Robot/Robot_R");
        }

        protected override void Update(GameTime gameTime)
        {
            // Inputs
            keyState = Keyboard.GetState();
            mouseState = Mouse.GetState();
            // Exit
            if (IsKeyDown(Keys.Escape)) Exit();

            // Delta time
            delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Movement
            moveX = 0; moveY = 0;
            moveX += IsAnyKeyDown(Keys.A, Keys.Left) ? -Constants.PlayerSpeed : 0;
            moveX += IsAnyKeyDown(Keys.D, Keys.Right) ? Constants.PlayerSpeed : 0;
            moveY += IsAnyKeyDown(Keys.W, Keys.Up) ? -Constants.PlayerSpeed : 0;
            moveY += IsAnyKeyDown(Keys.S, Keys.Down) ? Constants.PlayerSpeed : 0;
            GameHandler.Move(moveX, moveY);

            // Game update
            GameHandler.Update(delta);

            // Set previous key state
            previousKeyState = keyState;

            // Final
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Clear
            GraphicsDevice.Clear(Color.Magenta);
            spriteBatch.Begin(samplerState:SamplerState.PointClamp);

            // Draw game
            GameHandler.Draw();

            // Player
            Xna.Vector2 playerDest = Constants.Middle + GameHandler.CameraDest - GameHandler.Camera;
            if (moveX < 0)
                spriteBatch.Draw(RobotLeft, playerDest, Color.White);
            else if (moveX > 0)
                spriteBatch.Draw(RobotRight, playerDest, Color.White);
            else if (moveY < 0)
                spriteBatch.Draw(RobotUp, playerDest, Color.White);
            else if (moveY > 0)
                spriteBatch.Draw(RobotDown, playerDest, Color.White);
            else
                spriteBatch.Draw(RobotIdle, playerDest, Color.White);

            // Text gui
            spriteBatch.DrawString(Arial, $"FPS: {1f / delta:0.0}\nCamera: {GameHandler.Camera}\nTiles Drawn: {GameHandler.TilesDrawn}\nTile Below: {(GameHandler.TileBelow == null ? "none" : GameHandler.TileBelow.Type)}\nCoord: {GameHandler.Coord}\nLevel: {GameHandler.Level.Name}", new Vector2(10, 10), Color.Black);
            string frameString = string.Join("\n", GameHandler.FrameTimes.Select(kv => $"{kv.Key}: {kv.Value:0.0}ms"));
            spriteBatch.DrawString(Arial, frameString, new Vector2(Constants.Window.X - 200, 50), Color.Black);

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
    }
}
