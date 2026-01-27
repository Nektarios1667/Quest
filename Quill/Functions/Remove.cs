using System.Linq;

namespace Quest.Quill.Functions;
public class Remove : IBuiltinFunction
{
    public FunctionResponse Run(string[] args)
    {
        if (args.Length != 2)
            return new(false, "ParameterMismatch", $"Expected 3 parameters, got {args.Length}");
        if (!int.TryParse(args[1], out int idx))
            return new(false, "TypeMismatch", $"Expected second parameter to be an integer, got {args[1]}");

        // Items
        string[] items = args[0].Split(';');
        if (idx < 0 || idx >= items.Length)
            return new(false, "IndexOutOfRange", $"Index {idx} is out of range for items of length {items.Length}");
        
        string result;
        if (idx == items.Length - 1)
            result = string.Join(";", items[..^1]);
        else
            result = string.Join(";", items[..idx].Concat(items[(idx + 1)..]));

        return new(true, null, null, new() { { "[return]", $"{result}"} });
    }
}
