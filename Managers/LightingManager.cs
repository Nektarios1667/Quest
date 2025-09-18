using System.Linq;

namespace Quest.Managers;
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
// 0.6 - Light floor loot 

public static class LightingManager
{
    public static Dictionary<string, RadialLight> Lights { get; private set; } = [];
    public static readonly List<Vector3> LightSources = [];
    public static readonly List<Vector4> LightColors = [];
    public static void CreateLight(string name, Point pos, int size, Color color, float importance)
    {
        if (Lights.ContainsKey(name)) return;

        if (Lights.Count >= Constants.MAX_LIGHTS) Logger.Warning($"Lighting has reached max number of lights ({Constants.MAX_LIGHTS}). Lights with lower importance will be skipped.");
        Lights[name] = new(pos, size, color, importance);
        OrderLights();
    }
    public static void SetLight(string name, Point pos, int size, Color color, float importance)
    {
        if (Lights.Count >= Constants.MAX_LIGHTS) Logger.Warning($"Lighting has reached max number of lights ({Constants.MAX_LIGHTS}). Lights with lower importance will be skipped.");
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
        RadialLight[] top = Lights.Values.Where(light => LightAffectsScreen(light)).OrderByDescending(l => l.Importance).Take(Constants.MAX_LIGHTS).ToArray();
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
