using HarfBuzzSharp;
using NCalc;
using System;
using System.IO;

namespace Quest.Quill.Functions;
public class ReadLevel : IBuiltinFunction
{
    public FunctionResponse Run(string[] args)
    {
        if (args.Length != 1)
            return new(false, "ParameterMismatch", $"Expected 1 parameter, got {args.Length}");
        if (CommandManager.Execute($"level read {args[0]}").success)
            return new(true);
        return new(false, "CommandError", $"Failed to read level '{args[0]}'");
    }
}
