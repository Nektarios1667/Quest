using HarfBuzzSharp;
using NCalc;
using Quest.Quill.Functions;
using SharpDX.Direct2D1;
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
    public QuillCommand[] CompiledLines { get; set; } = [];
    public Dictionary<int, int> LinesVisited { get; private set; } = [];
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
        int allowedReps = PerformanceMode == QuillPerformanceMode.High ? 3 : 1;

        try
        {
            // Check sleeping
            if (IsSleeping)
            {
                SleepTimer -= game.DeltaTime * 1000; // Convert to ms
                if (IsSleeping)
                    return stepsUsed;
            }

            // Run lines - ensure enough step budget, not done, and line hasn't been run at all or above the allowance
            while (budget-- > 0 && !Done && (!LinesVisited.TryGetValue(L, out var reps) || reps < allowedReps))
            {
                // Run
                Interpreter.RunLine(this);
                stepsUsed++;
                // Optimization to not run same lines in one frame
                LinesVisited[L] = LinesVisited.TryGetValue(L, out reps) ? reps + 1 : 1;

                L++;
            }
        } catch (Exception e)
        {
            Errors.Add(new(L, QuillErrorType.RuntimeError, e.Message));
            L++;
        }
        LinesVisited.Clear();

        return stepsUsed;
    }
    public void Sleep(int ms) => SleepTimer = ms;
    public void SetPerformanceMode(QuillPerformanceMode mode) => PerformanceMode = mode;
}

public static partial class Interpreter
{
    private static readonly List<QuillInstance> Scripts = [];

    private static readonly Dictionary<string, IBuiltinFunction> BuiltinFunctions  = new() {
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
    public static IReadOnlyDictionary<string, IBuiltinFunction> GetBuiltinFunctions() => BuiltinFunctions;

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
        ExternalSymbols["<tilebelow>"] = player.TileBelow?.Type.Texture.ToString() ?? "NUL";
        ExternalSymbols["<camera_x>"] = CameraManager.Camera.X.ToString();
        ExternalSymbols["<camera_y>"] = CameraManager.Camera.Y.ToString();
        ExternalSymbols["<camera>"] = $"{CameraManager.Camera.X};{CameraManager.Camera.Y}";
        // Level
        ExternalSymbols["<currentlevel>"] = game.LevelManager.Level.Name.WrapSingleQuotes();
        ExternalSymbols["<currentlevelname>"] = game.LevelManager.Level.LevelName.WrapSingleQuotes();
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
    private static void ReplaceVariables(string[] args, Dictionary<string, string> vars)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (!args[i].Contains('=')) continue;
            foreach (var kvp in vars)
                args[i] = args[i].Replace("=" + kvp.Key, kvp.Value);
        }
    }
    public static void EvaluateCurlyExpressions(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (!(args[i].Contains('{') && args[i].Contains('}'))) continue;
            args[i] = CurlyExpressions().Replace(args[i], match =>
            {
                string exprStr = match.Groups[1].Value.Trim();
                Expression expr = new(exprStr);
                var result = expr.Evaluate();

                return result?.ToString()?.ToLower() ?? "";
            });
        }
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
        var inst = new QuillInstance(script);
        Compiler.CompileScript(inst);

        Scripts.Add(inst);
    }

    public static void Update(GameManager game, PlayerManager player)
    {
        if (Scripts.Count == 0) return;

        UpdateSymbols(game, player);

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
            steps = Math.Clamp(steps, 1, Math.Min(budget, Constants.QuillScriptMaxUpdatesPerFrame));

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
        QuillCommand command = instance.CompiledLines[instance.L];

        // Handle errors
        if (instance.Errors.Count > 0)
            OutputErrors(instance);

        // Check noop
        if (command.Operation == QuillOp.NoOp) return;

        // Fill variables and expressions
        string[] args = [.. command.Args];

        if (command.HasVariables)
        {
            ReplaceVariables(args, instance.Locals);
            ReplaceVariables(args, instance.Variables);
            ReplaceVariables(args, ExternalSymbols);
        }
        if (command.HasCurlyExpressions)
            EvaluateCurlyExpressions(args);

        ExecuteCommand(command.Operation, args, instance);
    }
    public static void ExecuteCommand(QuillOp op, string[] args, QuillInstance instance)
    {
        switch (op)
        {
            case QuillOp.PerfMode: HandlePerfMode(instance, args); break;
            case QuillOp.Num: HandleNum(instance, args); break;
            case QuillOp.Str: HandleStr(instance, args); break;
            case QuillOp.BreakWhile: HandleBreakWhile(instance, args); break;
            case QuillOp.ContinueWhile: HandleContinueWhile(instance, args); break;
            case QuillOp.If: HandleIf(instance, args); break;
            case QuillOp.EndIf: break;
            case QuillOp.While: HandleWhile(instance, args); break;
            case QuillOp.EndWhile: HandleEndWhile(instance, args); break;
            case QuillOp.Func: HandleFunc(instance, args); break;
            case QuillOp.EndFunc: HandleEndFunc(instance, args); break;
            case QuillOp.Sleep: HandleSleep(instance, args); break;
            case QuillOp.Wait: HandleWait(instance, args); break;
            case QuillOp.Only: HandleOnly(instance, args); break;
            case QuillOp.EndOnly: break;
            case QuillOp.Return: HandleReturn(instance, args); break;
            case QuillOp.BuiltinFuncCall:
                if (BuiltinFunctions.TryGetValue(args[0], out var builtinFunc))
                    HandleBuiltin(instance, builtinFunc, args);
                break;
            case QuillOp.CustomFuncCall:
                if (instance.Functions.TryGetValue(args[0], out var func))
                    HandleCall(instance, func, args);
                break;
            default: instance.Errors.Add(new(instance.L, QuillErrorType.UnknownCommand, op.ToString())); break;
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

    [GeneratedRegex(@"\{([^}]*)\}", RegexOptions.Compiled)]
    private static partial Regex CurlyExpressions();
}