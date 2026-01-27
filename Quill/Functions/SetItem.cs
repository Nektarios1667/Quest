namespace Quest.Quill.Functions;
public class SetItem : IBuiltinFunction
{
    public FunctionResponse Run(string[] args)
    {
        if (args.Length != 3)
            return new(false, "ParameterMismatch", $"Expected 3 parameters, got {args.Length}");
        if (!int.TryParse(args[1], out int idx))
            return new(false, "TypeMismatch", $"Expected integer for second parameter, got '{args[1]}'");

        string[] items = args[0].Split(";");
        if (idx < 0 || idx >= items.Length)
            return new(false, "IndexOutOfRange", $"Index {idx} is out of range");
        items[idx] = args[2];

        return new(true, null, null, new() { { "[return]", $"{string.Join(';', items)}"} });
    }
}
