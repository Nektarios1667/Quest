namespace Quest.Managers;
public static class TimerManager
{
    public class Timer(float duration, Action? call, int repetitions = 1)
    {
        public float left = duration;
        public int completions = 0;
        public readonly int repetitions = repetitions;
        public readonly Action? call = call;
        public readonly float duration = duration;
        public bool IsExpired => left <= 0f && completions >= repetitions;
        public event Action? Completed;

        public void Update(GameManager manager)
        {
            left -= manager.DeltaTime;
            if (left <= 0f)
            {
                completions++;
                Completed?.Invoke();
                call?.Invoke();
                if (repetitions > completions)
                    left = duration;
            }
        }
    }
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
            return timer.left;
        throw new KeyNotFoundException($"No timer with name '{name}' found");
    }
    public static float TryTimeLeft(string name)
    {
        if (timers.TryGetValue(name, out var timer))
            return timer.left;
        return 0;
    }
    public static bool IsComplete(string name)
    {
        if (timers.TryGetValue(name, out var timer))
            return timer.left <= 0;
        throw new KeyNotFoundException($"No timer with name '{name}' found");
    }
    public static bool IsCompleteOrMissing(string name)
    {
        if (timers.TryGetValue(name, out var timer))
            return timer.left <= 0;
        return true;
    }
    public static bool Exists(string name) => timers.ContainsKey(name);
}
