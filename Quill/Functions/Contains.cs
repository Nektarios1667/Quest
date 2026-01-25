using System.Linq;

namespace Quest.Quill.Functions;
public class Contains : IBuiltinFunction
{
    public FunctionResponse Run(string[] args)
    {
        if (args.Length != 2)
            return new(false, "ParameterMismatch", $"Expected 2 parameters, got {args.Length}");
        string[] lines = args[0].Trim('\'').Split("/");

        for (int l = 0; l < lines.Length; l++)
        {
            string[] items = lines[l].Split(";");
            if (items.Contains(args[1]))
                return new(true, outputVariables: new() { { "[return]", "true" } });
        }
        return new(true, outputVariables: new() { { "[return]", "false" } });
    }
}
