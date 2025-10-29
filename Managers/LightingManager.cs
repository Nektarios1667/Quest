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
    public void SetLight(Point pos, int light)
    {
        if (pos.X < 0 || pos.Y < 0 || pos.X >= Grid.GetLength(0) || pos.Y >= Grid.GetLength(1)) return;
        Grid[pos.X, pos.Y].LightLevel = light;
    }
    public void Run()
    {
        Queue<FloodLightingNode> toVisit = new();
        foreach (FloodLightingNode node in Grid)
        {
            if (node.LightLevel > 0)
                toVisit.Enqueue(node);
        }

        while (toVisit.Count > 0)
        {
            var current = toVisit.Dequeue();
            if (current.IsBlocked) continue;

            foreach (Point offset in Constants.AllNeighborTiles)
            {
                Point neighbor = current.Position + offset;
                if (neighbor.X < 0 || neighbor.Y < 0 || neighbor.X >= Width || neighbor.Y >= Height) continue;
                var neighborNode = Grid[neighbor.X, neighbor.Y];

                float newLightLevel = current.LightLevel - ((offset.X == 0 || offset.Y == 0) ? 1f : Constants.SQRT2);
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
    public Color Color { get; }
    public bool SingleFrame { get; init; }
    public RadialLight(Point pos, int size, Color color, bool singleFrame = false)
    {
        Position = pos;
        Size = size;
        Color = color;
        SingleFrame = singleFrame;
    }
}

public static class LightingManager
{
    public static Dictionary<string, RadialLight> Lights { get; private set; } = [];
    public static void Update()
    {
        foreach (var key in Lights.Keys.ToList())
        {
            var light = Lights[key];
            if (light.SingleFrame)
                Lights.Remove(key);
        }
    }
    public static void CreateLight(string name, Point pos, float tileSize, Color color)
    {
        if (Lights.ContainsKey(name)) return;
        Lights[name] = new(pos, (int)(tileSize * Constants.TileSize.X), color);
    }
    public static void SetLight(string name, Point pos, float tileSize, Color color, bool singleFrame = false) => Lights[name] = new(pos, (int)(tileSize * Constants.TileSize.X), color, singleFrame);
    public static RadialLight[] GetVisibleLights() => [.. Lights.Values.Where(LightAffectsScreen)];
    public static void RemoveLight(string name) => Lights.Remove(name);
    public static void ClearLights() => Lights.Clear();
    public static bool LightAffectsScreen(RadialLight light)
    {
        // Light's circular bounds
        float radius = light.Size;
        float left = light.Position.X - radius * Constants.TileSize.X;
        float right = light.Position.X + radius * Constants.TileSize.X;
        float top = light.Position.Y - radius * Constants.TileSize.Y;
        float bottom = light.Position.Y + radius * Constants.TileSize.Y;

        // AABB check
        return right > 0 && left < Constants.Window.X && bottom > 0 && top < Constants.Window.Y;
    }
}
