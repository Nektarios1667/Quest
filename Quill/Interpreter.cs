using NCalc;
using Quest.Quill.Functions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Quest.Quill;

public enum QuillErrorType
{
    UnknownError,
    SyntaxError,
    RuntimeError,
    ParameterMismatch,
    UnknownCommand,
    FunctionNotFound,
    InvalidVariableName,
    InvalidExpression,
    BlockMismatch,
    Fatal,
}
public struct QuillError
{
    public int Line { get; }
    public QuillErrorType ErrorType { get; }
    public string Message { get; }
    public bool Fatal { get; }
    public QuillError(int line, QuillErrorType errorType, string message, bool fatal = false)
    {
        Line = line;
        ErrorType = errorType;
        Message = message;
        Fatal = fatal;
    }
}
public static partial class Interpreter
{
    private readonly static List<QuillError> Errors = [];
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
        { "setitem", new SetItem() },
        { "setitem2d", new SetItem2D() },
        { "append", new Append() },
        { "remove", new Remove() },
        { "insert", new Insert() },
        { "contains", new Contains() },
        { "randomint", new RandomInt() },
        { "randomfloat", new RandomFloat() },
        { "notif", new Notif() }
    };

    private static readonly Dictionary<string, string> ExternalSymbols = new() {
        { "<ready>", "false" },
    };
    public static void UpdateSymbols(GameManager game, PlayerManager player)
    {
        DebugManager.StartBenchmark("QuillSymbolsUpdate");

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
        ExternalSymbols["<time>"] = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        ExternalSymbols["<datetime>"] = DateTime.Now.ToString("yyyy;MM;dd/HH;mm;ss;");
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
    static int FindLine(string[] lines, string target, int start = 0)
    {
        for (int i = start; i < lines.Length; i++)
            if (lines[i].Trim().StartsWith(target.Trim()))
                return i;

        Errors.Add(new(l, QuillErrorType.BlockMismatch, $"Failed to find line '{target}'", fatal:true));
        return l;
    }
    static int FindLineBackwards(string[] lines, string target, int start = 0)
    {
        for (int i = start; i >= 0; i--)
            if (lines[i].Trim().StartsWith(target.Trim()))
                return i;

        Errors.Add(new(l, QuillErrorType.BlockMismatch, $"Failed to find line '{target}'", fatal:true));
        return l;
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
    public static async Task RunScriptAsync(QuillScript script)
    {
        try
        {
            await RunScriptCore(script);
        }
        catch (Exception ex)
        {
            Logger.Error($"Quill execution error: {ex.Message}");
        }
    }
    private static async Task RunScriptCore(QuillScript script)
    {
        Variables = [];
        Parameters = [];
        Functions = [];
        Callbacks = [];

        Lines = script.SourceCode.Split("\n");
        for (l = 0; l < Lines.Length; l++)
        {
            if (l < 0 || l >= Lines.Length)
                Errors.Add(new(l, QuillErrorType.RuntimeError, $"Line index out of bounds - {l}", fatal: true));
            string line = Lines[l].Trim();

            // Handle errors
            foreach (QuillError error in Errors)
            {
                Logger.Error($"Quill | {script.ScriptName} @{error.Line} | {error.ErrorType} - {error.Message}");
                if (error.Fatal)
                {
                    Logger.Error($"Quill | {script.ScriptName} | Execution stopped due to fatal error");
                    return;
                }
            }
            Errors.Clear();

            // Comment
            if (line.StartsWith("//") || string.IsNullOrWhiteSpace(line)) continue;

            // Fill variables
            ReplaceVariables(ref line, Variables);
            ReplaceVariables(ref line, Parameters);
            ReplaceVariables(ref line, ExternalSymbols);

            // Evaluations
            line = CurlyExpressions().Replace(line, match =>
            {
                string exprStr = match.Groups[1].Value.Trim();
                Expression expr = new(exprStr);
                var result = expr.Evaluate();

                return result?.ToString() ?? "";
            });

            await ExecuteCommand(line);
        }
    }
    public static async Task ExecuteCommand(string line)
    {
        string command = line.Split(' ')[0].Trim().ToLower();
        string argsStr = line[command.Length..].Trim();

        switch (command)
        {
            case "num": HandleNum(argsStr); break;
            case "str": HandleStr(argsStr); break;
            case "breakwhile": HandleBreakWhile(argsStr); break;
            case "continuewhile": HandleContinueWhile(argsStr); break;
            case "if": HandleIf(argsStr); break;
            case "endif": break; // Marker
            case "while": HandleWhile(argsStr); break;
            case "endwhile": HandleEndWhile(argsStr); break;
            case "func": HandleFunc(argsStr); break;
            case "endfunc": HandleEndFunc(); break;
            case "call": HandleCall(argsStr); break;
            case "sleep": await HandleSleep(argsStr); break;
            case "wait": await HandleWait(argsStr); break;
            default:
                if (BuiltinFunctions.TryGetValue(command, out var func))
                    HandleBuiltin(func, command, argsStr);
                else
                    Errors.Add(new(l, QuillErrorType.UnknownCommand, command));
                break;
        }
    }
    // Handlers
    private static void HandleNum(string argsStr)
    {
        string[] args = argsStr.Split(',', StringSplitOptions.TrimEntries);
        if (args.Length != 2)
        {
            Errors.Add(new(l, QuillErrorType.ParameterMismatch, $"num command expects 2 arguments, received {args.Length}"));
            return;
        }

        string varName = args[0];
        if (ContainsAny(varName, "`~!@#$%^&*()-=+[]{}\\|;:'\",<.>/?"))
        {
            Errors.Add(new(l, QuillErrorType.InvalidVariableName, varName));
            return;
        }

        string varValue = args[1];
        Expression expr = new(varValue);
        var result = expr.Evaluate();
        Variables[varName] = result?.ToString() ?? "";
    }

    private static void HandleStr(string argsStr)
    {
        string[] args = argsStr.Split(',', StringSplitOptions.TrimEntries);
        if (args.Length != 2)
        {
            Errors.Add(new(l, QuillErrorType.ParameterMismatch, $"str command expects 2 arguments, received {args.Length}"));
            return;
        }

        string varName = args[0];
        if (ContainsAny(varName, "`~!@#$%^&*()-=+[]{}\\|;:'\",<.>/?"))
        {
            Errors.Add(new(l, QuillErrorType.InvalidVariableName, varName));
            return;
        }

        string varValue = args[1];
        Variables[varName] = varValue;
    }

    private static void HandleBreakWhile(string argsStr)
    {
        string[] args = argsStr.Split(' ');

        if (args.Length == 1)
            l = FindLine(Lines, "endwhile", l);
        else if (args.Length == 0)
            l = FindLine(Lines, $"endwhile {args[0]}", l);
        else
            Errors.Add(new(l, QuillErrorType.ParameterMismatch, $"breakwhile command expects 0 or 1 arguments, received {args.Length}"));

    }

    private static void HandleContinueWhile(string argsStr)
    {
        string[] args = argsStr.Split(' ');

        if (args.Length == 1)
            l = FindLineBackwards(Lines, "while", l) - 1;
        else if (args.Length == 0)
            l = FindLineBackwards(Lines, $"while {args[0]}", l) - 1;
        else
            Errors.Add(new(l, QuillErrorType.ParameterMismatch, $"continuewhile command expects 0 or 1 arguments, received {args.Length}"));
    }

    private static void HandleIf(string argsStr)
    {
        string[] args = argsStr.Split(' ');
        if (args.Length >= 2 && args[0].StartsWith('.'))
        {
            string label = args[0];
            Expression expr = new(string.Join(' ', args[1..]));
            var result = expr.Evaluate();
            if (result is bool b && !b)
                l = FindLine(Lines, $"endif {label}", l);
        } else if (args.Length >= 1)
        {
            Expression expr = new(string.Join(' ', args[0..]));
            var result = expr.Evaluate();
            if (result is bool b && !b)
                l = FindLine(Lines, $"endif", l);
        } else
            Errors.Add(new(l, QuillErrorType.ParameterMismatch, $"if command expects 1 or 2 arguments, received {args.Length}"));
    }

    private static void HandleWhile(string argsStr)
    {
        string[] args = argsStr.Split(' ');
        if (args.Length >= 2 && args[0].StartsWith('.'))
        {
            string label = args[0];
            Expression expr = new(string.Join(' ', args[1..]));
            var result = expr.Evaluate();
            if (result is bool b && !b)
                l = FindLine(Lines, $"endwhile {label}", l);
        }
        else if (args.Length >= 1)
        {
            Expression expr = new(string.Join(' ', args[0..]));
            var result = expr.Evaluate();
            if (result is bool b && !b)
                l = FindLine(Lines, $"endwhile", l);
        }
        else
            Errors.Add(new(l, QuillErrorType.ParameterMismatch, $"while command expects 1 or 2 arguments, received {args.Length}"));
    }

    private static void HandleEndWhile(string argsStr)
    {
        string[] args = argsStr.Split(' ');

        if (args.Length == 0)
            l = FindLineBackwards(Lines, "while", l) - 1;
        else if (args.Length == 1)
            l = FindLineBackwards(Lines, $"while {args[0]}", l) - 1;
        else
            Errors.Add(new(l, QuillErrorType.ParameterMismatch, $"endwhile command expects 0 or 1 arguments, received {args.Length}")   );
    }

    private static void HandleFunc(string argsStr)
    {
        string[] args = argsStr.Split(' ');

        if (args.Length < 1)
        {
            Errors.Add(new(l, QuillErrorType.ParameterMismatch, $"func command expects at least 1 argument, received {args.Length}"));
            return;
        }

        string funcName = args[0];
        string[] funcParams = args.Length < 2 ? [] : args[1..];

        Functions[funcName] = (l, funcParams);
        l = FindLine(Lines, $"endfunc {funcName}", l);
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

    private static void HandleCall(string argsStr)
    {
        string[] args = argsStr.Split(',', StringSplitOptions.TrimEntries);

        if (args.Length < 1)
        {
            Errors.Add(new(l, QuillErrorType.ParameterMismatch, $"call command expects at least 1 argument, received {args.Length}"));
            return;
        }

        string funcName = args[0];
        var function = Functions.TryGetValue(funcName, out var f) ? f : (-1, []);
        if (function.line == -1)
        {
            Errors.Add(new(l, QuillErrorType.FunctionNotFound, funcName));
            return;
        }

        string[] stringParams = args.Length < 2 ? [] : args[1..];
        foreach (string param in stringParams)
        {
            string[] kvp = param.Split(':');
            if (kvp.Length != 2)
            {
                Errors.Add(new(l, QuillErrorType.InvalidExpression, $"Invalid parameter: {param}"));
                continue;
            }

            string pName = kvp[0].Trim();
            string pValue = kvp[1].Trim();
            Expression expr = new(pValue);
            Parameters[pName] = expr.Evaluate()?.ToString() ?? "";
        }

        foreach (string p in function.parameters)
        {
            if (!Parameters.ContainsKey(p))
            {
                Errors.Add(new(l, QuillErrorType.ParameterMismatch, $"Function call '{funcName}' missing parameter '{p}'"));
                return;
            }
        }

        Callbacks.Add(l);
        l = function.line;
    }

    private static async Task HandleSleep(string argsStr)
    {
        string[] args = argsStr.Split(',');
        if (args.Length != 1)
        {
            Errors.Add(new(l, QuillErrorType.ParameterMismatch, $"sleep command expects 1 argument, received {args.Length}"));
            return;
        }

        string waitTimeStr = args[0];
        Expression expr = new(waitTimeStr);
        var result = expr.Evaluate();
        if (result is int ms)
            await Task.Delay(ms);
        else
            Errors.Add(new(l, QuillErrorType.ParameterMismatch, $"Invalid sleep time: {waitTimeStr}"));
    }

    private static async Task HandleWait(string argsStr)
    {
        string[] args = argsStr.Split(',');
        if (args.Length != 2)
        {
            Errors.Add(new(l, QuillErrorType.ParameterMismatch, $"wait command expects 2 arguments, received {args.Length}"));
            return;
        }

        string conditionStr = args[0].Trim();
        string waitTimeStr = args[1];
        Expression expr = new(conditionStr);
        var result = expr.Evaluate();
        if (result is not bool b || !int.TryParse(waitTimeStr, out int waitTime))
        {
            Errors.Add(new(l, QuillErrorType.ParameterMismatch, $"Invalid wait command: wait {argsStr}"));
            return;
        }

        if (!b)
        {
            l--;
            await Task.Delay(waitTime);
        }
    }

    private static void HandleBuiltin(IBuiltinFunction func, string funcName, string argsStr)
    {
        string[] args = argsStr.Split(',', StringSplitOptions.TrimEntries);

        var resp = func.Run(args);
        if (!resp.Success)
            Errors.Add(new(l, QuillErrorType.UnknownError, $"Builtin function '{funcName}' failed: {resp.ErrorType} - {resp.ErrorMessage}"));
        else
            foreach (var kvp in resp.OutputVariables)
                Variables[kvp.Key] = kvp.Value;
    }

    [GeneratedRegex(@"\{([^}]*)\}", RegexOptions.Compiled)]
    private static partial Regex CurlyExpressions();
}