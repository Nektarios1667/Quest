using NCalc;
using Quest.Quill.Functions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Quest.Quill;

public static partial class Interpreter
{
    private readonly static List<QuillError> Errors = [];
    private static int l = 0;
    private static readonly Dictionary<string, string> Variables = [];
    private static readonly Dictionary<string, string> Locals = [];
    private static readonly Dictionary<string, (int line, string[] parameters)> Functions = [];
    private static readonly List<int> Callbacks = [];
    private static readonly Stack<string> Scopes = [];
    private static readonly Dictionary<int, int> OnceFlags = [];
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
        Variables.Clear();
        Locals.Clear();
        Functions.Clear();
        Callbacks.Clear();
        OnceFlags.Clear();
        Scopes.Clear();

        Lines = script.SourceCode.Split("\n");
        for (l = 0; l < Lines.Length; l++)
        {
            if (l < 0 || l >= Lines.Length)
                Errors.Add(new(l, QuillErrorType.RuntimeError, $"Line index out of bounds - {l}", fatal: true));
            string line = Lines[l].Trim();

            // Handle errors
            foreach (QuillError error in Errors)
            {
                Logger.Error($"Quill | {script.ScriptName} @{error.Line + 1} | {error.ErrorType} - {error.Message}");
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
            ReplaceVariables(ref line, Locals);
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
            await Task.Delay(1);
        }
    }
    public static async Task ExecuteCommand(string line)
    {
        string command = line.Split(' ')[0].Trim();
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
            case "sleep": await HandleSleep(argsStr); break;
            case "wait": await HandleWait(argsStr); break;
            case "only": HandleOnly(argsStr); break;
            case "endonly": break; // Marker
            case "return": HandleReturn(argsStr); break;
            default:
                if (BuiltinFunctions.TryGetValue(command, out var builtinFunc))
                    HandleBuiltin(builtinFunc, command, argsStr);
                else if (Functions.TryGetValue(command, out var func))
                    HandleCall(func, command, argsStr);
                else
                    Errors.Add(new(l, QuillErrorType.UnknownCommand, command));
                break;
        }
    }
    // Handlers
    private static void HandleNum(string argsStr)
    {
        // Args
        string[] args = argsStr.Split(',', StringSplitOptions.TrimEntries);
        if (args.Length != 2)
        {
            Errors.Add(new(l, QuillErrorType.ParameterMismatch, $"num command expects 2 arguments, received {args.Length}"));
            return;
        }

        // Name
        string varName = args[0];
        if (ContainsAny(varName, "`~!@#$%^&*()-=+[]{}\\|;:'\",<.>/?"))
        {
            Errors.Add(new(l, QuillErrorType.InvalidName, varName));
            return;
        }

        // Value
        if (!float.TryParse(args[1], out float num))
        {
            Errors.Add(new(l, QuillErrorType.InvalidExpression, $"Invalid number expression '{args[1]}'"));
            return;
        }
        if (Scopes.Count == 0)
            Variables[varName] = num.ToString("F20").TrimEnd('0').TrimEnd('.');
        else
            Locals[varName] = num.ToString("F20").TrimEnd('0').TrimEnd('.');
    }

    private static void HandleStr(string argsStr)
    {
        // Args
        string[] args = argsStr.Split(',', StringSplitOptions.TrimEntries);
        if (args.Length != 2)
        {
            Errors.Add(new(l, QuillErrorType.ParameterMismatch, $"str command expects 2 arguments, received {args.Length}"));
            return;
        }

        // Name
        string varName = args[0];
        if (ContainsAny(varName, "`~!@#$%^&*()-=+[]{}\\|;:'\",<.>/?"))
        {
            Errors.Add(new(l, QuillErrorType.InvalidName, varName));
            return;
        }

        // Value
        if (Scopes.Count == 0)
            Variables[varName] = args[1];
        else
            Locals[varName] = args[1];
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
        l = FindLine(Lines, "endfunc", l);
    }

    private static void HandleEndFunc()
    {
        Locals.Clear();
        if (Callbacks.Count > 0)
        {
            l = Callbacks[^1];
            Callbacks.RemoveAt(Callbacks.Count - 1);
        }
        if (Scopes.Count > 0)
            Scopes.Pop();   
    }

    private static void HandleCall((int line, string[] parameters) function, string funcName, string argsStr)
    {
        string[] args = argsStr.Split(',', StringSplitOptions.TrimEntries);

        if (function.line == -1)
        {
            Errors.Add(new(l, QuillErrorType.FunctionNotFound, funcName));
            return;
        }

        foreach (string param in args)
        {
            string[] kvp = param.Split(':');
            if (kvp.Length != 2)
            {
                Errors.Add(new(l, QuillErrorType.InvalidExpression, $"Invalid parameter: {param}"));
                continue;
            }

            string pName = kvp[0].Trim();
            string pValue = kvp[1].Trim();
            Locals[pName] = pValue;
        }

        foreach (string p in function.parameters)
        {
            if (!Locals.ContainsKey(p))
            {
                Errors.Add(new(l, QuillErrorType.ParameterMismatch, $"Function call '{funcName}' missing parameter '{p}'"));
                return;
            }
        }

        Scopes.Push(funcName);
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
        if (int.TryParse(waitTimeStr, out var ms))
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
    private static void HandleOnly(string argsStr)
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
            Errors.Add(new(l, QuillErrorType.ParameterMismatch, $"sleep command expects 0, 1, or 2 arguments, received {args.Length}"));
            return;
        }

        // Check limit
        if (limit <= 0)
        {
            Errors.Add(new(l, QuillErrorType.ParameterMismatch, $"Invalid only amount"));
            return;
        }

        // Check flag
        if (!OnceFlags.TryGetValue(l, out int counter))
            OnceFlags[l] = 1;
        else if (counter < limit)
            OnceFlags[l] = ++counter;
        else
            l = FindLine(Lines, $"endonly {label}", l);
    }
    private static void HandleReturn(string argsStr)
    {
        string[] args = argsStr.Split(' ');
        if (args.Length == 1)
        {
            Variables["[return]"] = args[0];
        }
        else if (args.Length != 0)
        {
            Errors.Add(new(l, QuillErrorType.ParameterMismatch, $"return command expects 0 or 1 arguments, received {args.Length}"));
            return;
        }

        l = FindLine(Lines, "endfunc", l) - 1;
    }

    private static void HandleBuiltin(IBuiltinFunction func, string funcName, string argsStr)
    {
        string[] args = argsStr.Split(',', StringSplitOptions.TrimEntries);

        var resp = func.Run(Variables, args);
        if (!resp.Success)
            Errors.Add(new(l, resp.ErrorType!.Value, resp.ErrorMessage!));
    }

    [GeneratedRegex(@"\{([^}]*)\}", RegexOptions.Compiled)]
    private static partial Regex CurlyExpressions();
}