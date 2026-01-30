namespace Quest.Quill.Functions;
public class Notif : IBuiltinFunction
{
    public FunctionResponse Run(Dictionary<string, string> _, string[] args)
    {
        if (args.Length != 5)
            return new(false, QuillErrorType.ParameterMismatch, $"Expected 5 parameters, got {args.Length}");

        // Checks
        if (!int.TryParse(args[0], out int r))
            return new(false, QuillErrorType.ParameterMismatch, $"Failed to parse parameter 2 '{args[1]}' as integer");
        if (!int.TryParse(args[1], out int g))
            return new(false, QuillErrorType.ParameterMismatch, $"Failed to parse parameter 3 '{args[2]}' as integer");
        if (!int.TryParse(args[2], out int b))
            return new(false, QuillErrorType.ParameterMismatch, $"Failed to parse parameter 4 '{args[3]}' as integer");
        if (!float.TryParse(args[3], out float duration))
            return new(false, QuillErrorType.ParameterMismatch, $"Failed to parse parameter 5 '{args[4]}' as float");

        if (CommandManager.Execute($"notif {r} {g} {b} {duration} {args[4]}").success)
            return new(true);
        return new(false, QuillErrorType.RuntimeError, $"Failed to create notif '{args[4]}'");
    }
}
