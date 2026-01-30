using System.Drawing.Drawing2D;

namespace Quest.Quill.Functions;
// (dict var) (key)
public class GetValue : IBuiltinFunction
{
    public FunctionResponse Run(Dictionary<string, string> vars, string[] args)
    {
        if (args.Length != 2)
            return new(false, QuillErrorType.ParameterMismatch, $"Expected 2 parameters, got {args.Length}");

        // Get var
        if (!vars.TryGetValue(args[0], out string? value))
            return new(false, QuillErrorType.VariableNotFound, args[0]);
        args[0] = value;

        // Find key
        string[] pairs = args[0].Split("/");
        foreach (string pair in pairs)
        {
            string[] kv = pair.Split(':');
            if (kv.Length != 2)
                return new(false, QuillErrorType.ParameterMismatch, $"Bad dictionary pair '{pair}'");
            if (kv[0] == args[1])
            {
                vars["[return]"] = kv[1];
                return new(true);
            }
        }

        vars["[return]"] = "NUL";
        return new(true);
    }
}
