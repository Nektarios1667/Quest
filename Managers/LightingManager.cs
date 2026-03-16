using System.Linq;

namespace Quest.Managers;

public class FloodLightingGrid
{
    public int Width { get; }
    public int Height { get; }
    public FloodLightingNode[,] Grid { get; }
    public FloodLightingGrid(int width, int height, bool[,] blocked)
    {
        Width = width;
        Height = height;

        Grid = new FloodLightingNode[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                Grid[x, y] = new(this, new(x, y), 0, blocked[x, y]);
    }
    public void Reset(int light = 0, bool[,]? blocked = null)
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                Grid[x, y].LightLevel = light;
                if (blocked != null)
                    Grid[x, y].IsBlocked = blocked[x, y];
            }
    }
    public void AddLight(Point pos, int light)
    {
        if (pos.X < 0 || pos.Y < 0 || pos.X >= Grid.GetLength(0) || pos.Y >= Grid.GetLength(1)) return;
        if (Grid[pos.X, pos.Y].LightLevel > light) return;

        Grid[pos.X, pos.Y].LightLevel = light;
    }
    public void Run()
    {
        // Queue all lights
        Queue<FloodLightingNode> toVisit = new();
        foreach (FloodLightingNode node in Grid)
        {
            if (node.LightLevel > 0)
                toVisit.Enqueue(node);
        }

        // Solve
        while (toVisit.Count > 0)
        {
            // Get current node
            var current = toVisit.Dequeue();
            if (current.IsBlocked) continue;

            // Spread light to neighbors
            foreach (Point offset in Constants.AllNeighborTiles)
            {
                // Get neighbor
                Point neighbor = current.Position + offset;
                if (neighbor.X < 0 || neighbor.Y < 0 || neighbor.X >= Width || neighbor.Y >= Height) continue;
                var neighborNode = Grid[neighbor.X, neighbor.Y];

                // Calculate new light level
                float newLightLevel = current.LightLevel - ((offset.X == 0 || offset.Y == 0) ? 1f : 1.5f); // 1.5 is an estimate of sqrt(2)
                if (newLightLevel > neighborNode.LightLevel && newLightLevel > 0.05)
                {
                    neighborNode.LightLevel = newLightLevel;
                    toVisit.Enqueue(neighborNode);
                }
            }
        }
    }
}

public class FloodLightingNode(FloodLightingGrid grid, Point pos, int light, bool isBlocked)
{
    public FloodLightingGrid Grid { get; } = grid;
    public Point Position { get; } = pos;
    public float LightLevel { get; set; } = light;
    public bool IsBlocked { get; set; } = isBlocked;
}

public readonly struct RadialLight
{
    public Point Position { get; }
    public int Size { get; }
    public bool SingleFrame { get; init; }
    public RadialLight(Point pos, int size, bool singleFrame = false)
    {
        Position = pos;
        Size = size;
        SingleFrame = singleFrame;
    }
}
public static class LightingManager
{
    // Constants
    public const int LightDivisions = 2;
    public const float InvLightDivisions = 1f / LightDivisions;
    public const int LightMax = 10;
    public const float LightMult = 0.7f;
    // Lighting
    public static FloodLightingGrid LightGrid { get; private set; } = null!;
    public static bool[,] BlockedLuxels { get; private set; } = new bool[0, 0];
    public static Color[,] BiomeColors { get; private set; } = new Color[0, 0];
    public static Point LuxelSize { get; private set; } = Point.Zero;
    public static Point LightingStart { get; private set; }
    public static Point LastLuxel { get; private set; } = Point.Zero;
    // Other
    public static Dictionary<string, RadialLight> Lights { get; private set; } = [];
    public static float[] LightToIntensityCache { get; private set; } = [];
    static LightingManager()
    {
        // Precompute light to intensity mapping
        LightToIntensityCache = new float[LightMax * LightDivisions + 1];
        for (float i = 0; i <= LightMax; i += InvLightDivisions)
        {
            float intensity = MathF.Exp((i * LightMult) / LightMax) - 1;
            intensity = Math.Clamp(intensity, 0f, 1f);
            LightToIntensityCache[(int)Math.Round(i * LightDivisions)] = intensity;
        }
        Logger.System("Precomputed light to intensity mapping.");
    }
    public static void Update()
    {
        foreach (var key in Lights.Keys.ToList())
        {
            var light = Lights[key];
            if (light.SingleFrame)
                Lights.Remove(key);
        }
    }
    public static void SetLight(string name, Point pos, float tileSize, bool singleFrame = false) => Lights[name] = new(pos, (int)(tileSize * Constants.TileSize.X), singleFrame);
    public static void RemoveLight(string name) => Lights.Remove(name);

