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
    public void SetLightLevel(Point pos, int light)
    {
        if (pos.X < 0 || pos.Y < 0 || pos.X >= Grid.GetLength(0) || pos.Y >= Grid.GetLength(1)) return;
        Grid[pos.X, pos.Y] = new(this, pos, light, false);
    }
    public void Run()
    {
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                Grid[x, y].IsVisited = false;
                if (Grid[x, y].LightLevel > 0)
                    Grid[x, y].UpdateLight();
            }
    }
    public float GetLightLevel(Point pos)
    {
        if (pos.X < 0 || pos.Y < 0 || pos.X >= Grid.GetLength(0) || pos.Y >= Grid.GetLength(1)) return 0;
        return Grid[pos.X, pos.Y].LightLevel;
    }
    public float GetLightLevel(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Grid.GetLength(0) || y >= Grid.GetLength(1)) return 0;
        return Grid[x, y].LightLevel;
    }
}

public class FloodLightingNode(FloodLightingGrid grid, Point pos, int light, bool isBlocked)
{
    public FloodLightingGrid Grid { get; } = grid;
    public Point Position { get; } = pos;
    public float LightLevel { get; set; } = light;
    public bool IsVisited { get; set; } = false;
    public bool IsBlocked { get; } = isBlocked;
    public void UpdateLight(float light = -1)
    {
        LightLevel = Math.Max(LightLevel, light);
        IsVisited = true;

        if (IsBlocked) return;

        foreach (Point offset in Constants.AllNeighborTiles)
        {
            Point neighborPos = Position + offset;
            if (neighborPos.X < 0 || neighborPos.Y < 0 || neighborPos.X >= Grid.Grid.GetLength(0) || neighborPos.Y >= Grid.Grid.GetLength(1)) continue;
            
            FloodLightingNode neighbor = Grid.Grid[neighborPos.X, neighborPos.Y];
            if (neighbor.IsVisited && neighbor.LightLevel >= LightLevel - 1) continue;
            float decay = (offset.X == 0 || offset.Y == 0) ? 1 : Constants.SQRT2;
            float newLight = LightLevel - decay;
            neighbor.UpdateLight(newLight);
        }

    }
}
public readonly struct RadialLight
{
    public Point Position { get; }
    public int Size { get; }
    public Vector3 ShaderLightSource { get; }
    public Color Color { get; }
    public float Importance { get; }
    public RadialLight(Point pos, int size, Color color, float importance)
    {
        Position = pos;
        Size = size;
        Color = color;
        Importance = importance;
        ShaderLightSource = new(Position.ToVector2(), Size);
    }
}

// Current importance values:
// 0.8 - Torch decals
// 0.7 - Player handheld light
// 0.65 - Tile lights
// 0.6 - Light floor loot 

public static class LightingManager
{
    const int MAX_LIGHTS = 50; // New lighting system does not have limit, but shader has a limit
    public static Dictionary<string, RadialLight> Lights { get; private set; } = [];
    public static readonly List<Vector3> LightSources = [];
    public static readonly List<Vector4> LightColors = [];
    public static void CreateLight(string name, Point pos, int size, Color color, float importance)
    {
        if (Lights.ContainsKey(name)) return;

        if (Lights.Count >= MAX_LIGHTS) Logger.Warning($"Lighting has reached max number of lights ({MAX_LIGHTS}). Lights with lower importance will be skipped.");
        Lights[name] = new(pos, size, color, importance);
        OrderLights();
    }
    public static void SetLight(string name, Point pos, int size, Color color, float importance)
    {
        if (Lights.Count >= MAX_LIGHTS && !Lights.ContainsKey(name))
            Logger.Warning($"Lighting has reached max number of lights ({MAX_LIGHTS}). Lights with lower importance will be skipped.");
        Lights[name] = new(pos, size, color, importance);
        OrderLights();
    }
    public static void RemoveLight(string name)
    {
        Lights.Remove(name);
        OrderLights();
    }
    public static void ClearLights()
    {
        Lights.Clear();
        OrderLights();
    }
    public static void OrderLights()
    {
        LightColors.Clear();
        LightSources.Clear();
        RadialLight[] top = Lights.Values.Where(light => LightAffectsScreen(light)).OrderByDescending(l => l.Importance).Take(MAX_LIGHTS).ToArray();
        for (int r = 0; r < top.Length; r++)
        {
            LightSources.Add(top[r].ShaderLightSource);
            LightColors.Add(top[r].Color.ToVector4());
        }
    }
    public static bool LightAffectsScreen(RadialLight light)
    {
        // Light's circular bounds
        float radius = light.Size;
        float left = light.Position.X - radius;
        float right = light.Position.X + radius;
        float top = light.Position.Y - radius;
        float bottom = light.Position.Y + radius;

        // AABB check
        return right > 0 && left < Constants.Window.X && bottom > 0 && top < Constants.Window.Y;
    }
}
