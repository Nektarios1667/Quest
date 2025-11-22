using Quest.Gui;

namespace Quest.Managers;



public class OverlayManager
{
    public Gui.Gui Gui { get; private set; } // GUI handler
    public NotificationArea LootNotifications { get; private set; } // Loot pickup notifications
    public StatusBar HealthBar { get; private set; }
    public static readonly Point lootStackOffset = new(4, 4);
    private float deathTime = -1;

    private RenderTarget2D? minimap;

    // Lighting
    private FloodLightingGrid lightGrid = null!;
    private bool[,] blocked = new bool[0, 0];
    private Color[,] biomeColors = new Color[0, 0];
    private Point start;
    private const int lightDivisions = 2;
    private const float invLightDivisions = 1f / lightDivisions;
    private bool updateLighting = true;
    public OverlayManager(LevelManager levelManager, PlayerManager? playerManager)
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
            playerManager.Inventory.EquippedSlotChanged += (_) => updateLighting = true;
            playerManager.Inventory.ItemDropped += (_) => updateLighting = true;
        }
        TimerManager.SetTimer("LightingUpdate", 1f, () => updateLighting = true, int.MaxValue);
        CameraManager.CameraMove += (_, _) => updateLighting = true;
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
        // Darkening
        DrawPostProcessing(gameManager, playerManager);

        // Lighting
        if (StateManager.State == GameState.Game)
            DrawLighting(gameManager);

        // Widgets
        LootNotifications.Offset = (CameraManager.CameraDest - CameraManager.Camera).ToPoint();
        Gui.Draw(gameManager.Batch);

        // Minimap
        if (StateManager.OverlayState == OverlayState.Container)
            DrawMiniMap(device, gameManager);

        // Inventories
        DebugManager.StartBenchmark("InventoryGuiDraw");
        if (playerManager != null)
        {
            playerManager.OpenedContainer?.Inventory?.Draw(gameManager, playerManager);
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

        DebugManager.EndBenchmark("PostProcessing");
    }
    public void DrawLighting(GameManager gameManager)
    {

        DebugManager.StartBenchmark("Lighting");
        if (updateLighting)
            RecalculateLighting(gameManager);
        DebugManager.EndBenchmark("Lighting");

        DebugManager.StartBenchmark("DrawLighting");
        // Draw shadows
        for (int y = 0; y < lightGrid.Height; y++)
        {
            for (int x = 0; x < lightGrid.Width; x++)
            {
                // Light
                float light = lightGrid.Grid[x, y].LightLevel;
                int intensityLookup = Math.Clamp((int)Math.Floor(light * 2), 0, LightingManager.LightScale * 2);
                float intensity = LightingManager.LightToIntensityCache[intensityLookup];

                // Skip full light
                if (intensity >= 1f)
                    continue;

                // Draw
                Rectangle rect = new((new Point(x, y) + start.Scaled(lightDivisions)) * Constants.TileSize.Scaled(invLightDivisions) + Constants.Middle - CameraManager.Camera.ToPoint(), Constants.TileSize.Scaled(invLightDivisions));
                gameManager.Batch.FillRectangle(rect, gameManager.LevelManager.SkyColor * (1 - intensity));

                // Biome
                gameManager.Batch.FillRectangle(rect, biomeColors[x / lightDivisions, y / lightDivisions] * (1 - intensity));
            }
        }

        DebugManager.EndBenchmark("DrawLighting");
    }
    public void RecalculateLighting(GameManager gameManager)
    {
        updateLighting = false;

        // Flood fill lighting
        start = (CameraManager.Camera.ToPoint() - Constants.Middle) / Constants.TileSize;
        Point end = (CameraManager.Camera.ToPoint() + Constants.Middle) / Constants.TileSize;
        int tileWidth = end.X - start.X + 1;
        int tileHeight = end.Y - start.Y + 1;

        int lightWidth = tileWidth * lightDivisions;
        int lightHeight = tileHeight * lightDivisions;

        // Blocked
        if (blocked.GetLength(0) != lightWidth || blocked.GetLength(1) != lightHeight)
            blocked = new bool[lightWidth, lightHeight];
        for (int y = 0; y < tileHeight; y++)
            for (int x = 0; x < tileWidth; x++)
            {
                Tile? tile = gameManager.LevelManager.GetTile(x + start.X, y + start.Y);
                bool isBlocked = tile == null || (tile.IsWall && !tile.IsWalkable);
                for (int dy = 0; dy < lightDivisions; dy++)
                    for (int dx = 0; dx < lightDivisions; dx++)
                        blocked[x * lightDivisions + dx, y * lightDivisions + dy] = isBlocked;
            }

        // Lighting
        if (lightGrid == null || lightGrid.Width != lightWidth || lightGrid.Height != lightHeight)
            lightGrid = new(lightWidth, lightHeight, blocked);
        else
            lightGrid.Reset(blocked: blocked);
        foreach (var light in LightingManager.Lights.Values)
        {
            Point lightTile = ((light.Position + CameraManager.Camera.ToPoint() - Constants.Middle).ToVector2() / Constants.TileSize.ToVector2()).ToPoint() - start;
            if (lightTile.X >= 0 && lightTile.Y >= 0 && lightTile.X < lightGrid.Width && lightTile.Y < lightGrid.Height)
            {
                for (int dy = 0; dy < lightDivisions; dy++)
                    for (int dx = 0; dx < lightDivisions; dx++)
                        lightGrid.SetLight(lightTile.Scaled(lightDivisions) + new Point(dx, dy), (light.Size * lightDivisions) / Constants.TileSize.X);
            }
        }
        lightGrid.Run();

        // Biome
        if (biomeColors.GetLength(0) != tileWidth || biomeColors.GetLength(1) != tileHeight)
            biomeColors = new Color[tileWidth, tileHeight];
        float blend = StateManager.WeatherIntensity(gameManager.GameTime);
        for (int y = 0; y < tileHeight; y++)
        {
            for (int x = 0; x < tileWidth; x++)
            {
                // Biome
                Point worldLoc = ((new Point(x, y) + start) * Constants.TileSize) / Constants.TileSize;
                biomeColors[x, y] = gameManager.LevelManager.GetWeatherColor(gameManager, worldLoc, blend);
            }
        }
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
                    gameManager.Batch.DrawPoint(new(x, y), Constants.MiniMapColors[(int)tile.Type.ID]);
                }
            }

            // Resume normal render
            gameManager.Batch.End();
            device.SetRenderTarget(null);
            gameManager.Batch.Begin();

        }
        else
            gameManager.Batch.Draw(minimap, new Rectangle(10, Constants.NativeResolution.Y - Constants.MapSize.Y - 10, Constants.MapSize.X, Constants.MapSize.Y), Color.White);

        // Player
        Point dest = CameraManager.TileCoord + new Point(10, Constants.NativeResolution.Y - Constants.MapSize.Y - 10);
        gameManager.Batch.DrawPoint(dest.ToVector2(), Color.Red, size: 2);

        DebugManager.EndBenchmark("DrawMinimap");
    }
    public void Notification(string text, Color? color = null, float duration = 4f)
    {
        LootNotifications.AddNotification(text, color, duration);
    }
    public void RefreshMiniMap() { minimap = null; }
}
