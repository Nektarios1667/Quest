using MonoGame.Extended.Particles.Profiles;
using Quest.Gui;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;

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
                LootNotifications = new NotificationArea(Constants.Middle - new Point(0, Constants.MageHalfSize.Y + 15), 5, PixelOperatorBold)
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
    public void Draw(GraphicsDevice device, GameManager gameManager, PlayerManager? playerManager)
    {
        DebugManager.StartBenchmark("GuiDraw");

        // Darkening
        DrawPostProcessing(gameManager, playerManager);

        // Widgets
        LootNotifications.Offset = (CameraManager.CameraDest - CameraManager.Camera).ToPoint();
        Gui.Draw(gameManager.Batch);
        DebugManager.EndBenchmark("GuiDraw");

        // Minimap
        if (StateManager.OverlayState == OverlayState.Container)
            DrawMiniMap(device, gameManager);

        // Inventories
        DebugManager.StartBenchmark("InventoryGuiDraw");
        if (playerManager != null)
        {
            playerManager.OpenedContainer?.Inventory.Draw(gameManager, playerManager);
            playerManager.Inventory.Draw(gameManager, playerManager);
        }
        DebugManager.EndBenchmark("InventoryGuiDraw");
    }
    public void DrawPostProcessing(GameManager gameManager, PlayerManager? playerManager)
    {
        DebugManager.StartBenchmark("PostProcessing");

        // Hitboxes
        if (DebugManager.DrawHitboxes)
        {
            gameManager.Batch.DrawPoint(Constants.Middle.ToVector2(), Constants.DebugPinkTint, 5);
            gameManager.Batch.DrawPoint(Constants.Middle.ToVector2() - new Vector2(0, Constants.MageHalfSize.Y + 12), Constants.DebugPinkTint, 5);
            gameManager.Batch.DrawPoint(Constants.Middle.ToVector2() + CameraManager.CameraOffset, Constants.DebugGreenTint, 5);
        }

        // Guis
        if (StateManager.OverlayState == OverlayState.Container || StateManager.OverlayState == OverlayState.Pause)
            gameManager.Batch.FillRectangle(Constants.WindowRect, Color.Black * 0.6f);

        // Death
        if (StateManager.State == GameState.Death)
        {
            if (deathTime == -1) deathTime = gameManager.GameTime;
            gameManager.Batch.DrawString(PixelOperator, "YOU DIED!", Constants.Middle.ToVector2() - PixelOperator.MeasureString("You died!") * 2, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
            gameManager.Batch.DrawString(PixelOperator, "Press space to respawn", Constants.Middle.ToVector2() - PixelOperator.MeasureString("Press space to respawn") / 2 + new Vector2(0, 80), Color.White);
        }

        // Dijkstra's
        Point start = (CameraManager.Camera.ToPoint() - Constants.Middle) / Constants.TileSize;
        Point end = (CameraManager.Camera.ToPoint() + Constants.Middle) / Constants.TileSize;

        bool[,] blocked = new bool[end.X - start.X + 1, end.Y - start.Y + 1];
        for (int y = start.Y; y <= end.Y; y++)
            for (int x = start.X; x <= end.X; x++)
            {
                Tile? tile = gameManager.LevelManager.GetTile(x, y);
                blocked[x - start.X, y - start.Y] = tile == null || (tile.IsWall && !tile.IsWalkable);
            }
        DijkstraLightGrid lightGrid = new(end.X - start.X + 1, end.Y - start.Y + 1, blocked);
        foreach (var light in LightingManager.Lights.Values)
        {
            Point lightTile = ((light.Position + CameraManager.Camera.ToPoint() - Constants.Middle) / Constants.TileSize) - start;
            if (lightTile.X >= 0 && lightTile.Y >= 0 && lightTile.X < lightGrid.Width && lightTile.Y < lightGrid.Height)
                lightGrid.SetLightLevel(lightTile, light.Size / Constants.TileSize.X);
        }
        lightGrid.Run();

        for (int y = 0; y < lightGrid.Height; y++)
        {
            for (int x = 0; x < lightGrid.Width; x++)
            {
                float light = lightGrid.GetLightLevel(x, y);
                float intensity = Math.Clamp((float)Math.Sqrt(light / 10), 0, 1);

                gameManager.Batch.FillRectangle(new Rectangle((new Point(x, y) + start) * Constants.TileSize + Constants.Middle - CameraManager.Camera.ToPoint(), Constants.TileSize), gameManager.LevelManager.SkyLight * (1 - intensity));
            }
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
