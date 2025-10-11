using System.Diagnostics;

namespace Quest.Managers;
public static class DebugManager
{
    public static Stopwatch Watch { get; private set; } = new();
    public static Dictionary<string, double> FrameTimes { get; private set; } = [];
    private static Dictionary<string, float> benchmarkTimes = [];
    public static bool CollisionDebug { get; set; } = false;
    public static bool TextInfo { get; set; } = true;
    public static bool FrameInfo { get; set; } = true;
    public static bool LogInfo { get; set; } = false;
    public static bool FrameBar { get; set; } = true;
    public static bool DrawHitboxes { get; set; } = false;

    public static void Update()
    {
        // Debug
        if (InputManager.Hotkey(Keys.LeftControl, Keys.F1))
        {
            CollisionDebug = !CollisionDebug;
            Logger.System($"CollisionDebug set to: {CollisionDebug}");
        }
        if (InputManager.Hotkey(Keys.LeftControl, Keys.F2))
        {
            TextInfo = !TextInfo;
            Logger.System($"TextInfo set to: {TextInfo}");
        }
        if (InputManager.Hotkey(Keys.LeftControl, Keys.F3))
        {
            FrameInfo = !FrameInfo;
            Logger.System($"FrameInfo set to: {FrameInfo}");
        }
        if (InputManager.Hotkey(Keys.LeftControl, Keys.F4))
        {
            LogInfo = !LogInfo;
            Logger.System($"LogInfo set to: {LogInfo}");
        }
        if (InputManager.Hotkey(Keys.LeftControl, Keys.F5))
        {
            FrameBar = !FrameBar;
            Logger.System($"FrameBar set to: {FrameBar}");
        }
        if (InputManager.Hotkey(Keys.LeftControl, Keys.F6))
        {
            DrawHitboxes = !DrawHitboxes;
            Logger.System($"DrawHitboxes set to: {DrawHitboxes}");
        }
    }
    public static void StartBenchmark(string name)
    {
        benchmarkTimes[name] = (float)Watch.Elapsed.TotalMilliseconds;
    }
    public static void EndBenchmark(string name)
    {
        if (benchmarkTimes.TryGetValue(name, out float value))
        {
            float elapsed = (float)(Watch.Elapsed.TotalMilliseconds - value);
            FrameTimes[name] = elapsed;
        }
        else
            Logger.Error($"Benchmark '{name}' not started.");
    }
}
