using NCalc;
using Quest.Quill.Functions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Quest.Quill;
public static class Interpreter
{
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
    };
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
        Dictionary<string, string> variables = [];
        Dictionary<string, string> parameters = [];
        Dictionary<string, (int line, string[] parameters)> functions = [];
        List<int> callbacks = [];

        string[] lines = code.Split("\n");
        for (int l = 0; l < lines.Length; l++)
        {
            if (l < 0 || l >= lines.Length) break;
            string line = lines[l].Trim();

            // Comment
            if (line.StartsWith("//") || string.IsNullOrWhiteSpace(line)) continue;

            // Fill variables
            ReplaceVariables(ref line, variables);
            ReplaceVariables(ref line, parameters);
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

            // Variable declaration
            if (parts[0] == "num")
            {
                string varName = parts[1];
                if (ContainsAny(varName, """`~!@#$%^&*()-=+[]{}\|;:'",<.>/?"""))
                {
                    Console.WriteLine($"Invalid variable name: {varName}");
                    continue;
                }
                string varValue = parts[2];
                Expression expr = new(varValue);
                FillParameters(ref expr, variables);
                var result = expr.Evaluate();
                variables[varName] = result?.ToString() ?? "";
                continue;
            }
            else if (parts[0] == "str")
            {
                string varName = parts[1];
                if (ContainsAny(varName, """`~!@#$%^&*()-=+[]{}\|;:'",<.>/?"""))
                {
                    Console.WriteLine($"Invalid variable name: {varName}");
                    continue;
                }
                string varValue = parts[2];
                variables[varName] = varValue;
                continue;
            }
            else if (parts[0] == "breakwhile")
            {
                if (parts.Length < 2)
                    l = FindLine(lines, $"endwhile", l);
                else
                    l = FindLine(lines, $"endwhile {parts[1]}", l);
            }
            else if (parts[0] == "continuewhile")
            {
                if (parts.Length < 2)
                    l = FindLineBackwards(lines, $"while", l) - 1; // -1 because of the l++ at the end of the loop
                else
                    l = FindLineBackwards(lines, $"while {parts[1]}", l) - 1; // -1 because of the l++ at the end of the loop
            }
            else if (parts[0] == "if")
            {
                string name = parts[1];
                int conditionStart = 1;
                if (name.StartsWith('.')) conditionStart = 2;
                Expression expr = new(string.Join(' ', parts[conditionStart..]));
                var result = expr.Evaluate();
                if (result is bool b && !b)
                    l = FindLine(lines, $"endif {(name.StartsWith('.') ? name : "")}", l);
            }
            else if (parts[0] == "endif") { } // Marker
            else if (parts[0] == "while")
            {
                string name = parts[1];
                int conditionStart = 1;
                if (name.StartsWith('.')) conditionStart = 2;
                Expression expr = new(string.Join(' ', parts[conditionStart..]));
                FillParameters(ref expr, variables);
                var result = expr.Evaluate();
                if (result is bool b && !b)
                    l = FindLine(lines, $"endwhile {(name.StartsWith('.') ? name : "")}", l);
            }
            else if (parts[0] == "endwhile")
            {
                if (parts.Length < 2)
                    l = FindLineBackwards(lines, $"while", l) - 1; // -1 because of the l++ at the end of the loop
                else
                    l = FindLineBackwards(lines, $"while {parts[1]}", l) - 1; // -1 because of the l++ at the end of the loop
            }
            else if (parts[0] == "func")
            {
                string funcName = parts[1];
                string[] funcParams = parts.Length <= 2 ? [] : parts[2..];
                functions[funcName] = (l, funcParams);
                l = FindLine(lines, $"endfunc {funcName}", l);
                if (l == -1)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Function '{funcName}' missing endfunc");
                    Console.ResetColor();
                    break;
                }
            } // Skip function definition
            else if (parts[0] == "endfunc")
            {
                parameters.Clear();
                if (callbacks.Count > 0)
                {
                    l = callbacks[^1];
                    callbacks.RemoveAt(callbacks.Count - 1);
                }
            }
            // User functions
            else if (parts[0] == "call")
            {
                string funcName = parts[1];
                var function = functions.TryGetValue(funcName, out var f) ? f : (-1, []);
                if (function.line == -1)
                {
                    Console.WriteLine($"Function not found: {funcName}");
                    continue;
                }
                // Get parameters
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
                    FillParameters(ref expr, variables);
                    var result = expr.Evaluate();
                    parameters[pName] = result?.ToString() ?? "";
                }

                // Check params
                bool hasAllParams = true;
                foreach (string p in function.parameters)
                {
                    if (!parameters.ContainsKey(p))
                    {
                        Console.WriteLine($"Function call '{funcName}' missing parameter '{p}' @ l{l}");
                        hasAllParams = false;
                        break;
                    }
                }
                if (!hasAllParams) continue;

                callbacks.Add(l);
                l = function.line;
            }
            // Sleep
            else if (parts[0] == "sleep")
            {
                string waitTimeStr = parts[1];
                Expression expr = new(waitTimeStr);
                FillParameters(ref expr, variables);
                var result = expr.Evaluate();
                if (result is int ms)
                    await Task.Delay(ms);
                else
                    Console.WriteLine($"Invalid sleep time: {waitTimeStr}");
            }
            // Wait until
            else if (parts[0] == "wait")
            {
                string waitTimeStr = parts[1];
                string conditionStr = parts[2].Trim();
                Expression expr = new(conditionStr);
                FillParameters(ref expr, variables);
                var result = expr.Evaluate();
                if (result is not bool b || !int.TryParse(waitTimeStr, out int waitTime))
                {
                    Console.WriteLine($"Invalid wait command: {line}");
                    continue;
                }
                if (!b)
                {
                    l--;
                    await Task.Delay(waitTime);
                }
            }
            // Builtin functions
            else if (BuiltinFunctions.TryGetValue(parts[0], out var func))
            {
                // Get parameters
                string[] externalParams = parts.Length <= 1 ? [] : string.Join(' ', parts[1..]).Split(',', StringSplitOptions.TrimEntries);

                // Run
                var resp = func.Run(externalParams);
                if (!resp.Success)
                    Console.WriteLine($"Builtin function '{parts[0]}' failed: {resp.ErrorType} - {resp.ErrorMessage}");
                else
                    foreach (var kvp in resp.OutputVariables)
                        variables[kvp.Key] = kvp.Value;
            }
            else
            {
                Console.WriteLine($"Unknown command: {line}");
            }
        }
    }
}