using System.Diagnostics;

namespace Quest.Managers;
public static class DebugManager
{
    public static Stopwatch Watch { get; private set; } = new();
    public static Dictionary<string, double> FrameTimes { get; private set; } = [];
    private static readonly Dictionary<string, float> benchmarkTimes = [];
    public static bool CollisionDebug { get; set; } = false;
    public static bool TextInfo { get; set; } = true;
    public static bool FrameInfo { get; set; } = true;
    public static bool LogInfo { get; set; } = true;
    public static bool FrameBar { get; set; } = false;
    public static bool DrawHitboxes { get; set; } = false;
    public static bool ProgramInfo { get; set; } = false;

    public static void Update()
    {
        // Debug
        if (InputManager.BindPressed(InputAction.ToggleCollisionDebug))
        {
            CollisionDebug = !CollisionDebug;
            Logger.System($"CollisionDebug set to: {CollisionDebug}");
        }
        if (InputManager.BindPressed(InputAction.ToggleTextInfo))
        {
            TextInfo = !TextInfo;
            Logger.System($"TextInfo set to: {TextInfo}");
        }
        if (InputManager.BindPressed(InputAction.ToggleFrameInfo))
        {
            FrameInfo = !FrameInfo;
            Logger.System($"FrameInfo set to: {FrameInfo}");
        }
        if (InputManager.BindPressed(InputAction.ToggleLogInfo))
        {
            LogInfo = !LogInfo;
            Logger.System($"LogInfo set to: {LogInfo}");
        }
        if (InputManager.BindPressed(InputAction.ToggleFrameBar))
        {
            FrameBar = !FrameBar;
            Logger.System($"FrameBar set to: {FrameBar}");
        }
        if (InputManager.BindPressed(InputAction.ToggleHitboxes))
        {
            DrawHitboxes = !DrawHitboxes;
            Logger.System($"DrawHitboxes set to: {DrawHitboxes}");
        }
        if (InputManager.BindPressed(InputAction.ToggleProgramInfo))
        {
            ProgramInfo = !ProgramInfo;
            Logger.System($"ProgramInfo set to: {ProgramInfo}");
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
    public static void DrawHitbox(SpriteBatch batch, IEntity entity)
    {
        if (!DrawHitboxes) return;

        Vector2 screnPos = entity.Bounds.Position - CameraManager.Camera + Constants.Middle.ToVector2();
        batch.DrawRectangle(screnPos, entity.Bounds.Size, Constants.DebugGreenTint, thickness: 2);
        batch.DrawPoint(screnPos + entity.Bounds.Size.ToVector2() * 0.5f, Constants.DebugPinkTint, 3);
        batch.DrawPoint(screnPos + entity.Bounds.Size.ToVector2() * new Vector2(0.5f, 1), Constants.DebugPinkTint, 3);
    }
}
