using HarfBuzzSharp;
using NCalc;
using Quest.Quill.Functions;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quest.Quill;

public enum QuillPerformanceMode
{
    Low,
    Normal,
    High,
}

public class QuillInstance
{
    public QuillPerformanceMode PerformanceMode { get; private set; } = QuillPerformanceMode.Normal;
    public float SleepTimer { get; private set; } = 0;
    public bool IsSleeping => SleepTimer > 0;
    public QuillScript Script { get; private set; }
    public int L { get; set; } = 0;
    public Dictionary<string, string> Variables { get; private set; } = [];
    public Dictionary<string, string> Locals { get; private set; } = [];
    public Dictionary<string, (int line, string[] parameters)> Functions { get; private set; } = [];
    public List<int> Callbacks { get; private set; } = [];
    public Stack<string> Scopes { get; private set; } = [];
    public Dictionary<int, int> OnceFlags { get; private set; } = [];
    public string[] Lines { get; private set; }
    public List<QuillError> Errors { get; private set; } = [];
    public bool Done => L >= Lines.Length;
    public QuillInstance(QuillScript script)
    {
        Script = script;
        Lines = script.SourceCode.Split('\n');
    }

    public int Step(GameManager game, int budget)
    {
        int stepsUsed = 0;
        try
        {
            // Check sleeping
            if (IsSleeping)
            {
                SleepTimer -= game.DeltaTime * 1000; // Convert to ms
                if (IsSleeping)
                    return stepsUsed;
            }

            // Run lines
            while (budget-- > 0 && !Done)
            {
                Interpreter.RunLine(this);
                stepsUsed++;
                L++;
            }
        } catch (Exception e)
        {
            Errors.Add(new(L, QuillErrorType.RuntimeError, e.Message));
        }
        return stepsUsed;
    }
    public void Sleep(int ms) => SleepTimer = ms;
    public void SetPerformanceMode(QuillPerformanceMode mode) => PerformanceMode = mode;
}

