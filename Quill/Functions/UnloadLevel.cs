namespace Quest.Quill.Functions;
public class UnloadLevel : IBuiltinFunction
{
    public FunctionResponse Run(Dictionary<string, string> _, string[] args)
    {
        if (args.Length != 1)
            return new(false, QuillErrorType.ParameterMismatch, $"Expected 1 parameter, got {args.Length}");
        if (CommandManager.Execute($"level unload {args[0]}").success)
            return new(true);
        return new(false, QuillErrorType.RuntimeError, $"Failed to unload level '{args[0]}'");
    }
}
