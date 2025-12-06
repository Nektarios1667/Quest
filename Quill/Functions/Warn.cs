using NCalc;

namespace Quest.Quill.Functions;
public class Warn : IBuiltinFunction
{
    public FunctionResponse Run(string[] args)
    {
        if (args.Length != 1)
            return new(false, "ParameterMismatch", $"Expected 1 parameter, got {args.Length}");
        Expression expr = new(args[0]);
        object? output = args[0];
        try
        {
            output = expr.Evaluate();
        }
        catch { }
        Logger.Warning(output?.ToString() ?? string.Empty);
        return new(true);
    }
}
