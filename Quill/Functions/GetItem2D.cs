using System.CodeDom;

namespace Quest.Quill.Functions;
// (list var) (x) (y)
public class GetItem2D : IBuiltinFunction
{
    public FunctionResponse Run(Dictionary<string, string> vars, string[] args)
    {
        // Checks
        if (args.Length != 3)
            return new(false, QuillErrorType.ParameterMismatch, $"Expected 3 parameters, got {args.Length}");
        if (!int.TryParse(args[1], out int x))
            return new(false, QuillErrorType.ParameterMismatch, $"Expected integer for second parameter, got '{args[1]}'");
        if (!int.TryParse(args[2], out int y))
            return new(false, QuillErrorType.ParameterMismatch, $"Expected integer for third parameter, got '{args[2]}'");

        // Get var
        if (!vars.TryGetValue(args[0], out string? value))
            return new(false, QuillErrorType.VariableNotFound, args[0]);
        args[0] = value;

        // Parse
        string[] lines = args[0].Split("/");
        if (y < 0 || y >= lines.Length)
            return new(false, QuillErrorType.OutOfBounds, $"Y index {y} is out of range for provided data");
        string[] items = lines[y].Split(";");

        if (x < 0 || x >= items.Length)
            return new(false, QuillErrorType.OutOfBounds, $"X index {x} is out of range for line {y}");
        string item = items[x];

        vars["[return]"] = item;
        return new(true);
    }
}
