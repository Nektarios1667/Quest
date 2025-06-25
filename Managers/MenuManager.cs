namespace Quest.Managers;
public class MenuManager
{
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
            case GameState.Game or GameState.Editor:
                // DrawGame(gameManager);
                break;
            case GameState.Death:
                // DrawDeath(gameManager);
                break;
        }
    }
    private void DrawMenu(GameManager gameManager)
    {
        // Draw the menu screen
        DebugManager.StartBenchmark("DrawMenu");
        gameManager.Batch.FillRectangle(new Rectangle(Point.Zero, Constants.Window), Color.Black);
        gameManager.Batch.DrawString(TextureManager.PixelOperator, "Quest", Constants.Middle.ToVector2() - TextureManager.PixelOperator.MeasureString("Quest") * 2, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
        gameManager.Batch.DrawString(TextureManager.PixelOperator, "Press space to start", Constants.Middle.ToVector2() - TextureManager.PixelOperator.MeasureString("Press space to start") / 2 + new Vector2(0, 80), Color.White);

        // Check start game
        if (InputManager.KeyDown(Keys.Space))
        {
            StateManager.State = GameState.Game;
        }
        DebugManager.StartBenchmark("DrawMenu");
    }
}
