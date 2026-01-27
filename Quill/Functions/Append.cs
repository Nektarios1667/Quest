namespace Quest.Quill.Functions;
public class Append : IBuiltinFunction
{
    public FunctionResponse Run(string[] args)
    {
        if (args.Length != 2)
            return new(false, "ParameterMismatch", $"Expected 2 parameters, got {args.Length}");

        return new(true, null, null, new() { { "[return]", $"{args[0]};{args[1]}"} });
    }
}
