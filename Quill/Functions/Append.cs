namespace Quest.Quill.Functions;
// (list) (item)
public class Append : IBuiltinFunction
{
    public FunctionResponse Run(Dictionary<string, string> vars, string[] args)
    {
        if (args.Length != 2)
            return new(false, QuillErrorType.ParameterMismatch, $"Expected 2 parameters, got {args.Length}");

        vars["[return]"] = $"{args[0]};{args[1]}";
        return new(true);
    }
}
