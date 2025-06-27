namespace Quest.Managers;
public static class TimerManager
{
    private static Dictionary<string, Timer> timers = [];
    private static List<string> removals = [];
    public class Timer
    {
        public float left;
        public int completions;
        public readonly int repetitions;
        public readonly Action? call;
        public readonly float duration;

        public event Action? Completed;

        public Timer(float duration, Action? call, int repetitions = 1)
        {
            this.duration = duration;
            left = duration;
            this.repetitions = repetitions;
            completions = 0;
            this.call = call;
        }
        public void Update(GameManager manager)
        {
            DebugManager.StartBenchmark("TimerUpdates");

            left -= manager.DeltaTime;
            if (left <= 0f)
            {
                completions++;
                Completed?.Invoke();
                call?.Invoke();
                if (repetitions > completions)
                    left = duration;
            }

            DebugManager.EndBenchmark("TimerUpdates");
        }

        public bool IsExpired => left <= 0f && completions >= repetitions;
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
    public static bool TryIsComplete(string name)
    {
        if (timers.TryGetValue(name, out var timer))
            return timer.left <= 0;
        return false;
    }
    public static bool Exists(string name) => timers.ContainsKey(name);
    public static void Update(GameManager gameManager)
    {
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
    }
}
