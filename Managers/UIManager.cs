using Quest.Gui;

namespace Quest.Managers;
public class UIManager
{
    public Gui.Gui Gui { get; private set; } // GUI handler
    public NotificationArea LootNotifications { get; private set; } // Loot pickup notifications
    public StatusBar HealthBar { get; private set; }
    public static readonly Point lootStackOffset = new(4, 4);
    private float deathTime = -1;
    private RenderTarget2D? minimap;
    public UIManager()
    {
        Gui = new()
        {
            Widgets = [
                HealthBar = new StatusBar(new(10, Constants.Window.Y - 35), new(300, 25), Color.Green, Color.Red, 100, 100),
                LootNotifications = new NotificationArea(Constants.Middle - new Point(0, Constants.MageHalfSize.Y + 15), 5, TextureManager.PixelOperatorBold)
            ]
        };
    }
    public void Update(GameManager gameManager)
    {
        // Gui
        DebugManager.StartBenchmark("GuiUpdate");
        Gui.Update(gameManager);
        DebugManager.EndBenchmark("GuiUpdate");

        // Resoawn
        if (StateManager.State == GameState.Death && InputManager.KeyPressed(Keys.Space))
            gameManager.Respawn();
    }
    public void Draw(GraphicsDevice device, GameManager gameManager, Inventory inventory)
    {
        DebugManager.StartBenchmark("GuiDraw");

        // Darkening
        DrawPostProcessing(gameManager);

        // Widgets
        LootNotifications.Offset = (CameraManager.CameraDest - CameraManager.Camera).ToPoint();
        Gui.Draw(gameManager.Batch);
        DebugManager.EndBenchmark("GuiDraw");

        // Inventory
        DebugManager.StartBenchmark("InventoryDraw");
        inventory.Draw(gameManager);
        DebugManager.EndBenchmark("InventoryDraw");

        // Minimap
        if (StateManager.OverlayState == OverlayState.Inventory)
            DrawMiniMap(device, gameManager);

        // Debug
        gameManager.Batch.DrawPoint(CameraManager.PlayerFoot.ToVector2() - CameraManager.Camera, Constants.DebugGreenTint, 3);
    }
    public void DrawPostProcessing(GameManager gameManager)
    {
        DebugManager.StartBenchmark("PostProcessing");

        // Tint
        if (gameManager.LevelManager.Level != null)
            gameManager.Batch.FillRectangle(new(Vector2.Zero, Constants.Window), gameManager.LevelManager.Level.Tint);

        // Hitboxes
        if (Constants.DRAW_HITBOXES)
        {
            gameManager.Batch.DrawPoint(Constants.Middle.ToVector2(), Constants.DebugPinkTint, 5);
            gameManager.Batch.DrawPoint(Constants.Middle.ToVector2() - new Vector2(0, Constants.MageHalfSize.Y + 12), Constants.DebugPinkTint, 5);
            gameManager.Batch.DrawPoint(Constants.Middle.ToVector2() + CameraManager.CameraOffset, Constants.DebugGreenTint, 5);
        }

        // Death
        if (StateManager.State == GameState.Death)
        {
            if (deathTime == -1) deathTime = gameManager.TotalTime;
            gameManager.Batch.FillRectangle(new Rectangle(Point.Zero, Constants.Window), Color.Black * ((gameManager.TotalTime - deathTime) / 5));
            gameManager.Batch.DrawString(TextureManager.PixelOperator, "YOU DIED!", Constants.Middle.ToVector2() - TextureManager.PixelOperator.MeasureString("You died!") * 2, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
            gameManager.Batch.DrawString(TextureManager.PixelOperator, "Press space to respawn", Constants.Middle.ToVector2() - TextureManager.PixelOperator.MeasureString("Press space to respawn") / 2 + new Vector2(0, 80), Color.White);
        }
        DebugManager.EndBenchmark("PostProcessing");
    }
    public void DrawMiniMap(GraphicsDevice device, GameManager gameManager)
    {
        DebugManager.StartBenchmark("DrawMinimap");
        // Frame
        gameManager.Batch.DrawRectangle(new(7, Constants.Window.Y - Constants.MapSize.Y - 13, Constants.MapSize.X + 6, Constants.MapSize.Y + 6), Color.Black, 3);

        // Create render if not done already
        if (minimap == null)
        {
            // Setup target
            minimap = new RenderTarget2D(device, Constants.MapSize.X, Constants.MapSize.Y);
            device.SetRenderTarget(minimap);
            device.Clear(Color.Transparent);
            gameManager.Batch.End();
            gameManager.Batch.Begin();

            // Pixels
            for (int y = 0; y < Constants.MapSize.Y; y++)
            {
                for (int x = 0; x < Constants.MapSize.X; x++)
                {
                    // Get tile
                    Tile tile = gameManager.LevelManager.GetTile(new Point(x, y))!;
                    gameManager.Batch.DrawPoint(new(x, y), Constants.MiniMapColors[(int)tile.Type]);
                }
            }

            // Resume normal render
            gameManager.Batch.End();
            device.SetRenderTarget(null);
            gameManager.Batch.Begin();

        }
        else
            gameManager.Batch.Draw(minimap, new Rectangle(10, Constants.Window.Y - Constants.MapSize.Y - 10, Constants.MapSize.X, Constants.MapSize.Y), Color.White);

        // Player
        Point dest = CameraManager.TileCoord + new Point(10, Constants.Window.Y - Constants.MapSize.Y - 10);
        gameManager.Batch.DrawPoint(dest.ToVector2(), Color.Red, size: 2);

        DebugManager.EndBenchmark("DrawMinimap");
    }
    public void Notification(string text, Color? color = null, float duration = 4f)
    {
        LootNotifications.AddNotification(text, color, duration);
    }
    public void RefreshMiniMap() { minimap = null; }
}
