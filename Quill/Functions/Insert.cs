namespace Quest.Quill.Functions;
public class Insert : IBuiltinFunction
{
    public FunctionResponse Run(Dictionary<string, string> vars, string[] args)
    {
        if (args.Length != 3)
            return new(false, QuillErrorType.ParameterMismatch, $"Expected 3 parameters, got {args.Length}");
        if (!int.TryParse(args[1], out int idx))
            return new(false, QuillErrorType.ParameterMismatch, $"Expected second parameter to be an integer, got {args[1]}");

        // Items
        string[] items = args[0].Split(';');
        if (idx < 0 || idx >= items.Length)
            return new(false, QuillErrorType.OutOfBounds, $"Index {idx} is out of range for items of length {items.Length}");
        string result = string.Join(';', items[..idx]) + (idx > 0 ? ";" : "") + args[2] + (idx < items.Length ? ";" : "") + string.Join(';', items[idx..]);

        vars["[return]"] = result;
        return new(true);
    }
}
