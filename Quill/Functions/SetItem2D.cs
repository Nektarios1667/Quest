using System.Drawing.Text;

namespace Quest.Quill.Functions;
public class SetItem2D : IBuiltinFunction
{
    public FunctionResponse Run(Dictionary<string, string> vars, string[] args)
    {
        if (args.Length != 4)
            return new(false, QuillErrorType.ParameterMismatch, $"Expected 4 parameters, got {args.Length}");
        if (!int.TryParse(args[1], out int x))
            return new(false, QuillErrorType.ParameterMismatch, $"Expected integer for second parameter, got '{args[1]}'");
        if (!int.TryParse(args[2], out int y))
            return new(false, QuillErrorType.ParameterMismatch, $"Expected integer for third parameter, got '{args[2]}'");

        // Get var
        if (!vars.TryGetValue(args[0], out string? value))
            return new(false, QuillErrorType.VariableNotFound, args[0]);
        args[0] = value;

        // Get 2d array
        string[] lines = args[0].Split("/");
        string[][] items = new string[lines.Length][];
        for (int l = 0; l < lines.Length; l++)
        {
            items[l] = lines[l].Split(";");
        }

        // Replace
        if (y < 0 || y >= items.Length)
            return new(false, QuillErrorType.OutOfBounds, $"Y Index {y} is out of range");
        if (x < 0 || x >= items[y].Length)
            return new(false, QuillErrorType.OutOfBounds, $"X Index {x} is out of range");
        items[y][x] = args[3];

        // Reconstruct 2d array
        List<string> reconstructedLines = [];
        for (int l = 0; l < items.Length; l++)
            reconstructedLines.Add(string.Join(";", items[l]));
        string finalResult = string.Join("/", reconstructedLines);

        vars["[return]"] = finalResult;
        return new(true);
    }
}
