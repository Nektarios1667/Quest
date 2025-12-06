namespace Quest.Quill.Functions;
public class Teleport : IBuiltinFunction
{
    public FunctionResponse Run(string[] args)
    {
        if (args.Length != 2)
            return new(false, "ParameterMismatch", $"Expected 2 parameters, got {args.Length}");
        if (!int.TryParse(args[0], out int x))
            return new(false, "ParameterMismatch", $"Expected integer for first parameter, got {args[0]}");
        if (!int.TryParse(args[1], out int y))
            return new(false, "ParameterMismatch", $"Expected integer for second parameter, got {args[1]}");
        CommandManager.Execute($"teleport {x},{y}");
        return new(true);
    }
}
