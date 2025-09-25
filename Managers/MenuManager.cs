using Microsoft.Xna.Framework.Content;
using MonoGUI;

namespace Quest.Managers;
public class MenuManager
{
    public GUI MainMenu { get; private set; }
    public MenuManager(Game window, SpriteBatch batch, ContentManager content)
    {
        // Main Menu
        MainMenu = new(window, batch, PixelOperator);
        MainMenu.LoadContent(content);
        Button startButton = new(MainMenu, new(Constants.Middle.X - 150, 300), new(300, 75), Color.White, Color.Black, Color.Gray, () => StateManager.State = GameState.Game, [], text: "Start Game", font: PixelOperatorSubtitle);
        MainMenu.Widgets = [startButton];
    }
    public void Update(GameManager gameManager)
    {
        switch (StateManager.State)
        {
            case GameState.MainMenu:
                MainMenu.Update(gameManager.DeltaTime, InputManager.MouseState, InputManager.KeyboardState);
                break;
            case GameState.Settings:
                // DrawSettings(gameManager);
                break;
        }
    }
    public void Draw(GameManager gameManager)
    {
        switch (StateManager.State)
        {
            case GameState.MainMenu:
                DrawMenu(gameManager);
                break;
            case GameState.Settings:
                // DrawSettings(gameManager);
                break;
        }
    }
    private void DrawMenu(GameManager gameManager)
    {
        // Draw the menu screen
        DebugManager.StartBenchmark("DrawMenu");
        FillRectangle(gameManager.Batch, new(Point.Zero, Constants.Window), Color.Black);
        MainMenu.Draw();
        //gameManager.Batch.DrawString(PixelOperator, "Quest", Constants.Middle.ToVector2() - PixelOperator.MeasureString("Quest") * 2, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
        //gameManager.Batch.DrawString(PixelOperator, "Press space to start", Constants.Middle.ToVector2() - PixelOperator.MeasureString("Press space to start") / 2 + new Vector2(0, 80), Color.White);

        //// Check start game
        //if (InputManager.KeyDown(Keys.Space))
        //{
        //    StateManager.State = GameState.Game;
        //}
        DebugManager.EndBenchmark("DrawMenu");
    }
}
