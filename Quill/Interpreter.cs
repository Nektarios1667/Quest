using NCalc;
using Quest.Quill.Functions;
using SharpDX.MediaFoundation.DirectX;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Quest.Quill;
public static class Interpreter
{
    private static int l = 0;
    private static Dictionary<string, string> Variables = [];
    private static Dictionary<string, string> Parameters = [];
    private static Dictionary<string, (int line, string[] parameters)> Functions = [];
    private static List<int> Callbacks = [];
    private static string[] Lines = [];

    private readonly static Dictionary<string, IBuiltinFunction> BuiltinFunctions = new() {
        { "readfile", new ReadFile() },
        { "execute", new Execute() },
        { "log", new Log() },
        { "warn", new Warn() },
        { "error", new Error() },
        { "teleport", new Teleport() },
        { "loadlevel", new LoadLevel() },
        { "unloadlevel", new UnloadLevel() },
        { "readlevel", new ReadLevel() },
        { "give", new Give() },
        { "getitem", new GetItem() },
        { "getitem2d", new GetItem2D() },
        { "contains", new Contains() },
        { "randomint", new RandomInt() },
    };

    private static readonly Dictionary<string, string> ExternalSymbols = new() {
        { "<ready>", "false" },
    };
    public static void UpdateSymbols(GameManager game, PlayerManager player)
    {
        DebugManager.StartBenchmark("QuillSymbolsUpdate");

        // Check
        //if (StateManager.State != GameState.Game) return;

        // Player
        ExternalSymbols["<playercoord_x>"] = CameraManager.TileCoord.X.ToString();
        ExternalSymbols["<playercoord_y>"] = CameraManager.TileCoord.Y.ToString();
        ExternalSymbols["<playercoord>"] = $"{CameraManager.TileCoord.X};{CameraManager.TileCoord.Y}";
        ExternalSymbols["<playerhealth>"] = game.UIManager.HealthBar.CurrentValue.ToString();
        ExternalSymbols["<playermaxhealth>"] = game.UIManager.HealthBar.MaxValue.ToString();
        ExternalSymbols["<playerspeed>"] = Constants.PlayerSpeed.ToString();
        ExternalSymbols["<isstuck>"] = (!(player.TileBelow?.Type.IsWalkable ?? true)).ToString().ToLower();
        ExternalSymbols["<tilebelow>"] = player.TileBelow?.Type.ToString() ?? "NUL";
        ExternalSymbols["<camera_x>"] = CameraManager.Camera.X.ToString();
        ExternalSymbols["<camera_y>"] = CameraManager.Camera.Y.ToString();
        ExternalSymbols["<camera>"] = $"{CameraManager.Camera.X};{CameraManager.Camera.Y}";
        // Level
        ExternalSymbols["<currentlevel>"] = game.LevelManager.Level.Name.WrapSingleQuotes();
        ExternalSymbols["<currentworld>"] = game.LevelManager.Level.World.WrapSingleQuotes();
        ExternalSymbols["<spawn>"] = game.LevelManager.Level.Spawn.CoordString();
        // Game
        ExternalSymbols["<gametime>"] = game.GameTime.ToString();
        ExternalSymbols["<daytime>"] = game.DayTime.ToString();
        ExternalSymbols["<totaltime>"] = game.TotalTime.ToString();
        ExternalSymbols["<gamestate>"] = StateManager.State.ToString().WrapSingleQuotes();
        // Inventory
        ExternalSymbols["<inventoryitems>"] = player.Inventory.GetItemsString().WrapSingleQuotes();
        ExternalSymbols["<inventoryamounts>"] = player.Inventory.GetItemsAmountString().WrapSingleQuotes();
        ExternalSymbols["<inventorysize_x>"] = player.Inventory.Width.ToString();
        ExternalSymbols["<inventorysize_y>"] = player.Inventory.Height.ToString();
        ExternalSymbols["<inventorysize>"] = $"{player.Inventory.Width};{player.Inventory.Height}";
        ExternalSymbols["<isinventoryopen>"] = player.Inventory.Opened.ToString();
        ExternalSymbols["<equippedslot>"] = player.Inventory.EquippedSlot.ToString();
        ExternalSymbols["<equippeditem>"] = (player.Inventory.Equipped?.Name ?? "NUL").WrapSingleQuotes();
        ExternalSymbols["<equippeditemuid>"] = player.Inventory.Equipped?.UID.ToString() ?? "-1";
        ExternalSymbols["<equippeditemamount>"] = player.Inventory.Equipped?.Amount.ToString() ?? "0";
        // Technical
        ExternalSymbols["<ready>"] = "true";
        ExternalSymbols["<fps>"] = (1f / game.DeltaTime).ToString();
        ExternalSymbols["<deltatime>"] = game.DeltaTime.ToString();
        ExternalSymbols["<ispaused>"] = (StateManager.OverlayState == OverlayState.Pause).ToString();
        ExternalSymbols["<vsync>"] = Constants.VSYNC.ToString();
        ExternalSymbols["<resolution_x>"] = Constants.ScreenResolution.X.ToString();
        ExternalSymbols["<resolution_y>"] = Constants.ScreenResolution.X.ToString();
        ExternalSymbols["<resolution>"] = $"{Constants.ScreenResolution.X};{Constants.ScreenResolution.Y}";
        ExternalSymbols["<fpslimit>"] = Constants.FPS.ToString();
        DebugManager.EndBenchmark("QuillSymbolsUpdate");
    }
    // Helpers
    static void FillParameters(ref Expression expr, Dictionary<string, string> vars)
    {
        foreach (var kvp in vars)
        {
            expr.Parameters[kvp.Key] = kvp.Value;
        }
    }
    static int FindLine(string[] lines, string target, int start = 0)
    {
        for (int i = start; i < lines.Length; i++)
            if (lines[i].Split(' ')[0].Trim() == target)
                return i;
        return -1;
    }
    static int FindLineBackwards(string[] lines, string target, int start = 0)
    {
        for (int i = start; i >= 0; i--)
            if (lines[i].Split(' ')[0].Trim() == target)
                return i;
        return -1;
    }
    static void ReplaceVariables(ref string line, Dictionary<string, string> vars)
    {
        foreach (var kvp in vars)
            line = line.Replace('=' + kvp.Key, kvp.Value);
    }
    public static bool ContainsAny(string str, string chars)
    {
        foreach (char c in chars)
            if (str.Contains(c)) return true;
        return false;
    }
    // Run
    public static async Task RunScriptAsync(string code)
    {
        try
        {
            await RunScriptCore(code);
        }
        catch (Exception ex)
        {
            Logger.Error($"Quill execution error: {ex.Message}");
        }
    }
    private static async Task RunScriptCore(string code)
    {
        Variables = [];
        Parameters = [];
        Functions = [];
        Callbacks = [];

        Lines = code.Split("\n");
        for (l = 0; l < Lines.Length; l++)
        {
            if (l < 0 || l >= Lines.Length) break;
            string line = Lines[l].Trim();

            // Comment
            if (line.StartsWith("//") || string.IsNullOrWhiteSpace(line)) continue;

            // Fill variables
            ReplaceVariables(ref line, Variables);
            ReplaceVariables(ref line, Parameters);
            ReplaceVariables(ref line, ExternalSymbols);

            // Evaluations
            line = Regex.Replace(line, @"\{([^}]*)\}", match =>
            {
                string exprStr = match.Groups[1].Value.Trim();
                Expression expr = new(exprStr);
                var result = expr.Evaluate();

                return result?.ToString() ?? "";
            });

            string[] parts = line.Split(' ');
            string command = parts[0].Trim().ToLower();

            switch (command)
            {
                case "num": HandleNum(parts); break;
                case "str": HandleStr(parts); break;
                case "breakwhile": HandleBreakWhile(parts); break;
                case "continuewhile": HandleContinueWhile(parts); break;
                case "if": HandleIf(parts); break;
                case "endif": break; // Marker
                case "while": HandleWhile(parts); break;
                case "endwhile": HandleEndWhile(parts); break;
                case "func": HandleFunc(parts); break;
                case "endfunc": HandleEndFunc(); break;
                case "call": HandleCall(parts); break;
                case "sleep": await HandleSleep(parts); break;
                case "wait": await HandleWait(parts); break;
                default:
                    if (BuiltinFunctions.TryGetValue(command, out var func))
                        HandleBuiltin(func, parts);
                    else
                        Console.WriteLine($"Unknown command: {line}");
                    break;
            }
        }
    }
    // Handlers
    private static void HandleNum(string[] parts)
    {
        string varName = parts[1];
        if (ContainsAny(varName, "`~!@#$%^&*()-=+[]{}\\|;:'\",<.>/?"))
        {
            Console.WriteLine($"Invalid variable name: {varName}");
            return;
        }

        string varValue = parts[2];
        Expression expr = new(varValue);
        FillParameters(ref expr, Variables);
        var result = expr.Evaluate();
        Variables[varName] = result?.ToString() ?? "";
    }