public static partial class Interpreter
{
    private static readonly List<QuillInstance> Scripts = [];

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
        { "setitem", new SetItem() },
        { "setitem2d", new SetItem2D() },
        { "append", new Append() },
        { "remove", new Remove() },
        { "insert", new Insert() },
        { "contains", new Contains() },
        { "randomint", new RandomInt() },
        { "randomfloat", new RandomFloat() },
        { "notif", new Notif() },
        { "setvalue", new SetValue() },
        { "getvalue", new GetValue() },
    };

    private static readonly Dictionary<string, string> ExternalSymbols = new() {
        { "<ready>", "false" },
    };
    public static void UpdateSymbols(GameManager game, PlayerManager player)
    {
        DebugManager.StartBenchmark("QuillSymbolsUpdate");

        // Precheck
        if (Scripts.Count == 0)
        {
            DebugManager.EndBenchmark("QuillSymbolsUpdate");
            return;
        }

        // Player
        ExternalSymbols["<playercoord_x>"] = CameraManager.TileCoord.X.ToString();
        ExternalSymbols["<playercoord_y>"] = CameraManager.TileCoord.Y.ToString();
        ExternalSymbols["<playercoord>"] = $"{CameraManager.TileCoord.X};{CameraManager.TileCoord.Y}";
        ExternalSymbols["<playerhealth>"] = game.UIManager.HealthBar.CurrentValue.ToString();
        ExternalSymbols["<playermaxhealth>"] = game.UIManager.HealthBar.MaxValue.ToString();
        ExternalSymbols["<playerspeed>"] = Constants.PlayerSpeed.ToString();
        ExternalSymbols["<isstuck>"] = (!(player.TileBelow?.Type.IsWalkable ?? true)).ToString().ToLower();
        ExternalSymbols["<tilebelow>"] = player.TileBelow?.Type.Texture.ToString() ?? "NUL";
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
        ExternalSymbols["<time>"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        ExternalSymbols["<datetime>"] = DateTime.Now.ToString("yyyy;MM;dd/HH;mm;ss");
        ExternalSymbols["<fps>"] = (1f / game.DeltaTime).ToString();
        ExternalSymbols["<deltatime>"] = game.DeltaTime.ToString();
        ExternalSymbols["<ispaused>"] = (StateManager.OverlayState == OverlayState.Pause).ToString();
        ExternalSymbols["<vsync>"] = Constants.VSYNC.ToString().ToLower();
        ExternalSymbols["<resolution_x>"] = Constants.ScreenResolution.X.ToString();
        ExternalSymbols["<resolution_y>"] = Constants.ScreenResolution.X.ToString();
        ExternalSymbols["<resolution>"] = $"{Constants.ScreenResolution.X};{Constants.ScreenResolution.Y}";
        ExternalSymbols["<fpslimit>"] = Constants.FPS.ToString();


        DebugManager.EndBenchmark("QuillSymbolsUpdate");
    }
    // Helpers
    private static int FindLine(QuillInstance instance, string target, int start = 0)
    {
        for (int i = start; i < instance.Lines.Length; i++)
            if (instance.Lines[i].Trim().StartsWith(target.Trim()))
                return i;

        instance.Errors.Add(new(instance.L, QuillErrorType.BlockMismatch, $"Failed to find line '{target}'", fatal:true));
        return instance.L;
    }
    private static int FindLineBackwards(QuillInstance instance, string target, int start = -1)
    {
        if (start == -1) start = instance.Lines.Length - 1;

        for (int i = start; i >= 0; i--)
            if (instance.Lines[i].Trim().StartsWith(target.Trim()))
                return i;

        instance.Errors.Add(new(instance.L, QuillErrorType.BlockMismatch, $"Failed to find line '{target}'", fatal:true));
        return instance.L;
    }
    private static void ReplaceVariables(ref string line, Dictionary<string, string> vars)
    {
        foreach (var kvp in vars)
            if (line.Contains(kvp.Key))
                line = line.Replace('=' + kvp.Key, kvp.Value);
    }
    private static bool ContainsAny(string str, string chars)
    {
        foreach (char c in chars)
            if (str.IndexOf(c) > 0) return true;
        return false;
    }
    public static IReadOnlyList<QuillInstance> GetQuillInstances() => Scripts;
    // Run
    public static void RunScript(QuillScript script)
    {
        Scripts.Add(new QuillInstance(script));
    }
    public static void Update(GameManager game, PlayerManager player)
    {

        UpdateSymbols(game, player);

        if (Scripts.Count == 0) return;
        DebugManager.StartBenchmark("QuillUpdate");
        // Run scripts
        int budget = Constants.QuillUpdatesPerFrame;
        int stepsPerScript = budget / Scripts.Count;
        for (int s = Scripts.Count - 1; s >= 0 && budget > 0; s--)
        {
            var script = Scripts[s];

            // Divy up budget
            int steps = stepsPerScript;
            if (script.PerformanceMode == QuillPerformanceMode.Low)
                steps = stepsPerScript / 2;
            else if (script.PerformanceMode == QuillPerformanceMode.High)
                steps = stepsPerScript * 2;
            steps = Math.Clamp(steps, 1, Math.Min(budget, 50)); // Limit between 1 and 50/budget

            // Run script
            int stepsUsed = script.Step(game, steps);
            // Cleanup
            if (script.Done)
                Scripts.RemoveAt(s);
            budget -= stepsUsed;
        }

        DebugManager.EndBenchmark("QuillUpdate");
    }
    public static void RunLine(QuillInstance instance)
    {
        string line = instance.Lines[instance.L].Trim();

        // Handle errors
        if (instance.Errors.Count > 0)
            OutputErrors(instance);

        // Comment
        if (line.StartsWith("//", StringComparison.Ordinal) || string.IsNullOrWhiteSpace(line)) return;

        // Fill variables
        ReplaceVariables(ref line, instance.Locals);
        ReplaceVariables(ref line, instance.Variables);
        ReplaceVariables(ref line, ExternalSymbols);

        // Evaluations
        line = CurlyExpressions().Replace(line, match =>
        {
            string exprStr = match.Groups[1].Value.Trim();
            Expression expr = new(exprStr);
            var result = expr.Evaluate();

            return result?.ToString()?.ToLower() ?? "";
        });

        ExecuteCommand(line, instance);
    }
    public static void ExecuteCommand(string line, QuillInstance instance)
    {
        string[] parts = line.Split(' ');
        string command = parts[0].Trim();
        string argsStr = "";
        if (parts.Length > 1)
            argsStr = string.Join(' ', parts[1..]).Trim();

        switch (command)
        {
            case "#perfmode": HandlePerfMode(instance, argsStr); break;
            case "num": HandleNum(instance, argsStr); break;
            case "str": HandleStr(instance, argsStr); break;
            case "breakwhile": HandleBreakWhile(instance, argsStr); break;
            case "continuewhile": HandleContinueWhile(instance, argsStr); break;
            case "if": HandleIf(instance, argsStr); break;
            case "endif": break; // Marker
            case "while": HandleWhile(instance, argsStr); break;
            case "endwhile": HandleEndWhile(instance, argsStr); break;
            case "func": HandleFunc(instance, argsStr); break;
            case "endfunc": HandleEndFunc(instance, argsStr); break;
            case "sleep": HandleSleep(instance, argsStr); break;
            case "wait": HandleWait(instance, argsStr); break;
            case "only": HandleOnly(instance, argsStr); break;
            case "endonly": break; // Marker
            case "return": HandleReturn(instance, argsStr); break;
            default:
                if (BuiltinFunctions.TryGetValue(command, out var builtinFunc))
                    HandleBuiltin(instance, builtinFunc, command, argsStr);
                else if (instance.Functions.TryGetValue(command, out var func))
                    HandleCall(instance, func, command, argsStr);
                else
                    instance.Errors.Add(new(instance.L, QuillErrorType.UnknownCommand, command));
                break;
        }
    }
    public static void OutputErrors(QuillInstance instance)
    {
        foreach (QuillError error in instance.Errors)
        {
            Logger.Error($"Quill | {instance.Script.Name} @{error.Line + 1} | {error.ErrorType} - {error.Message}");
            if (error.Fatal)
            {
                Logger.Error($"Quill | {instance.Script.Name} | Execution stopped due to fatal error");
                return;
            }
        }
        instance.Errors.Clear();
    }
    // Handlers
    private static void HandlePerfMode(QuillInstance inst, string argsStr)
    {
        string[] args = argsStr.Split(',');
        if (args.Length != 1)
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"#perfmode expects 1 argument, received {args.Length}"));
            return;
        }
        if (!int.TryParse(args[0], out int value) || value < 0 || value > 2)
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"Invalid perfmode: {args[0]}"));
            return;
        }

        // Set performance mode
        inst.SetPerformanceMode(Enum.Parse<QuillPerformanceMode>(args[0]));
    }
    private static void HandleNum(QuillInstance inst, string argsStr)
    {
        // Args
        string[] args = argsStr.Split(',', StringSplitOptions.TrimEntries);
        if (args.Length != 2)
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"num command expects 2 arguments, received {args.Length}"));
            return;
        }

        // Name
        string varName = args[0];
        if (ContainsAny(varName, "`~!@#$%^&*()-=+[]{}\\|;:'\",<.>/?"))
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.InvalidName, varName));
            return;
        }

        // Value
        if (!float.TryParse(args[1], out float num))
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.InvalidExpression, $"Invalid number expression '{args[1]}'"));
            return;
        }
        if (inst.Scopes.Count == 0)
            inst.Variables[varName] = num.ToString("F20").TrimEnd('0').TrimEnd('.');
        else
            inst.Locals[varName] = num.ToString("F20").TrimEnd('0').TrimEnd('.');
    }

    private static void HandleStr(QuillInstance inst, string argsStr)
    {
        // Args
        string[] args = argsStr.Split(',', StringSplitOptions.TrimEntries);
        if (args.Length != 2)
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"str command expects 2 arguments, received {args.Length}"));
            return;
        }

        // Name
        string varName = args[0];
        if (ContainsAny(varName, "`~!@#$%^&*()-=+[]{}\\|;:'\",<.>/?"))
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.InvalidName, varName));
            return;
        }

        // Value
        if (inst.Scopes.Count == 0)
            inst.Variables[varName] = args[1];
        else
            inst.Locals[varName] = args[1];
    }

    private static void HandleBreakWhile(QuillInstance inst, string argsStr)
    {
        string[] args = argsStr.Split(' ');

        if (args.Length == 1)
            inst.L = FindLine(inst, "endwhile", inst.L);
        else if (args.Length == 0)
            inst.L = FindLine(inst, $"endwhile {args[0]}", inst.L);
        else
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"breakwhile command expects 0 or 1 arguments, received {args.Length}"));

    }

    private static void HandleContinueWhile(QuillInstance inst, string argsStr)
    {
        string[] args = argsStr.Split(' ');

        if (args.Length == 1)
            inst.L = FindLineBackwards(inst, $"while {args[0]}", inst.L) - 1;
        else if (args.Length == 0)
            inst.L = FindLineBackwards(inst, "while", inst.L) - 1;
        else
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"continuewhile command expects 0 or 1 arguments, received {args.Length}"));
    }

    private static void HandleIf(QuillInstance inst, string argsStr)
    {
        string[] args = argsStr.Split(' ');
        if (args.Length >= 2 && args[0].StartsWith('.'))
        {
            string label = args[0];
            Expression expr = new(string.Join(' ', args[1..]));
            var result = expr.Evaluate();
            if (result is bool b && !b)
                inst.L = FindLine(inst, $"endif {label}", inst.L);
        } else if (args.Length >= 1)
        {
            Expression expr = new(string.Join(' ', args[0..]));
            var result = expr.Evaluate();
            if (result is bool b && !b)
                inst.L = FindLine(inst, $"endif", inst.L);
        } else
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"if command expects 1 or 2 arguments, received {args.Length}"));
    }

    private static void HandleWhile(QuillInstance inst, string argsStr)
    {
        string[] args = argsStr.Split(' ');
        if (args.Length >= 2 && args[0].StartsWith('.'))
        {
            string label = args[0];
            Expression expr = new(string.Join(' ', args[1..]));
            var result = expr.Evaluate();
            if (result is bool b && !b)
                inst.L = FindLine(inst, $"endwhile {label}", inst.L);
        }
        else if (args.Length >= 1)
        {
            Expression expr = new(string.Join(' ', args[0..]));
            var result = expr.Evaluate();
            if (result is bool b && !b)
                inst.L = FindLine(inst, $"endwhile", inst.L);
        }
        else
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"while command expects 1 or 2 arguments, received {args.Length}"));
    }

    private static void HandleEndWhile(QuillInstance inst, string argsStr)
    {
        string[] args = argsStr.Split(' ');

        // Loop back
        if (args.Length == 0)
            inst.L = FindLineBackwards(inst, "while", inst.L) - 1;
        else if (args.Length == 1)
            inst.L = FindLineBackwards(inst, $"while {args[0]}", inst.L) - 1;
        else
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"endwhile command expects 0 or 1 arguments, received {args.Length}")   );

        // Delay
        if (inst.PerformanceMode == QuillPerformanceMode.Low)
            inst.Sleep(1000); // ms
        else if (inst.PerformanceMode == QuillPerformanceMode.Normal)
            inst.Sleep(100); // ms
    }

    private static void HandleFunc(QuillInstance inst, string argsStr)
    {
        string[] args = argsStr.Split(' ');

        if (args.Length < 1)
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"func command expects at least 1 argument, received {args.Length}"));
            return;
        }

        string funcName = args[0];
        string[] funcParams = args.Length < 2 ? [] : args[1..];

        inst.Functions[funcName] = (inst.L, funcParams);
        inst.L = FindLine(inst, "endfunc", inst.L);
    }

    private static void HandleEndFunc(QuillInstance inst, string argsStr)
    {
        inst.Locals.Clear();
        if (inst.Callbacks.Count > 0)
        {
            inst.L = inst.Callbacks[^1];
            inst.Callbacks.RemoveAt(inst.Callbacks.Count - 1);
        }
        if (inst.Scopes.Count > 0)
            inst.Scopes.Pop();   
    }

    private static void HandleCall(QuillInstance inst, (int line, string[] parameters) function, string funcName, string argsStr)
    {
        string[] args = argsStr.Split(',', StringSplitOptions.TrimEntries);

        // Functions not found
        if (function.line < 0)
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.FunctionNotFound, funcName));
            return;
        }

        // Parse parameters
        foreach (string param in args)
        {
            string[] kvp = param.Split(':');
            if (kvp.Length != 2)
            {
                inst.Errors.Add(new(inst.L, QuillErrorType.InvalidExpression, $"Invalid parameter: {param}"));
                continue;
            }

            string pName = kvp[0].Trim();
            string pValue = kvp[1].Trim();
            inst.Locals[pName] = pValue;
        }

        // Check parameters match
        foreach (string p in function.parameters)
        {
            if (!inst.Locals.ContainsKey(p))
            {
                inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"Function call '{funcName}' missing parameter '{p}'"));
                return;
            }
        }

        // Go to function
        inst.Scopes.Push(funcName);
        inst.Callbacks.Add(inst.L);
        inst.L = function.line;
    }

    private static void HandleSleep(QuillInstance inst, string argsStr)
    {
        string[] args = argsStr.Split(',');
        if (args.Length != 1)
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"sleep command expects 1 argument, received {args.Length}"));
            return;
        }

        string waitTimeStr = args[0];
        if (int.TryParse(waitTimeStr, out var ms))
            inst.Sleep(ms);
        else
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"Invalid sleep time: {waitTimeStr}"));
    }

    private static void HandleWait(QuillInstance inst, string argsStr)
    {
        string[] args = argsStr.Split(',');
        if (args.Length != 2)
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"wait command expects 2 arguments, received {args.Length}"));
            return;
        }

        string conditionStr = args[0].Trim();
        string waitTimeStr = args[1];
        Expression expr = new(conditionStr);
        var result = expr.Evaluate();
        if (result is not bool b || !int.TryParse(waitTimeStr, out int waitTime))
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"Invalid wait command: wait {argsStr}"));
            return;
        }

        if (!b)
        {
            inst.L--;
            inst.Sleep(waitTime);
        }
    }
    private static void HandleOnly(QuillInstance inst, string argsStr)
    {
        // Parse args
        string[] args = argsStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string label = "";

        int limit;
        // [label] (times) / [label] / (times)
        if (args.Length == 2 && args[0].StartsWith('.'))
        {
            label = args[0];
            limit = int.TryParse(args[1], out int v) ? v : -1;
        }
        else if (args.Length == 1 && args[0].StartsWith('.'))
        {
            label = args[0];
            limit = 1;
        }
        else if (args.Length == 1)
            limit = int.TryParse(args[0], out int v) ? v : -1;
        else if (args.Length == 0)
            limit = 1;
        else
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"sleep command expects 0, 1, or 2 arguments, received {args.Length}"));
            return;
        }

        // Check limit
        if (limit <= 0)
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"Invalid only amount"));
            return;
        }

        // Check flag
        if (!inst.OnceFlags.TryGetValue(inst.L, out int counter))
            inst.OnceFlags[inst.L] = 1;
        else if (counter < limit)
            inst.OnceFlags[inst.L] = ++counter;
        else
            inst.L = FindLine(inst, $"endonly {label}", inst.L);
    }
    private static void HandleReturn(QuillInstance inst, string argsStr)
    {
        string[] args = argsStr.Split(' ');
        if (args.Length == 1)
        {
            inst.Variables["[return]"] = args[0];
        }
        else if (args.Length != 0)
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"return command expects 0 or 1 arguments, received {args.Length}"));
            return;
        }

        inst.L = FindLine(inst, "endfunc", inst.L) - 1;
    }

    private static void HandleBuiltin(QuillInstance inst, IBuiltinFunction func, string funcName, string argsStr)
    {
        string[] args = argsStr.Split(',', StringSplitOptions.TrimEntries);

        var resp = func.Run(inst.Variables, args);
        if (!resp.Success)
            inst.Errors.Add(new(inst.L, resp.ErrorType!.Value, resp.ErrorMessage!));
    }

    [GeneratedRegex(@"\{([^}]*)\}", RegexOptions.Compiled)]
    private static partial Regex CurlyExpressions();
}