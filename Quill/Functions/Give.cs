using HarfBuzzSharp;
using NCalc;
using System;
using System.IO;

namespace Quest.Quill.Functions;
public class Give : IBuiltinFunction
{
    public FunctionResponse Run(string[] args)
    {
        if (args.Length != 2)
            return new(false, "ParameterMismatch", $"Expected 2 parameters, got {args.Length}");
        if (CommandManager.Execute($"give {args[0]} {args[1]}").success)
            return new(true);
        return new(false, "CommandError", $"Failed to give {args[1]} '{args[0]}'");
    }
}
