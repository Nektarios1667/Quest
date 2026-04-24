using Antlr.Runtime;
using Quest.Gui;
using LM = Quest.Managers.LightingManager;

namespace Quest.Managers;

public class OverlayManager
{
    public Gui.Overlay Gui { get; private set; } // GUI handler
    public NotificationArea LootNotifications { get; private set; } // Loot pickup notifications
    public StatusBar HealthBar { get; private set; }
    public static readonly Point lootStackOffset = new(4, 4);
    private RenderTarget2D? minimap;
    public bool UpdateLighting { get; set; } = true;
    public OverlayManager(PlayerManager? playerManager)
    {
        Gui = new()
        {
            Widgets = [
                HealthBar = new StatusBar(new(10, Constants.NativeResolution.Y - 35), new(300, 25), Color.Green, Color.Red, 100, 100),
                LootNotifications = new NotificationArea(Constants.Middle - new Point(0, Constants.MageHalfSize.Y + 15), 5, PixelOperatorBold)
            ]
        };


        // Trigger lighting updates
        if (playerManager != null)
        {
            playerManager.EquippedSlotChanged += (_) => MarkUpdateLighting();
            playerManager.InventoryUI.OnSlotDrop += (_, _) => MarkUpdateLighting();
            playerManager.InventoryUI.OnSlotItemChange += (_, _) => MarkUpdateLighting();
        }
        TimerManager.SetTimer("LightingUpdate", 1f, MarkUpdateLighting, int.MaxValue);
        CameraManager.TileChange += (_, _) => MarkUpdateLighting();
        CameraManager.CameraMove += (_, newCam) =>
        {
            if (newCam.ToPoint() / Constants.TileSize.Scaled(LM.InvLightDivisions) != LM.LastLuxel)
                MarkUpdateLighting();
        };
    }


    public void Update(GameManager gameManager, PlayerManager? playerManager)
    {
        // Gui
        DebugManager.StartBenchmark("GuiUpdate");
        Gui.Update(gameManager);
        DebugManager.EndBenchmark("GuiUpdate");

        // Respawn
        if (playerManager != null && StateManager.OverlayState == OverlayState.Death && InputManager.KeyPressed(Keys.Space))
            gameManager.Respawn(playerManager);
    }
    public void Draw(GraphicsDevice device, GameManager gameManager, PlayerManager? playerManager)
    {
        if (!StateManager.IsPlayingState) return;

        // Lighting
        if (StateManager.State == GameState.Game)
            DrawLighting(gameManager);

        // Darkening
        DrawPostProcessing(gameManager, playerManager);

        // Widgets
        LootNotifications.Offset = (CameraManager.CameraDest - CameraManager.Camera).ToPoint();
        Gui.Draw(gameManager.Batch);

        // Minimap
        if (StateManager.OverlayState != OverlayState.None)
            DrawMiniMap(device, gameManager);

        // Inventories
        if (playerManager != null)
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
                DrawTexture(gameManager.Batch, item.Texture, InputManager.MousePosition - new Point(20, 20), scale: 2);
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
        if (StateManager.OverlayState == OverlayState.Container || StateManager.OverlayState == OverlayState.Pause || StateManager.OverlayState == OverlayState.Typing)
            gameManager.Batch.FillRectangle(Constants.WindowRect, Color.Black * 0.6f);

        // Death
        if (StateManager.OverlayState == OverlayState.Death)
        {
            gameManager.Batch.FillRectangle(Constants.WindowRect, Color.Black);
            gameManager.Batch.DrawString(PixelOperator, "YOU DIED!", Constants.Middle.ToVector2() - PixelOperator.MeasureString("You died!") * 2, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
            gameManager.Batch.DrawString(PixelOperator, "Press space to respawn", Constants.Middle.ToVector2() - PixelOperator.MeasureString("Press space to respawn") / 2 + new Vector2(0, 80), Color.White);
        } else if (StateManager.OverlayState == OverlayState.Finished)
        {
            TimerManager.NewTimer("FinishedFade", 2, null);
            float fade = TimerManager.GetTimer("FinishedFade").Progress * 0.5f;

            gameManager.Batch.FillRectangle(Constants.WindowRect, Color.Black * fade);
            gameManager.Batch.DrawString(PixelOperator, "LEVEL FINISHED!", Constants.Middle.ToVector2() - PixelOperator.MeasureString("LEVEL FINISHED!") * 2, Color.White * fade, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
            gameManager.Batch.DrawString(PixelOperator, "Press space to close", Constants.Middle.ToVector2() - PixelOperator.MeasureString("Press space to close") / 2 + new Vector2(0, 80), Color.White * fade);

            if (InputManager.KeyPressed(Keys.Space))
                StateManager.OverlayState = OverlayState.None;
        }

        DebugManager.EndBenchmark("PostProcessing");
    }
    public void MarkUpdateLighting() {
        UpdateLighting = true;
    }
    public void DrawLighting(GameManager gameManager)
    {

        DebugManager.StartBenchmark("Lighting");
        if (UpdateLighting)
            LightingManager.RecalculateLighting(gameManager);
        DebugManager.EndBenchmark("Lighting");

        DebugManager.StartBenchmark("DrawLighting");
        // Draw shadows
        for (int y = 0; y < LM.LightGrid.Height; y++)
        {
            for (int x = 0; x < LM.LightGrid.Width; x++)
            {
                // Light
                float light = LM.LightGrid.Grid[x, y].LightLevel;
                int intensityLookup = Math.Clamp((int)Math.Floor(light * LM.LightDivisions), 0, LM.LightMax * LM.LightDivisions);
                float intensity = LM.LightToIntensityCache[intensityLookup];

                // Skip full light
                if (intensity >= 0.99f)
                    continue;

                // Draw
                Rectangle rect = new((new Point(x, y) + LM.LightingStart.Scaled(LM.LightDivisions)) * LM.LuxelSize + Constants.Middle - CameraManager.Camera.ToPoint(), LM.LuxelSize);
                gameManager.Batch.FillRectangle(rect, gameManager.LevelManager.SkyColor * (1 - intensity));

                // Biome
                gameManager.Batch.FillRectangle(rect, LM.BiomeColors[x / LM.LightDivisions, y / LM.LightDivisions] * (1 - intensity));
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
