using System.Linq;

namespace Quest.Managers;

public class FloodLightingGrid
{
    public int Width { get; }
    public int Height { get; }
    public FloodLightingNode[,] Grid { get; }
    private readonly Queue<FloodLightingNode> toVisit = new();
    public FloodLightingGrid(int width, int height, bool[,] blocked)
    {
        Width = width;
        Height = height;

        Grid = new FloodLightingNode[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                Grid[x, y] = new(this, new(x, y), 0, blocked[x, y]);
    }
    public void Reset(Rectangle region)
    {
        // Clamping
        int minX = Math.Clamp(region.X, 0, Width);
        int minY = Math.Clamp(region.Y, 0, Height);
        int maxX = Math.Clamp(region.Right, 0, Width);
        int maxY = Math.Clamp(region.Bottom, 0, Height);

        for (int y = minY; y < maxY; y++)
        {
            for (int x = minX; x < maxX; x++)
            {
                Grid[x, y].LightLevel = 0;
            }
        }
    }
    public void AddLight(Point pos, int light)
    {
        if (pos.X < 0 || pos.Y < 0 || pos.X >= Grid.GetLength(0) || pos.Y >= Grid.GetLength(1)) return;
        if (Grid[pos.X, pos.Y].LightLevel > light) return;

        Grid[pos.X, pos.Y].LightLevel = light;
    }
    public void Run(Rectangle region)
    {
        // Clamping
        int minX = Math.Clamp(region.X, 0, Width);
        int minY = Math.Clamp(region.Y, 0, Height);
        int maxX = Math.Clamp(region.Right, 0, Width);
        int maxY = Math.Clamp(region.Bottom, 0, Height);

        // Queue all lights
        toVisit.Clear();
        for (int y = minY; y < maxY; y++)
        {
            for (int x = minX; x < maxX; x++)
            {
                if (Grid[x, y].LightLevel > 0)
                {
                    toVisit.Enqueue(Grid[x, y]);
                }
            }
        }

        // Solve
        while (toVisit.Count > 0)
        {
            // Get current node
            var current = toVisit.Dequeue();
            DebugManager.IncrStat(Stats.FloodFillCellUpdates);
            if (current.IsBlocked) continue;

            // Spread light to neighbors
            foreach (Point offset in Constants.AllNeighborTiles)
            {
                // Get neighbor
                Point neighbor = current.Position + offset;
                if (neighbor.X < minX || neighbor.Y < minY || neighbor.X >= maxX || neighbor.Y >= maxY) continue;
                var neighborNode = Grid[neighbor.X, neighbor.Y];

                // Calculate new light level
                float newLightLevel = current.LightLevel - ((offset.X == 0 || offset.Y == 0) ? 1f : 1.5f); // 1.5 is an estimate of sqrt(2)
                if (newLightLevel > neighborNode.LightLevel && newLightLevel > 0.05f)
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
    public static bool UpdateLighting { get; private set; } = true;
    public static FloodLightingGrid LightGrid { get; private set; } = null!;
    public static bool[,] BlockedLuxels { get; private set; } = new bool[0, 0];
    public static Color[,] BiomeColors { get; private set; } = new Color[0, 0];
    public static Point LuxelSize { get; private set; } = Point.Zero;
    public static Point LightingStart { get; private set; }
    public static Point LightingEnd { get; private set; }
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
            float intensity = MathF.Exp(i * LightMult / LightMax) - 1;
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
    public static void MarkUpdateLighting() => UpdateLighting = true;
    public static void SetLight(string name, Point tilePos, float radius, bool singleFrame = false) => Lights[name] = new(tilePos, (int)(radius * Constants.TileSize.X), singleFrame);
    public static void RemoveLight(string name) => Lights.Remove(name);
    public static void ClearLights() => Lights.Clear();
    public static void BuildLevelLighting(GameManager gameManager)
    {
        int lightWidth = Constants.MapSize.X * LightDivisions;
        int lightHeight = Constants.MapSize.Y * LightDivisions;

        // Blocked
        if (BlockedLuxels.GetLength(0) != lightWidth || BlockedLuxels.GetLength(1) != lightHeight)
            BlockedLuxels = new bool[lightWidth, lightHeight];

        // Set blocked luxels
        for (int y = 0; y < Constants.MapSize.Y; y++)
        {
            for (int x = 0; x < Constants.MapSize.Y; x++)
            {
                Tile? tile = gameManager.LevelManager.GetTile(x, y);
                bool isBlocked = tile == null || (tile.IsWall && !tile.IsTransparent && !tile.IsWalkable);
                for (int dy = 0; dy < LightDivisions; dy++)
                    for (int dx = 0; dx < LightDivisions; dx++)
                        BlockedLuxels[x * LightDivisions + dx, y * LightDivisions + dy] = isBlocked;
            }
        }

        LightGrid = new(lightWidth, lightHeight, BlockedLuxels);

        // Biome
        if (BiomeColors.GetLength(0) != LightGrid.Width || BiomeColors.GetLength(1) != LightGrid.Height)
            BiomeColors = new Color[LightGrid.Width, LightGrid.Height];

        MarkUpdateLighting();
    }
    public static void RecalculateLighting(GameManager gameManager)
    {
        DebugManager.StartBenchmark("LightingCalculations");

        UpdateLighting = false;

        // Precomputations
        if (LuxelSize.X == 0)
            LuxelSize = Constants.TileSize.Scaled(InvLightDivisions);
        LastLuxel = CameraManager.Camera.ToPoint() / Constants.TileSize.Scaled(InvLightDivisions);
        // Calculate region
        LightingStart = (CameraManager.Camera.ToPoint() - Constants.Middle) / Constants.TileSize + PointTools.Up - Constants.TileDrawPadding;
        LightingEnd = (CameraManager.Camera.ToPoint() + Constants.Middle) / Constants.TileSize - PointTools.Up + Constants.TileDrawPadding;
        Rectangle updateRegion = new(LightingStart.Scaled(LightDivisions), (LightingEnd - LightingStart).Scaled(LightDivisions));

        // Reset
        LightGrid.Reset(updateRegion);
        // Set lights
        foreach (var light in Lights.Values)
        {
            // Check if light is in camera view
            if (light.Position.X < LightingStart.X || light.Position.Y < LightingStart.Y || light.Position.X > LightingEnd.X || light.Position.Y > LightingEnd.Y)
                continue;

            // Set all luxels in the light tile area
            for (int dy = 0; dy < LightDivisions; dy++)
                for (int dx = 0; dx < LightDivisions; dx++)
                    LightGrid.AddLight(light.Position.Scaled(LightDivisions) + new Point(dx, dy), light.Size * LightDivisions / Constants.TileSize.X);
        }

        LightGrid.Run(updateRegion);

        float blend = gameManager.WeatherManager.GetWeatherIntensity(GameManager.GameTime);
        Point start = (LightingStart + Constants.TileDrawPadding);
        Point end = (LightingEnd - Constants.TileDrawPadding + Constants.OnePoint);
        for (int y = start.Y; y < end.Y; y++)
        {
            for (int x = start.X; x < end.X; x++)
            {
                // Biome
                Point worldLoc = new Point(x, y);
                BiomeColors[x, y] = gameManager.WeatherManager.GetWeatherColor(gameManager, worldLoc, blend);
            }
        }

        DebugManager.EndBenchmark("LightingCalculations");
    }
    public static void SetLightGridBlocking(Point tile, bool isBlocked)
    {
        for (int dy = 0; dy < LightDivisions; dy++)
        {
            for (int dx = 0; dx < LightDivisions; dx++)
            {
                BlockedLuxels[tile.X * LightDivisions + dx, tile.Y * LightDivisions + dy] = isBlocked;
                LightGrid.Grid[tile.X * LightDivisions + dx, tile.Y * LightDivisions + dy].IsBlocked = isBlocked;
            }
        }
        MarkUpdateLighting();
    }
}
