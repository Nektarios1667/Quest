using System.Linq;

namespace Quest.Quill.Functions;
// (list / 2d-array) (target)
public class Contains : IBuiltinFunction
{
    public FunctionResponse Run(Dictionary<string, string> vars, string[] args)
    {
        if (args.Length != 2)
            return new(false, QuillErrorType.ParameterMismatch, $"Expected 2 parameters, got {args.Length}");
        string[] lines = args[0].Trim('\'').Split("/");

        for (int l = 0; l < lines.Length; l++)
        {
            string[] items = lines[l].Split(";");
            if (items.Contains(args[1]))
            {
                vars["[return]"] = "true";
                return new(true);
            }
        }
        vars["[return]"] = "false";
        return new(true);
    }
}
