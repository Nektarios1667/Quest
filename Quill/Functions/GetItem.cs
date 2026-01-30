namespace Quest.Quill.Functions;

// (list var) (index)
public class GetItem : IBuiltinFunction
{
    public FunctionResponse Run(Dictionary<string, string> vars, string[] args)
    {
        if (args.Length != 2)
            return new(false, QuillErrorType.ParameterMismatch, $"Expected 2 parameters, got {args.Length}");
        if (!int.TryParse(args[1], out int idx))
            return new(false, QuillErrorType.ParameterMismatch, $"Expected integer for second parameter, got '{args[1]}'");

        // Get var
        if (!vars.TryGetValue(args[0], out string? value))
            return new(false, QuillErrorType.VariableNotFound, args[0]);
        args[0] = value;

        // Parse
        string[] items = args[0].Split(";");
        if (idx < 0 || idx >= items.Length)
            return new(false, QuillErrorType.OutOfBounds, $"Index {idx} is out of range");
        string item = items[idx];

        vars["[return"] = item;
        return new(true);
    }
}