    private static void HandleStr(string[] parts)
    {
        string varName = parts[1];
        if (ContainsAny(varName, "`~!@#$%^&*()-=+[]{}\\|;:'\",<.>/?"))
        {
            Console.WriteLine($"Invalid variable name: {varName}");
            return;
        }

        string varValue = parts[2];
        Variables[varName] = varValue;
    }

    private static void HandleBreakWhile(string[] parts)
    {
        if (parts.Length < 2)
            l = FindLine(Lines, "endwhile", l);
        else
            l = FindLine(Lines, $"endwhile {parts[1]}", l);
    }

    private static void HandleContinueWhile(string[] parts)
    {
        if (parts.Length < 2)
            l = FindLineBackwards(Lines, "while", l) - 1;
        else
            l = FindLineBackwards(Lines, $"while {parts[1]}", l) - 1;
    }

    private static void HandleIf(string[] parts)
    {
        string name = parts[1];
        int conditionStart = name.StartsWith('.') ? 2 : 1;
        Expression expr = new(string.Join(' ', parts[conditionStart..]));
        var result = expr.Evaluate();
        if (result is bool b && !b)
            l = FindLine(Lines, $"endif{(name.StartsWith('.') ? $" {name}" : "")}", l);
    }

    private static void HandleWhile(string[] parts)
    {
        string name = parts[1];
        int conditionStart = name.StartsWith('.') ? 2 : 1;
        Expression expr = new(string.Join(' ', parts[conditionStart..]));
        FillParameters(ref expr, Variables);
        var result = expr.Evaluate();
        if (result is bool b && !b)
            l = FindLine(Lines, $"endwhile{(name.StartsWith('.') ? $" {name}" : "")}", l);
    }

