using NCalc;
using System;
using System.IO;

namespace Quest.Quill.Functions;
public class Log : IBuiltinFunction
{
    public FunctionResponse Run(Dictionary<string, string> args)
    {
        if (!args.TryGetValue("output", out string? cmd))
            return new(false, "ParameterMismatch", $"Parameter 'output' is undefined");
        Expression expr = new(cmd);
        object? output = cmd;
        try
        {
            output = expr.Evaluate();
        }
        catch { }
        Logger.Log(output?.ToString() ?? string.Empty);
        return new(true);
    }
}
