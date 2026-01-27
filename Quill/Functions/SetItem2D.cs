using System.Drawing.Text;

namespace Quest.Quill.Functions;
public class SetItem2D : IBuiltinFunction
{
    public FunctionResponse Run(string[] args)
    {
        if (args.Length != 4)
            return new(false, "ParameterMismatch", $"Expected 4 parameters, got {args.Length}");
        if (!int.TryParse(args[1], out int x))
            return new(false, "TypeMismatch", $"Expected integer for second parameter, got '{args[1]}'");
        if (!int.TryParse(args[2], out int y))
            return new(false, "TypeMismatch", $"Expected integer for third parameter, got '{args[2]}'");

        // Get 2d array
        string[] lines = args[0].Split("/");
        string[][] items = new string[lines.Length][];
        for (int l = 0; l < lines.Length; l++)
        {
            items[l] = lines[l].Split(";");
        }

        // Replace
        if (y < 0 || y >= items.Length)
            return new(false, "IndexOutOfRange", $"Y Index {y} is out of range");
        if (x < 0 || x >= items[y].Length)
            return new(false, "IndexOutOfRange", $"X Index {x} is out of range");
        items[y][x] = args[3];

        // Reconstruct 2d array
        List<string> reconstructedLines = [];
        for (int l = 0; l < items.Length; l++)
            reconstructedLines.Add(string.Join(";", items[l]));
        string finalResult = string.Join("/", reconstructedLines);

        return new(true, null, null, new() { { "[return]", $"{finalResult}"} });
    }
}
