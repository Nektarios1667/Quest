namespace Quest.Quill.Functions;
public class Execute : IBuiltinFunction
{
    public FunctionResponse Run(string[] args)
    {
        if (args.Length != 1)
            return new(false, "ParameterMismatch", $"Expected 1 parameter, got {args.Length}");
        var (success, output) = CommandManager.Execute(args[0]);
        if (!success)
            return new(false, "CommandError", output);
        return new(true, null, null, new() { { "[output]", $"'{output}'" } });
    }
}
