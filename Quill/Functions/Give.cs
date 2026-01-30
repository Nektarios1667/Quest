namespace Quest.Quill.Functions;
// (item) (amount)
public class Give : IBuiltinFunction
{
    public FunctionResponse Run(Dictionary<string, string> _, string[] args)
    {
        if (args.Length != 2)
            return new(false, QuillErrorType.ParameterMismatch, $"Expected 2 parameters, got {args.Length}");
        if (CommandManager.Execute($"give {args[0]} {args[1]}").success)
            return new(true);
        return new(false, QuillErrorType.InvalidExpression, $"Failed to give {args[1]} '{args[0]}'");
    }
}
