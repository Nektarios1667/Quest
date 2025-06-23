namespace Quest.Managers;
public class TimerManager
{
    private Dictionary<string, Timer> timers = [];
    private List<string> removals = [];
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

        public bool IsExpired => left <= 0f && completions >= repetitions;
    }
    public Timer NewTimer(string name, float duration, Action? call, int repetitions = 1)
    {
        if (!timers.ContainsKey(name))
            timers[name] = new(duration, call, repetitions);
        return timers[name];
    }
    public Timer SetTimer(string name, float duration, Action? call, int repetitions = 1)
    {
        timers[name] = new(duration, call, repetitions);
        return timers[name];
    }
    public void Remove(string name)
    {
        if (!timers.Remove(name))
            throw new KeyNotFoundException($"No timer with name '{name}' found");
    }
    public void TryRemove(string name) { timers.Remove(name); }
    public float TimeLeft(string name)
    {
        if (timers.TryGetValue(name, out var timer))
            return timer.left;
        throw new KeyNotFoundException($"No timer with name '{name}' found");
    }
    public bool IsComplete(string name)
    {
        if (timers.TryGetValue(name, out var timer))
            return timer.left <= 0;
        throw new KeyNotFoundException($"No timer with name '{name}' found");
    }
    public bool Exists(string name) => timers.ContainsKey(name);
    public void Update(GameManager gameManager)
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
