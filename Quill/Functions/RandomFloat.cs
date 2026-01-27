using SharpDX;

namespace Quest.Quill.Functions;
public class RandomFloat : IBuiltinFunction
{
    public FunctionResponse Run(string[] args)
    {
        if (args.Length != 0)
            return new(false, "ParameterMismatch", $"Expected 0 parameters, got {args.Length}");
        if (!float.TryParse(args[0], out float min))
            return new(false, "TypeMismatch", $"Expected float for first parameter, got '{args[0]}'");
        if (!float.TryParse(args[1], out float max))
            return new(false, "TypeMismatch", $"Expected float for second parameter, got '{args[1]}'");
        if (min > max)
            return new(false, "ValueError", $"Minimum value {min} cannot be greater than maximum value {max}");
        float randNum = new Random().NextFloat(min, max);
        return new(true, null, null, new() { { "[return]", $"{randNum}" } });
    }
}
