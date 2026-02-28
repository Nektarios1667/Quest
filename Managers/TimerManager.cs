namespace Quest.Managers;
public class Timer(float duration, Action? call, int repetitions = 1)
{
    public float Left = duration;
    public int Completions = 0;
    public bool Paused = false;

    public readonly int Repetitions = repetitions;
    public readonly Action? Call = call;
    public readonly float Duration = duration;

    public bool IsExpired => Left <= 0f && Completions >= Repetitions;
    public event Action? Completed;

    public void Update(GameManager manager)
    {
        Left -= manager.DeltaTime;
        if (Left <= 0f)
        {
            Completions++;
            Completed?.Invoke();
            Call?.Invoke();
            if (Repetitions > Completions)
                Left = Duration;
        }
    }
    public void Pause() => Paused = true;
    public void Unpause() => Paused = false;
    public void TogglePause() => Paused = !Paused;
}
public static class TimerManager
{
    private static readonly Dictionary<string, Timer> timers = [];
    private static readonly List<string> removals = [];
    public static void Update(GameManager gameManager)
    {
        DebugManager.StartBenchmark("TimerUpdates");

        // Update timers
        foreach (var (key, timer) in timers)
        {
            timer.Update(gameManager);
            if (timer.IsExpired)
                removals.Add(key);
        }

        // Remove expired timers
        foreach (string timer in removals)
            timers.Remove(timer);
        removals.Clear();

        DebugManager.EndBenchmark("TimerUpdates");
    }
    public static Timer NewTimer(string name, float duration, Action? call, int repetitions = 1)
    {
        if (!timers.ContainsKey(name))
            timers[name] = new(duration, call, repetitions);
        return timers[name];
    }
    public static Timer SetTimer(string name, float duration, Action? call, int repetitions = 1)
    {
        timers[name] = new(duration, call, repetitions);
        return timers[name];
    }
    public static void Remove(string name)
    {
        if (!timers.Remove(name))
            throw new KeyNotFoundException($"No timer with name '{name}' found");
    }
    public static void TryRemove(string name) { timers.Remove(name); }
    public static float TimeLeft(string name)
    {
        if (timers.TryGetValue(name, out var timer))
            return timer.Left;
        throw new KeyNotFoundException($"No timer with name '{name}' found");
    }
    public static float TryTimeLeft(string name)
    {
        if (timers.TryGetValue(name, out var timer))
            return timer.Left;
        return 0;
    }
    public static bool IsComplete(string name)
    {
        if (timers.TryGetValue(name, out var timer))
            return timer.Left <= 0;
        throw new KeyNotFoundException($"No timer with name '{name}' found");
    }
    public static bool IsCompleteOrMissing(string name)
    {
        if (timers.TryGetValue(name, out var timer))
            return timer.Left <= 0;
        return true;
    }
    public static bool Exists(string name) => timers.ContainsKey(name);
}
