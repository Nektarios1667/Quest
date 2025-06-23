using System.Diagnostics;

namespace Quest.Managers;
public static class DebugManager
{
    public static Stopwatch Watch { get; private set; } = new();
    public static Dictionary<string, double> FrameTimes { get; private set; } = [];
    private static Dictionary<string, float> benchmarkTimes = [];
    public static void Update()
    {
        // Debug
        if (InputManager.KeyPressed(Keys.F1))
        {
            Constants.COLLISION_DEBUG = !Constants.COLLISION_DEBUG;
            Logger.System($"COLLISION_DEBUG set to: {Constants.COLLISION_DEBUG}");
        }
        if (InputManager.KeyPressed(Keys.F2))
        {
            Constants.TEXT_INFO = !Constants.TEXT_INFO;
            Logger.System($"TEXT_INFO set to: {Constants.TEXT_INFO}");
        }
        if (InputManager.KeyPressed(Keys.F3))
        {
            Constants.FRAME_INFO = !Constants.FRAME_INFO;
            Logger.System($"FRAME_INFO set to: {Constants.FRAME_INFO}");
        }
        if (InputManager.KeyPressed(Keys.F4))
        {
            Constants.LOG_INFO = !Constants.LOG_INFO;
            Logger.System($"LOG_INFO set to: {Constants.LOG_INFO}");
        }
        if (InputManager.KeyPressed(Keys.F5))
        {
            Constants.FRAME_BAR = !Constants.FRAME_BAR;
            Logger.System($"FRAME_BAR set to: {Constants.FRAME_BAR}");
        }
        if (InputManager.KeyPressed(Keys.F6))
        {
            Constants.DRAW_HITBOXES = !Constants.DRAW_HITBOXES;
            Logger.System($"DRAW_HITBOXES set to: {Constants.DRAW_HITBOXES}");
        }
    }
    public static void StartBenchmark(string name)
    {
        benchmarkTimes[name] = (float)Watch.Elapsed.TotalMilliseconds;
    }
    public static void EndBenchmark(string name)
    {
        if (benchmarkTimes.ContainsKey(name))
        {
            float elapsed = (float)(Watch.Elapsed.TotalMilliseconds - benchmarkTimes[name]);
            FrameTimes[name] = elapsed;
        }
        else
            Logger.Error($"Benchmark '{name}' not started.");
    }
}
