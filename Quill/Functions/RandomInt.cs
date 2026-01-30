namespace Quest.Quill.Functions;
public class RandomInt : IBuiltinFunction
{
    public FunctionResponse Run(Dictionary<string, string> vars, string[] args)
    {
        if (args.Length != 2)
            return new(false, QuillErrorType.ParameterMismatch, $"Expected 2 parameters, got {args.Length}");
        if (!int.TryParse(args[0], out int min))
            return new(false, QuillErrorType.ParameterMismatch, $"Expected integer for first parameter, got '{args[0]}'");
        if (!int.TryParse(args[1], out int max))
            return new(false, QuillErrorType.ParameterMismatch, $"Expected integer for second parameter, got '{args[1]}'");
        if (min > max)
            return new(false, QuillErrorType.InvalidExpression, $"Minimum value {min} cannot be greater than maximum value {max}");
        int randNum = new Random().Next(min, max + 1);

        vars["[return]"] = randNum.ToString();
        return new(true);
    }
}
