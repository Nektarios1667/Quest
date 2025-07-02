using System.Linq;

namespace Quest.Managers;


public static class CommandManager
{
    // Sub classes
    private class Command(string pattern, Func<string, bool> action, string success, string failure)
    {
        public string Name { get; private set; } = pattern.Split(' ')[0];
        public string Pattern { get; private set; } = pattern;
        public Func<string, bool> Action { get; private set; } = action;
        public string Success { get; private set; } = success;
        public string Failure { get; private set; } = failure;

        public bool IsCommand(string command) => command.Split(' ')[0] == Name;
        public bool Matches(string command) { return CommandManager.CommandCheck(command, Pattern); }
        public string TryExecute(string command)
        {
            // Setup
            string[] parts = command.Split(' ');
            for (int p = 0; p < parts.Length; p++)
            {
                Success = Success.Replace($"|{p}|", parts[p]);
            }
            for (int p = 0; p < parts.Length; p++)
            {
                Failure = Failure.Replace($"|{p}|", parts[p]);
            }

            // Checks and run
            if (Matches(command))
            {
                if (Action(command)) { return Success; }
                return Failure;
            }
            else { return $"Command should be formatted as '{Pattern}'"; }
        }
    }
    private class CommandResponse(string message, bool success)
    {
        public string Message { get; private set; } = message;
        public bool Success { get; private set; } = success;
    }

    private static List<Command> commands { get; set; } = [];
    private static Game? game { get; set; }
    private static GameManager? gameManager { get; set; }
    private static LevelManager? levelManager { get; set; }
    private static PlayerManager? playerManager { get; set; }
    // Type dict
    private readonly static Dictionary<string, Func<string, bool>> predicateMap = new()
    {
        { "int", value => int.TryParse(value, out _)  },
        { "decimal", value => decimal.TryParse(value, out _) && !decimal.TryParse(value, out _) },
        { "number", value => decimal.TryParse(value, out _) }, // Any numeric value
        { "string", value => true },
        { "bool", value => bool.TryParse(value, out _) },
        { "coordinate", value => IsCoordinate(value) },
        { "modify", value => value == "set" || value == "change" }
    };
    public static void Init(Game game, GameManager gameManager, LevelManager levelManager, PlayerManager playerManager)
    {
        CommandManager.game = game;
        CommandManager.gameManager = gameManager;
        CommandManager.levelManager = levelManager;
        CommandManager.playerManager = playerManager;

        // Commands creation
        commands = [
            new("teleport <coordinate>", CTeleport, "Teleported player to |1|.", "Failed to teleport player to |1|."),
            new("health <modify> {0:999}", CHealth, "Player health |1| [|2|].", "Failed to |1| player health [|2|]."),
            new("move_speed {0:999}", CMoveSpeed, "Set player speed to |1|.", "Failed to set player speed to |1|."),
            new("force_quit", CForceQuit, "Force quit application.", "Failed to force quit application."),
            new("quit", CQuit, "Quit application.", "Failed to quit application."),
            new("location", CLocation, "$noout", "Failed to get player location."),
            new("level [load|read] <string>", CLevel, "Ran |1| level '|2|'.", "Failed to |1| level '|2|'."),
            new("mood [calm|dark|epic]", CMood, "Set mood to '|1|'.", "Failed to set mood to '|1|.'"),
            new("say **", CSay, "$noout", "Failed to speak."),
            new($"daytime <modify> {{-{Constants.DayLength}:{Constants.DayLength}}}", CDaytime, "Daytime |1| |2|", "Failed |1| daytime |2|"),
        ];
    }
    // Custom type predicates
    static bool IsCoordinate(string val)
    {
        string[] parts = val.Split(',');
        if (parts.Length == 2)
        {
            return float.TryParse(parts[0], out _) && float.TryParse(parts[1], out _);
        }
        return false;
    }

