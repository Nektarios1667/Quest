using Quest.Quill.Functions;
using ScottPlot.TickGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Quill;

public enum QuillOp {
    // Noop
    NoOp,

    // Interpreter
    PerfMode,

    // Control flow
    If,
    EndIf,
    While,
    EndWhile,
    BreakWhile,
    ContinueWhile,
    Return,

    // Variable
    Str,
    Num,

    // Function
    Func,
    EndFunc,

    // Timing
    Sleep,
    Wait,
    Only,
    EndOnly,

    // Func calls
    BuiltinFuncCall,
    CustomFuncCall,
}
public class QuillCommand
{
    public QuillOp Operation { get; set; }
    public string[] Args { get; set; }
    public bool HasVariables { get; }
    public bool HasCurlyExpressions { get; }
    public override string ToString() => $"{Operation} {string.Join(' ', Args)}";
    public QuillCommand(QuillOp op, string[] args)
    {
        Operation = op;
        Args = args;

        string argsStr = string.Join("", args);
        HasVariables = argsStr.Contains('=');
        HasCurlyExpressions = argsStr.Contains('{') && argsStr.Contains('}');
    }
}

public static class Compiler
{
    public static void CompileScript(QuillInstance inst)
    {
        // Setup
        List<QuillCommand> commands = [];

        // Compile lines
        for (int l = 0; l < inst.Lines.Length; l++)
        {
            string line = inst.Lines[l].Trim();

            // NoOp
            if (line.StartsWith("//", StringComparison.Ordinal) || string.IsNullOrWhiteSpace(line))
            {
                commands.Add(new QuillCommand(QuillOp.NoOp, []));
                continue;
            }

            // Parse parts
            string command = line.Split(' ')[0];
            string[] args = line[command.Length..].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            command = command.Trim().Trim('#');

            // Check functions
            if (string.Equals(command, "func", StringComparison.OrdinalIgnoreCase))
            {
                if (args.Length == 0)
                {
                    inst.Errors.Add(new(l + 1, QuillErrorType.ParameterMismatch, "func command expects at least 1 argument, received 0"));
                    commands.Add(new QuillCommand(QuillOp.Func, []));
                    continue;
                }

                // Record function and add NoOp
                inst.Functions[args[0]] = (line: l, args.Length > 1 ? args[1..] : []);
                commands.Add(new(QuillOp.Func, args));
                continue;
            }

            // Parse line
            if (inst.Functions.ContainsKey(command)) // Custom func
                commands.Add(new(QuillOp.CustomFuncCall, [command, ..args]));
            else if (Interpreter.GetBuiltinFunctions().ContainsKey(command)) // Builtin func
                commands.Add(new(QuillOp.BuiltinFuncCall, [command, .. args]));
            else if (Enum.TryParse<QuillOp>(command, true, out var op))
                commands.Add(new(op, args));
            else
            {
                inst.Errors.Add(new(l + 1, QuillErrorType.UnknownCommand, line));
                commands.Add(new(QuillOp.NoOp, args));
            }
        }

        inst.CompiledLines = [.. commands];
    }
}
