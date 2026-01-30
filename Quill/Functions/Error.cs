using NCalc;

namespace Quest.Quill.Functions;
// (message) (exit?)
public class Error : IBuiltinFunction
{
    public FunctionResponse Run(Dictionary<string, string> _, string[] args)
    {
        if (args.Length != 2)
            return new(false, QuillErrorType.ParameterMismatch, $"Expected 2 parameters, got {args.Length}");
        if (!bool.TryParse(args[1], out bool exit))
            return new(false, QuillErrorType.ParameterMismatch, $"Expected second parameter to be a boolean, got {args[1]}");
        Expression expr = new(args[0]);
        object? output = args[0];
        try
        {
            output = expr.Evaluate();
        }
        catch { }
        Logger.Error(output?.ToString() ?? string.Empty, exit);
        return new(true);
    }
}
