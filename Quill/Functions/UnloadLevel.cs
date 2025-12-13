namespace Quest.Quill.Functions;
public class UnloadLevel : IBuiltinFunction
{
    public FunctionResponse Run(string[] args)
    {
        if (args.Length != 1)
            return new(false, "ParameterMismatch", $"Expected 1 parameter, got {args.Length}");
        if (CommandManager.Execute($"level unload {args[0]}").success)
            return new(true);
        return new(false, "CommandError", $"Failed to unload level '{args[0]}'");
    }
}
