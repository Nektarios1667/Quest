using NCalc;

namespace Quest.Quill.Functions;
public class Log : IBuiltinFunction
{
    public FunctionResponse Run(string[] args)
    {
        if (args.Length != 1)
            return new(false, "ParameterMismatch", $"Expected 1 parameter, got {args.Length}");
        Logger.Log(args[0]);
        return new(true);
    }
}
