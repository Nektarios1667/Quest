using NCalc;
using System;
using System.IO;

namespace Quest.Quill.Functions;
public class Warn : IBuiltinFunction
{
    public FunctionResponse Run(Dictionary<string, string> args)
    {
        if (!args.TryGetValue("output", out string? cmd))
            Logger.Error("Warn function missing 'output' parameter.", true);
        Expression expr = new(cmd);
        object? output = cmd;
        try
        {
            output = expr.Evaluate();
        }
        catch { }
        Logger.Warning(output?.ToString() ?? string.Empty);
        return new(true);
    }
}
