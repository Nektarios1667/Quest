using System;
using System.IO;

namespace Quest.Quill.Functions;
public class Execute : IBuiltinFunction
{
    public FunctionResponse Run(Dictionary<string, string> args)
    {
        if (!args.TryGetValue("command", out string? cmd))
            return new(false, "ParameterMismatch", $"Parameter 'command' is undefined");
        var (success, output) = CommandManager.Execute(cmd);
        if (!success)
            return new(false, "CommandError", output);
        return new(true, null, null, new() { { "[output]", $"'{output}'" } });
    }
}
