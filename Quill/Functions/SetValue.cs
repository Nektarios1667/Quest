namespace Quest.Quill.Functions;
// (dict) (key) (value)
public class SetValue : IBuiltinFunction
{
    public FunctionResponse Run(Dictionary<string, string> vars, string[] args)
    {
        if (args.Length != 3)
            return new(false, QuillErrorType.ParameterMismatch, $"Expected 3 parameters, got {args.Length}");

        // Create dict
        Dictionary<string, string> dict = [];
        foreach (string pair in args[0].Split('/'))
        {
            // Get key-value
            string[] kv = pair.Split(':', 2);
            if (kv.Length != 2)
                return new(false, QuillErrorType.InvalidExpression, $"Bad dictionary pair '{kv}'");

            dict[kv[0]] = kv[1];
        }
        // Check key exists
        if (!dict.ContainsKey(args[1]))
            return new(false, QuillErrorType.KeyNotFound, $"Dictionary does not contain key '{args[1]}'");
        // Set value
        dict[args[1]] = args[2];
        // Reconstruct
        string modified = Utilities.DictToQict(dict);

        vars["[return]"] = modified;
        return new(true);
    }
}