    private static bool CommandCheck(string command, string pattern)
    {
        // Setup
        string[] parts = command.Split(' ');
        string[] args = pattern.Split(' ')[1..];
        int p = 0;

        // Command check
        if (parts[0] != pattern.Split(' ')[0]) { return false; }

        // Checks
        foreach (string part in parts[1..])
        {
            // Too many args
            if (args.Length - 1 < p) { return false; }
            // Builtin predicate type
            else if (args[p].StartsWith('<') && args[p].EndsWith('>'))
            {
                if (!predicateMap.TryGetValue(args[p][1..^1], out var predicate) || !predicate(part)) { return false; }
            }
            // Selection
            else if (args[p].StartsWith('[') && args[p].EndsWith(']'))
            {
                if (!args[p][1..^1].Split('|').Contains(part)) { return false; }
            }
            // Exact
            else if (args[p].StartsWith('"') && args[p].EndsWith('"'))
            {
                if (part != args[p][1..^1]) { return false; }
            }
            // Any
            else if (args[p] == "*") { }
            // Float range
            else if (args[p].StartsWith("f{") && args[p].EndsWith('}'))
            {
                string[] range = args[p][2..^1].Split(':');
                if (range.Length != 2) { return false; }
                if (!float.TryParse(range[0], out float min)) { return false; }
                if (!float.TryParse(range[1], out float max)) { return false; }
                if (!float.TryParse(part, out float num)) { return false; }
                if (num < min || num > max) { return false; }
            }
            // Integer range
            else if (args[p].StartsWith('{') && args[p].EndsWith('}'))
            {
                string[] range = args[p][1..^1].Split(':');
                if (range.Length != 2) { return false; }
                if (!int.TryParse(range[0], out int min)) { return false; }
                if (!int.TryParse(range[1], out int max)) { return false; }
                if (!int.TryParse(part, out int num)) { return false; }
                if (num < min || num > max) { return false; }
            }
            // Unlimited args
            else if (args[p] == "**") { return true; }


            // +1
            p++;
        }

        // Passed
        return true;
    }
    public static string Execute(string command)
    {
        if (game == null || gameManager == null || levelManager == null || playerManager == null)
            throw new Exception("CommandManager not initialized");

        foreach (Command commandObj in commands)
            if (commandObj.IsCommand(command)) { return commandObj.TryExecute(command); }
        return $"Unknown command '{command}'";
    }
    // Command functions
    private static bool CTeleport(string command)
    {
        string[] coords = command.Split(' ')[1].Split(',');
        CameraManager.CameraDest = new Vector2(float.Parse(coords[0]), float.Parse(coords[1]));
        return true;
    }
    private static bool CHealth(string command)
    {
        string[] parts = command.Split(' ');
        if (parts[1] == "set") { gameManager!.UIManager.HealthBar.CurrentValue = int.Parse(parts[2]); return true; }
        if (parts[1] == "change") { gameManager!.UIManager.HealthBar.CurrentValue += int.Parse(parts[2]); return true; }
        return false;
    }
    private static bool CMoveSpeed(string command)
    {
        Constants.PlayerSpeed = int.Parse(command.Split(' ')[1]);
        return true;
    }
    private static bool CForceQuit(string command) { throw new Exception("Force quit"); }
    private static bool CQuit(string command) { game?.Exit(); return true; }
    private static bool CLocation(string command) { Logger.Log($"Player location: {CameraManager.Camera}"); return true; }
    private static bool CSay(string command) { Logger.Log(string.Join(' ', command.Split(' ')[1..])); return true; }
    private static bool CLevel(string command)
    {
        string[] parts = command.Split(' ');
        try
        {
            if (parts[1] == "load")
            {
                levelManager!.LoadLevel(gameManager!, parts[2]);
                return true;
            }
            else if (parts[1] == "read")
            {
                levelManager!.ReadLevel(gameManager!.UIManager, parts[2]);
                return true;
            }
        }
        catch { }
        return false;
    }
    private static bool CMood(string command)
    {
        string mood = command.Split(' ')[1];
        if (mood == "calm") { StateManager.Mood = Mood.Calm; return true; }
        else if (mood == "dark") { StateManager.Mood = Mood.Dark; return true; }
        else if (mood == "epic") { StateManager.Mood = Mood.Epic; return true; }
        return false;
    }
    private static bool CDaytime(string command)
    {
        string[] parts = command.Split(' ');
        if (parts[1] == "set")
            gameManager!.DayTime = int.Parse(parts[2]);
        else if (parts[1] == "change")
            gameManager!.DayTime += int.Parse(parts[2]);
        else
            return false;
        return true;
    }
}