    public static void RecalculateLighting(GameManager gameManager)
    {
        gameManager.OverlayManager.UpdateLighting = false;

        // Precomputations
        if (LuxelSize.X == 0)
            LuxelSize = Constants.TileSize.Scaled(InvLightDivisions);

        // Flood fill lighting                                                          one tile buffer at top
        LightingStart = (CameraManager.Camera.ToPoint() - Constants.Middle) / Constants.TileSize + PointTools.Up;
        LastLuxel = CameraManager.Camera.ToPoint() / Constants.TileSize.Scaled(InvLightDivisions);
        Point end = (CameraManager.Camera.ToPoint() + Constants.Middle) / Constants.TileSize;
        int tileWidth = end.X - LightingStart.X + 1;
        int tileHeight = end.Y - LightingStart.Y + 3; // Extra row ontop and below for smoothness

        int lightWidth = tileWidth * LightDivisions;
        int lightHeight = tileHeight * LightDivisions;

        // Blocked
        if (BlockedLuxels.GetLength(0) != lightWidth || BlockedLuxels.GetLength(1) != lightHeight)
            BlockedLuxels = new bool[lightWidth, lightHeight];

        // Set blocked luxels
        for (int y = 0; y < tileHeight; y++)
            for (int x = 0; x < tileWidth; x++)
            {
                Tile? tile = gameManager.LevelManager.GetTile(x + LightingStart.X, y + LightingStart.Y);
                bool isBlocked = tile == null || (tile.IsWall && !tile.IsWalkable);
                for (int dy = 0; dy < LightDivisions; dy++)
                    for (int dx = 0; dx < LightDivisions; dx++)
                        BlockedLuxels[x * LightDivisions + dx, y * LightDivisions + dy] = isBlocked;
            }

        // Reset or make new grid
        if (LightGrid == null || LightGrid.Width != lightWidth || LightGrid.Height != lightHeight)
            LightGrid = new(lightWidth, lightHeight, BlockedLuxels);
        else
            LightGrid.Reset(blocked: BlockedLuxels);
        // Set lights
        foreach (var light in Lights.Values)
        {
            Point lightTile = ((light.Position + CameraManager.Camera.ToPoint() - Constants.Middle).ToVector2() / Constants.TileSize.ToVector2()).ToPoint() - LightingStart;
            if (lightTile.X >= 0 && lightTile.Y >= 0 && lightTile.X < LightGrid.Width && lightTile.Y < LightGrid.Height)
            {
                // Set all luxels in the light tile area
                for (int dy = 0; dy < LightDivisions; dy++)
                    for (int dx = 0; dx < LightDivisions; dx++)
                        LightGrid.AddLight(lightTile.Scaled(LightDivisions) + new Point(dx, dy), light.Size * LightDivisions / Constants.TileSize.X);
            }
        }
        LightGrid.Run();

        // Biome
        if (BiomeColors.GetLength(0) != tileWidth || BiomeColors.GetLength(1) != tileHeight)
            BiomeColors = new Color[tileWidth, tileHeight];
        float blend = StateManager.WeatherIntensity(GameManager.GameTime);
        for (int y = 0; y < tileHeight; y++)
        {
            for (int x = 0; x < tileWidth; x++)
            {
                // Biome
                Point worldLoc = (new Point(x, y) + LightingStart) * Constants.TileSize / Constants.TileSize;
                BiomeColors[x, y] = gameManager.LevelManager.GetWeatherColor(gameManager, worldLoc, blend);
            }
        }
    }
}