    private static void HandleEndWhile(string[] parts)
    {
        if (parts.Length < 2)
            l = FindLineBackwards(Lines, "while", l) - 1;
        else
            l = FindLineBackwards(Lines, $"while {parts[1]}", l) - 1;
    }

    private static void HandleFunc(string[] parts)
    {
        string funcName = parts[1];
        string[] funcParams = parts.Length <= 2 ? [] : parts[2..];
        Functions[funcName] = (l, funcParams);
        l = FindLine(Lines, $"endfunc {funcName}", l);
        if (l == -1)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Function '{funcName}' missing endfunc");
            Console.ResetColor();
            throw new InvalidOperationException();
        }
    }

    private static void HandleEndFunc()
    {
        Parameters.Clear();
        if (Callbacks.Count > 0)
        {
            l = Callbacks[^1];
            Callbacks.RemoveAt(Callbacks.Count - 1);
        }
    }

    private static void HandleCall(string[] parts)
    {
        string funcName = parts[1];
        var function = Functions.TryGetValue(funcName, out var f) ? f : (-1, []);
        if (function.line == -1)
        {
            Console.WriteLine($"Function not found: {funcName}");
            return;
        }

        string[] stringParams = parts.Length <= 2 ? [] : string.Join(' ', parts[1..]).Split(',');
        foreach (string param in stringParams)
        {
            string[] kvp = param.Split(':');
            if (kvp.Length != 2)
            {
                Console.WriteLine($"Invalid parameter: {param}");
                continue;
            }

            string pName = kvp[0].Trim();
            string pValue = kvp[1].Trim();
            Expression expr = new(pValue);
            FillParameters(ref expr, Variables);
            Parameters[pName] = expr.Evaluate()?.ToString() ?? "";
        }

        foreach (string p in function.parameters)
        {
            if (!Parameters.ContainsKey(p))
            {
                Console.WriteLine($"Function call '{funcName}' missing parameter '{p}' @ l{l}");
                return;
            }
        }

        Callbacks.Add(l);
        l = function.line;
    }

    private static async Task HandleSleep(string[] parts)
    {
        string waitTimeStr = parts[1];
        Expression expr = new(waitTimeStr);
        FillParameters(ref expr, Variables);
        var result = expr.Evaluate();
        if (result is int ms)
            await Task.Delay(ms);
        else
            Console.WriteLine($"Invalid sleep time: {waitTimeStr}");
    }

    private static async Task HandleWait(string[] parts)
    {
        string waitTimeStr = parts[1];
        string conditionStr = parts[2].Trim();
        Expression expr = new(conditionStr);
        FillParameters(ref expr, Variables);
        var result = expr.Evaluate();
        if (result is not bool b || !int.TryParse(waitTimeStr, out int waitTime))
        {
            Console.WriteLine($"Invalid wait command: {string.Join(' ', parts)}");
            return;
        }

        if (!b)
        {
            l--;
            await Task.Delay(waitTime);
        }
    }

    private static void HandleBuiltin(IBuiltinFunction func, string[] parts)
    {
        string[] externalParams = parts.Length <= 1 ? [] : string.Join(' ', parts[1..])
            .Split(',', StringSplitOptions.TrimEntries);

        var resp = func.Run(externalParams);
        if (!resp.Success)
            Console.WriteLine($"Builtin function '{parts[0]}' failed: {resp.ErrorType} - {resp.ErrorMessage}");
        else
            foreach (var kvp in resp.OutputVariables)
                Variables[kvp.Key] = kvp.Value;
    }
}