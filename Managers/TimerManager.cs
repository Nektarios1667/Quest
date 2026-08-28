namespace Quest.Managers;

public class Timer
{
    public float Left { get; set; }
    public int Completions { get; private set; } = 0;
    public bool Paused { get; private set; } = false;

    public readonly int Repetitions;
    public readonly Action? CompleteAction;
    public readonly Action<float>? UpdateAction;
    public readonly float Duration;
    public float Progress => 1 - Left / Duration;

    public bool IsExpired => Left <= 0f && Completions >= Repetitions;
    public event Action? Completed;
    public Timer(float duration, Action? call, int repetitions = 1, Action<float>? updateAction = null)
    {
        Left = duration;
        Repetitions = repetitions;
        CompleteAction = call;
        UpdateAction = updateAction;
        Duration = duration;
    }
    public void Update(GameManager gameManager)
    {
        if (IsExpired) return;

        if (Left > 0)
        {
            Left -= GameManager.DeltaTime;
            UpdateAction?.Invoke(Progress);
        }

        if (Left <= 0f)
        {
            Completions++;
            Completed?.Invoke();
            CompleteAction?.Invoke();
            if (Repetitions > Completions)
                Left = Duration;
        }
    }
    public void Restart() => Left = Duration;
    public void Pause() => Paused = true;
    public void Unpause() => Paused = false;
    public void TogglePause() => Paused = !Paused;
}
public static class TimerManager
{
    private static readonly Dictionary<string, Timer> timers = [];
    private static readonly List<string> expiredTimers = [];
    public static void Update(GameManager gameManager)
    {
        DebugManager.StartBenchmark("TimerUpdates");

        // Update timers - create copy to allow modification while updating
        foreach (var (name, timer) in new Dictionary<string, Timer>(timers))
        {
            timer.Update(gameManager);
            if (timer.IsExpired)
                expiredTimers.Add(name);
        }
        // Clear expired
        foreach (string expired in expiredTimers)
            timers.Remove(expired);
        expiredTimers.Clear();

        DebugManager.EndBenchmark("TimerUpdates");
    }
    public static Timer NewTimer(string name, float duration, Action? completeAction, int repetitions = 1, Action<float>? updateAction = null)
    {
        if (!timers.ContainsKey(name))
            timers[name] = new(duration, completeAction, repetitions, updateAction);
        return timers[name];
    }
    public static Timer SetTimer(string name, float duration, Action? completeAction, int repetitions = 1, Action<float>? updateAction = null)
    {
        timers[name] = new(duration, completeAction, repetitions, updateAction);
        return timers[name];
    }
    public static void Remove(string name)
    {
        if (!timers.Remove(name))
            Logger.Error($"No timer with name '{name}' found", true);
    }
    public static void TryRemove(string name) { timers.Remove(name); }
    public static float TimeLeft(string name)
    {
        if (timers.TryGetValue(name, out var timer))
            return timer.Left;
        Logger.Error($"No timer with name '{name}' found", true);
        return -1;
    }
    public static float? TryTimeLeft(string name)
    {
        if (timers.TryGetValue(name, out var timer))
            return timer.Left;
        return null;
    }
    public static Timer GetTimer(string name)
    {
        if (timers.TryGetValue(name, out var timer))
            return timer;
        Logger.Error($"No timer with name '{name}' found", true);
        return new(-1, null);
    }
    public static Dictionary<string, Timer> GetAllTimers() => timers;

    public static Timer? TryGetTimer(string name)
    {
        if (timers.TryGetValue(name, out var timer))
            return timer;
        return null;
    }
    public static bool IsComplete(string name)
    {
        if (timers.TryGetValue(name, out var timer))
            return timer.Left <= 0;
        Logger.Error($"No timer with name '{name}' found", true);

        // Won't reach here since Logger.Error will exit
        return false;
    }
    public static bool IsCompleteOrMissing(string name)
    {
        if (timers.TryGetValue(name, out var timer))
            return timer.Left <= 0;
        return true;
    }
    public static bool Exists(string name) => timers.ContainsKey(name);
}
