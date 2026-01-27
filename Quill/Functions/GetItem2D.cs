namespace Quest.Quill.Functions;
public class GetItem2D : IBuiltinFunction
{
    public FunctionResponse Run(string[] args)
    {
        // Checks
        if (args.Length != 3)
            return new(false, "ParameterMismatch", $"Expected 3 parameters, got {args.Length}");
        if (!int.TryParse(args[1], out int x))
            return new(false, "TypeMismatch", $"Expected integer for second parameter, got '{args[1]}'");
        if (!int.TryParse(args[2], out int y))
            return new(false, "TypeMismatch", $"Expected integer for third parameter, got '{args[2]}'");

        // Parse
        string[] lines = args[0].Split("/");
        if (y < 0 || y >= lines.Length)
            return new(false, "IndexOutOfRange", $"Y index {y} is out of range for provided data");
        string[] items = lines[y].Split(";");

        if (x < 0 || x >= items.Length)
            return new(false, "IndexOutOfRange", $"X index {x} is out of range for line {y}");
        string item = items[x];

        return new(true, null, null, new() { { "[return]", $"{item}" } });
    }
}
