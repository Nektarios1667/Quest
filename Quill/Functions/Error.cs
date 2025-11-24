using NCalc;
using System;
using System.IO;

namespace Quest.Quill.Functions;
public class Error : IBuiltinFunction
{
    public FunctionResponse Run(Dictionary<string, string> args)
    {
        if (!args.TryGetValue("output", out string? cmd))
            return new(false, "ParameterMismatch", $"Parameter 'output' is undefined");
        if (!args.TryGetValue("exit", out string? exitStr))
            return new(false, "ParameterMismatch", $"Parameter 'exit' is undefined");
        if (!bool.TryParse(exitStr, out bool exit))
            return new(false, "ParameterMismatch", $"Parameter 'exit' is not a valid boolean");
        Expression expr = new(cmd);
        object? output = cmd;
        try
        {
            output = expr.Evaluate();
        }
        catch { }
        Logger.Error(output?.ToString() ?? string.Empty, exit);
        return new(true);
    }
}
