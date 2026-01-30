namespace Quest.Quill.Functions;
// (command)
public class Execute : IBuiltinFunction
{
    public FunctionResponse Run(Dictionary<string, string> _, string[] args)
    {
        if (args.Length != 1)
            return new(false, QuillErrorType.ParameterMismatch, $"Expected 1 parameter, got {args.Length}");
        var (success, output) = CommandManager.Execute(args[0]);
        if (!success)
            return new(false, QuillErrorType.InvalidExpression, output);
        return new(true, null, null, new() { { "[return]", $"'{output}'" } });
    }
}
