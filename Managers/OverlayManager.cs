using Quest.Gui;
using Quest.World;
using LM = Quest.Managers.LightingManager;

namespace Quest.Managers;

public class OverlayManager
{
    public Gui.Overlay Gui { get; private set; } // GUI handler
    public NotificationArea LootNotifications { get; private set; } // Loot pickup notifications
    public StatusBar HealthBar { get; private set; }
    public Dialog WorldInfobox { get; private set; }
    public Dialog ItemInfobox { get; private set; }
    public static readonly Point lootStackOffset = new(4, 4);
    private RenderTarget2D? minimap;
    public OverlayManager(PlayerManager playerManager)
    {
        Gui = new();
        Gui.Widgets = [
            HealthBar = new StatusBar(new(10, Constants.NativeResolution.Y - 35), new(300, 25), Color.Green * 0.7f, Color.Red * 0.7f, 100, 100),
            LootNotifications = new NotificationArea(Constants.Middle - new Point(0, Constants.MageHalfSize.Y + 15), 5, PixelOperatorBold),
            WorldInfobox = new Dialog(Gui, null, new(1200, 200), new Color(100, 100, 100) * 0.5f, Color.White, "", PixelOperator, borderColor: new Color(40, 40, 40) * 0.5f) { IsVisible = false },
            ItemInfobox = new Dialog(Gui, new(Constants.NativeResolution.X - 370, Constants.NativeResolution.Y - 200), new(350, 200), new Color(100, 100, 100) * 0.5f, Color.White, "", PixelOperator, borderColor: new Color(40, 40, 40) * 0.5f) { IsVisible = false }
        ];

        // Trigger lighting updates
        playerManager.EquippedSlotChanged += (_) => LM.MarkUpdateLighting();
        playerManager.InventoryUI.OnSlotDrop += (_, _) => LM.MarkUpdateLighting();
        playerManager.InventoryUI.OnSlotItemChange += (_, _) => LM.MarkUpdateLighting();

        TimerManager.SetTimer("LightingUpdate", 0.5f, LM.MarkUpdateLighting, int.MaxValue);
        CameraManager.TileChange += (_, _) => LM.MarkUpdateLighting();
        CameraManager.CameraMove += (_, newCam) =>
        {
            if (newCam.ToPoint() / Constants.TileSize.Scaled(LM.InvLightDivisions) != LM.LastLuxel)
                LM.MarkUpdateLighting();
        };
    }
    public void ToggleWorldInfobox(WorldMetadata metadata)
    {
        if (WorldInfobox.IsVisible)
            WorldInfobox.IsVisible = false;
        else
        {
            WorldInfobox.IsVisible = true;
            WorldInfobox.SetText($"Author: {metadata.Author}\nDescription: {metadata.Description}", respeak: DialogRespeak.Instant);
        }
    }
    public void Update(GameManager gameManager, PlayerManager? playerManager)
    {
        if (gameManager.StateManager.State != GameState.Game) return;

        // Set item infobox
        if (playerManager?.HoveredItem != null)
        {
            ItemInfobox.IsVisible = true;
            Item item = playerManager.HoveredItem;
            ItemInfobox.SetText($"--- {item.Name} ---\n{item.Description}", respeak: DialogRespeak.Auto);
        }
        else
            ItemInfobox.IsVisible = false;

        // Gui
        DebugManager.StartBenchmark("GuiUpdate");
        Gui.Update(gameManager);
        DebugManager.EndBenchmark("GuiUpdate");

        // Respawn
        if (playerManager != null && gameManager.StateManager.OverlayState == OverlayState.Death && InputManager.KeyPressed(Keys.Space))
            _ = gameManager.Respawn(playerManager);
        // Exit finished
        if (gameManager.StateManager.OverlayState == OverlayState.Finished && InputManager.KeyPressed(Keys.Space))
            gameManager.StateManager.OverlayState = OverlayState.None;
    }
    public void Draw(GraphicsDevice device, GameManager gameManager, PlayerManager? playerManager)
    {
        if (gameManager.StateManager.State != GameState.Game) return;

        // Lighting
        DrawLighting(gameManager);

        // Darkening
        DrawPostProcessing(gameManager, playerManager);

        // Widgets
        if (gameManager.StateManager.OverlayState != OverlayState.Death)
        {
            LootNotifications.Offset = (CameraManager.CameraDest - CameraManager.Camera).ToPoint();
            Gui.Draw(gameManager.Batch);

            // Minimap
            if (gameManager.StateManager.OverlayState != OverlayState.None)
                DrawMiniMap(device, gameManager);
        }

        // Inventories
        if (playerManager != null && playerManager.IsAlive)
            DrawUI(gameManager, playerManager);
    }
    public void DrawUI(GameManager gameManager, PlayerManager playerManager)
    {
        DebugManager.StartBenchmark("InventoryGuiDraw");

        // Draw interfaces
        playerManager.OpenedInterface?.Draw();
        playerManager.InventoryUI.Draw(playerManager.InventoryOpen ? null : "hotbar");

        // Draw gui mouse item
        if (playerManager.InventoryOpen && playerManager.MouseSelection != null)
        {
            Item? item = playerManager.MouseSelection.Value.ui.BoundContainer?.Items[playerManager.MouseSelection.Value.idx];
            if (item != null)
                DrawTexture(gameManager.Batch, item.Texture, InputManager.MousePosition - new Point(20, 20), scale: new(2));
        }

        // Draw hover label
        if (playerManager.InventoryOpen && playerManager.HoveredItem != null)
        {

            string display = StringTools.FillCamelSpaces(playerManager.HoveredItem.Name);
            Point textSize = PixelOperator.MeasureString(display).ToPoint();
            Vector2 labelPos = InputManager.MousePosition.ToVector2() - new Vector2(0, 17);
            FillRectangle(gameManager.Batch, labelPos.ToPoint() + new Point(4, -8), new Point(textSize.X + 4, 30), Color.Black * 0.7f);
            gameManager.Batch.DrawRectangle(labelPos + new Vector2(2, -10), new Vector2(textSize.X + 8, 34), Color.Blue * 0.7f, 2);
            gameManager.Batch.DrawString(PixelOperator, display, labelPos + new Vector2(8, -8), playerManager.HoveredItem.CustomName == null ? Color.White : Color.Cyan);
        }

        DebugManager.EndBenchmark("InventoryGuiDraw");
    }
    public void DrawPostProcessing(GameManager gameManager, PlayerManager? playerManager)
    {
        DebugManager.StartBenchmark("PostProcessing");

        // Hitboxes
        if (DebugManager.DrawHitboxes)
        {
            // 9 points on the screen
            gameManager.Batch.DrawPoint(Vector2.Zero, Constants.DebugBlueTint, 10);
            gameManager.Batch.DrawPoint(new(Constants.Middle.X, 0), Constants.DebugBlueTint, 10);
            gameManager.Batch.DrawPoint(new(Constants.NativeResolution.X, 0), Constants.DebugBlueTint, 10);
            gameManager.Batch.DrawPoint(new(0, Constants.Middle.Y), Constants.DebugBlueTint, 10);
            gameManager.Batch.DrawPoint(Constants.Middle.ToVector2(), Constants.DebugBlueTint, 10);
            gameManager.Batch.DrawPoint(new(Constants.NativeResolution.X, Constants.Middle.Y), Constants.DebugBlueTint, 10);
            gameManager.Batch.DrawPoint(new(0, Constants.NativeResolution.Y), Constants.DebugBlueTint, 10);
            gameManager.Batch.DrawPoint(new(Constants.Middle.X, Constants.NativeResolution.Y), Constants.DebugBlueTint, 10);
            gameManager.Batch.DrawPoint(Constants.NativeResolution.ToVector2(), Constants.DebugBlueTint, 10);
        }

        // Guis
        if (gameManager.StateManager.OverlayState == OverlayState.Container || gameManager.StateManager.OverlayState == OverlayState.Pause || gameManager.StateManager.OverlayState == OverlayState.GUI)
            gameManager.Batch.FillRectangle(Constants.WindowRect, Color.Black * 0.6f);

        // Fading - for general purpose
        if (TimerManager.Exists("ScreenFadeOut"))
            gameManager.Batch.FillRectangle(Constants.WindowRect, Color.Black * TimerManager.GetTimer("ScreenFadeOut").Progress);
        if (TimerManager.Exists("ScreenFadeIn"))
            gameManager.Batch.FillRectangle(Constants.WindowRect, Color.Black * (1 - TimerManager.GetTimer("ScreenFadeIn").Progress));

        // Death
        if (gameManager.StateManager.OverlayState == OverlayState.Death)
            DrawBlackScreen(gameManager, "YOU DIED!", "Press space to respawn");
        // Finished
        else if (gameManager.StateManager.OverlayState == OverlayState.Finished)
            DrawBlackScreen(gameManager, "LEVEL FINISHED!", "Press space to close");

        DebugManager.EndBenchmark("PostProcessing");
    }
    private static void DrawBlackScreen(GameManager gameManager, string title, string message)
    {
        Timer? timer = TimerManager.TryGetTimer("ScreenFadeOut");
        float fade = timer != null ? timer.Progress : 1;
        if (timer == null)
            gameManager.Batch.FillRectangle(Constants.WindowRect, Color.Black);

        gameManager.Batch.DrawString(PixelOperator, "YOU DIED!", Constants.Middle.ToVector2() - PixelOperator.MeasureString("You died!") * 2, Color.White * fade, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
        gameManager.Batch.DrawString(PixelOperator, "Press space to respawn", Constants.Middle.ToVector2() - PixelOperator.MeasureString("Press space to respawn") / 2 + new Vector2(0, 80), Color.White * fade);
    }
    public void DrawLighting(GameManager gameManager)
    {

        if (LM.UpdateLighting)
            LightingManager.RecalculateLighting(gameManager);

        DebugManager.StartBenchmark("DrawLighting");
        // Draw lighting - do not draw the offscreen lighting
        Point start = (LM.LightingStart + Constants.TileDrawPadding).Scaled(LM.LightDivisions);
        Point end = (LM.LightingEnd - Constants.TileDrawPadding + Constants.OnePoint).Scaled(LM.LightDivisions);
        int startX = Math.Max(0, start.X);
        int startY = Math.Max(0, start.Y);
        int endX = Math.Min(LM.LightGrid.Grid.GetLength(0), end.X);
        int endY = Math.Min(LM.LightGrid.Grid.GetLength(1), end.Y);

        for (int y = startY; y < endY; y++)
        {
            for (int x = startX; x < endX; x++)
            {
                // Light
                float light = LM.LightGrid.Grid[x, y].LightLevel;
                int intensityLookup = Math.Clamp((int)(light * LM.LightDivisions), 0, LM.LightMax * LM.LightDivisions);
                float intensity = LM.LightToIntensityCache[intensityLookup];

                // Skip full light
                if (intensity >= 0.98f)
                    continue;

                // Draw
                Rectangle rect = new(new Point(x, y) * LM.LuxelSize + Constants.Middle - CameraManager.Camera.ToPoint(), LM.LuxelSize);
                Color sky = gameManager.LevelManager.SkyColor * (1 - intensity);
                Color weather = LM.BiomeColors[x / LM.LightDivisions, y / LM.LightDivisions] * (1 - intensity);
                Color color = ColorTools.Blend(weather, sky, 0.5f * sky.A / 255, AlphaBlend.Max);

                gameManager.Batch.FillRectangle(rect, color);
            }
        }

        DebugManager.EndBenchmark("DrawLighting");
    }

    public void DrawMiniMap(GraphicsDevice device, GameManager gameManager)
    {
        DebugManager.StartBenchmark("DrawMinimap");
        // Frame
        gameManager.Batch.DrawRectangle(new(7, Constants.NativeResolution.Y - Constants.MapSize.Y - 13, Constants.MapSize.X + 6, Constants.MapSize.Y + 6), Color.Black, 3);

        // Create render if not done already
        if (minimap == null)
        {
            // Setup target
            minimap = new RenderTarget2D(device, Constants.MapSize.X, Constants.MapSize.Y);
            gameManager.Batch.End();
            device.SetRenderTarget(minimap);
            device.Clear(Color.Transparent);
            gameManager.MinimapBatch.Begin();

            // Pixels
            for (int y = 0; y < Constants.MapSize.Y; y++)
            {
                for (int x = 0; x < Constants.MapSize.X; x++)
                {
                    // Get tile
                    Tile tile = gameManager.LevelManager.GetTile(new Point(x, y))!;
                    gameManager.MinimapBatch.DrawPoint(new(x, y), Constants.MiniMapColors[(int)tile.Type.ID]);
                }
            }

            // Resume normal render
            gameManager.MinimapBatch.End();
            device.SetRenderTarget(null);
            gameManager.Batch.Begin();
        }
        gameManager.Batch.Draw(minimap, new Rectangle(10, Constants.NativeResolution.Y - Constants.MapSize.Y - 10, Constants.MapSize.X, Constants.MapSize.Y), Color.White);

        // Player
        Point dest = CameraManager.TileCoord + new Point(10, Constants.NativeResolution.Y - Constants.MapSize.Y - 10);
        gameManager.Batch.DrawPoint(dest.ToVector2(), Color.Red, size: 2);

        DebugManager.EndBenchmark("DrawMinimap");
    }
    public void Notification(string text, Color? color = null, float duration = 5f)
    {
        LootNotifications.AddNotification(text, color, duration);
    }
    public void RefreshMiniMap() { minimap = null; }
}
