using System.CodeDom;
using System.Linq;

namespace Quest.Quill.Functions;
public class Remove : IBuiltinFunction
{
    public FunctionResponse Run(Dictionary<string, string> vars, string[] args)
    {
        if (args.Length != 2)
            return new(false, QuillErrorType.ParameterMismatch, $"Expected 3 parameters, got {args.Length}");
        if (!int.TryParse(args[1], out int idx))
            return new(false, QuillErrorType.ParameterMismatch, $"Expected second parameter to be an integer, got {args[1]}");

        // Get var
        if (!vars.TryGetValue(args[0], out string? value))
            return new(false, QuillErrorType.VariableNotFound, args[0]);
        args[0] = value;

        // Items
        string[] items = args[0].Split(';');
        if (idx < 0 || idx >= items.Length)
            return new(false, QuillErrorType.OutOfBounds, $"Index {idx} is out of range for items of length {items.Length}");
        
        string result;
        if (idx == items.Length - 1)
            result = string.Join(";", items[..^1]);
        else
            result = string.Join(";", items[..idx].Concat(items[(idx + 1)..]));

        vars["[return]"] = result;
        return new(true);
    }
}
