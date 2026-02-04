using NCalc;
using Quest.Quill.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Quill;
public static partial class Interpreter
{
    // Handlers
    private static void HandlePerfMode(QuillInstance inst, string[] args)
    {
        if (args.Length != 1)
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"#perfmode expects 1 argument, received {args.Length}"));
            return;
        }
        if (!int.TryParse(args[0], out int value) || value < 0 || value > 2)
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"Invalid perfmode: {args[0]}"));
            return;
        }

        // Set performance mode
        inst.SetPerformanceMode(Enum.Parse<QuillPerformanceMode>(args[0]));
    }
    private static void HandleNum(QuillInstance inst, string[] args)
    {
        if (args.Length != 2)
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"num command expects 2 arguments, received {args.Length}"));
            return;
        }

        // Name
        string varName = args[0];
        if (ContainsAny(varName, "`~!@#$%^&*()-=+[]{}\\|;:'\",<.>/?"))
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.InvalidName, varName));
            return;
        }

        // Value
        if (!float.TryParse(args[1], out float num))
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.InvalidExpression, $"Invalid number expression '{args[1]}'"));
            return;
        }
        if (inst.Scopes.Count == 0)
            inst.Variables[varName] = num.ToString("F20").TrimEnd('0').TrimEnd('.');
        else
            inst.Locals[varName] = num.ToString("F20").TrimEnd('0').TrimEnd('.');
    }
    private static void HandleStr(QuillInstance inst, string[] args)
    {
        if (args.Length != 2)
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"str command expects 2 arguments, received {args.Length}"));
            return;
        }

        // Name
        string varName = args[0];
        if (ContainsAny(varName, "`~!@#$%^&*()-=+[]{}\\|;:'\",<.>/?"))
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.InvalidName, varName));
            return;
        }

        // Value
        if (inst.Scopes.Count == 0)
            inst.Variables[varName] = args[1];
        else
            inst.Locals[varName] = args[1];
    }
    private static void HandleBreakWhile(QuillInstance inst, string[] args)
    {
        if (args.Length == 1)
            inst.L = FindLine(inst, QuillOp.EndWhile, label: args[0], start: inst.L);
        else if (args.Length == 0)
            inst.L = FindLine(inst, QuillOp.EndWhile, start: inst.L);
        else
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"breakwhile command expects 0 or 1 arguments, received {args.Length}"));
    }
    private static void HandleContinueWhile(QuillInstance inst, string[] args)
    {
        if (args.Length == 1)
            inst.L = FindLineBackwards(inst, QuillOp.While, label: args[0], start: inst.L) - 1;
        else if (args.Length == 0)
            inst.L = FindLineBackwards(inst, QuillOp.While, start: inst.L) - 1;
        else
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"continuewhile command expects 0 or 1 arguments, received {args.Length}"));
    }
    private static void HandleIf(QuillInstance inst, string[] args)
    {
        if (args.Length >= 2 && args[0].StartsWith('.'))
        {
            string label = args[0];
            Expression expr = new(string.Join(' ', args[1..]));
            var result = expr.Evaluate();
            if (result is bool b && !b)
                inst.L = FindLine(inst, QuillOp.EndIf, label: label, start: inst.L);
        }
        else if (args.Length >= 1)
        {
            Expression expr = new(string.Join(' ', args[0..]));
            var result = expr.Evaluate();
            if (result is bool b && !b)
                inst.L = FindLine(inst, QuillOp.EndIf, start: inst.L);
        }
        else
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"if command expects 1 or 2 arguments, received {args.Length}"));
    }
    private static void HandleWhile(QuillInstance inst, string[] args)
    {
        if (args.Length >= 2 && args[0].StartsWith('.'))
        {
            string label = args[0];
            Expression expr = new(string.Join(' ', args[1..]));
            var result = expr.Evaluate();
            if (result is bool b && !b)
                inst.L = FindLine(inst, QuillOp.EndWhile, label:label, start: inst.L);
        }
        else if (args.Length >= 1)
        {
            Expression expr = new(string.Join(' ', args[0..]));
            var result = expr.Evaluate();
            if (result is bool b && !b)
                inst.L = FindLine(inst, QuillOp.EndWhile, start: inst.L);
        }
        else
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"while command expects 1 or 2 arguments, received {args.Length}"));
    }
    private static void HandleEndWhile(QuillInstance inst, string[] args)
    {
        if (args.Length == 0)
            inst.L = FindLineBackwards(inst, QuillOp.While, start: inst.L) - 1;
        else if (args.Length == 1)
            inst.L = FindLineBackwards(inst, QuillOp.While, label: args[0], start: inst.L) - 1;
        else
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"endwhile command expects 0 or 1 arguments, received {args.Length}"));

        // Delay
        if (inst.PerformanceMode == QuillPerformanceMode.Low)
            inst.Sleep(1000); // ms
        else if (inst.PerformanceMode == QuillPerformanceMode.Normal)
            inst.Sleep(100); // ms
    }
    private static void HandleFunc(QuillInstance inst, string[] args)
    {
        inst.L = FindLine(inst, QuillOp.EndFunc, start: inst.L);
    }
    private static void HandleEndFunc(QuillInstance inst, string[] args)
    {
        inst.Locals.Clear();
        if (inst.Callbacks.Count > 0)
        {
            inst.L = inst.Callbacks[^1];
            inst.Callbacks.RemoveAt(inst.Callbacks.Count - 1);
        }
        if (inst.Scopes.Count > 0)
            inst.Scopes.Pop();
    }
    private static void HandleSleep(QuillInstance inst, string[] args)
    {
        if (args.Length != 1)
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"sleep command expects 1 argument, received {args.Length}"));
            return;
        }

        if (int.TryParse(args[0], out var ms))
            inst.Sleep(ms);
        else
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"Invalid sleep time: {args[0]}"));
    }
    private static void HandleWait(QuillInstance inst, string[] args)
    {
        if (args.Length != 2)
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"wait command expects 2 arguments, received {args.Length}"));
            return;
        }

        string conditionStr = args[0].Trim();
        string waitTimeStr = args[1];
        Expression expr = new(conditionStr);
        var result = expr.Evaluate();
        if (result is not bool b || !int.TryParse(waitTimeStr, out int waitTime))
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"Invalid wait command: wait {string.Join(',', args)}"));
            return;
        }

        if (!b)
        {
            inst.L--;
            inst.Sleep(waitTime);
        }
    }
    private static void HandleOnly(QuillInstance inst, string[] args)
    {
        string label = "";
        int limit;

        if (args.Length == 2 && args[0].StartsWith('.'))
        {
            label = args[0];
            limit = int.TryParse(args[1], out int v) ? v : -1;
        }
        else if (args.Length == 1 && args[0].StartsWith('.'))
        {
            label = args[0];
            limit = 1;
        }
        else if (args.Length == 1)
            limit = int.TryParse(args[0], out int v) ? v : -1;
        else if (args.Length == 0)
            limit = 1;
        else
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"sleep command expects 0, 1, or 2 arguments, received {args.Length}"));
            return;
        }

        // Check limit
        if (limit <= 0)
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"Invalid only amount"));
            return;
        }

        // Check flag
        if (!inst.OnceFlags.TryGetValue(inst.L, out int counter))
            inst.OnceFlags[inst.L] = 1;
        else if (counter < limit)
            inst.OnceFlags[inst.L] = ++counter;
        else
            inst.L = FindLine(inst, QuillOp.EndOnly, label: label, start: inst.L);
    }
    private static void HandleReturn(QuillInstance inst, string[] args)
    {
        if (args.Length == 1)
        {
            inst.Variables["[return]"] = args[0];
        }
        else if (args.Length != 0)
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"return command expects 0 or 1 arguments, received {args.Length}"));
            return;
        }

        inst.L = FindLine(inst, QuillOp.EndFunc, start: inst.L) - 1;
    }
    private static void HandleCall(QuillInstance inst, (int line, string[] parameters) function, string[] args)
    {
        if (function.line < 0)
        {
            inst.Errors.Add(new(inst.L, QuillErrorType.FunctionNotFound, args[0]));
            return;
        }

        // Parse parameters
        string[] parameters = args.Length > 1 ? args[1..] : [];
        foreach (string param in parameters)
        {
            string[] kvp = param.Split(':');
            if (kvp.Length != 2)
            {
                inst.Errors.Add(new(inst.L, QuillErrorType.InvalidExpression, $"Invalid parameter: {param}"));
                continue;
            }

            string pName = kvp[0].Trim();
            string pValue = kvp[1].Trim();
            inst.Locals[pName] = pValue;
        }

        // Check parameters match
        foreach (string p in function.parameters)
        {
            if (!inst.Locals.ContainsKey(p))
            {
                inst.Errors.Add(new(inst.L, QuillErrorType.ParameterMismatch, $"Function call '{args[0]}' missing parameter '{p}'"));
                return;
            }
        }

        // Go to function
        inst.Scopes.Push(args[0]);
        inst.Callbacks.Add(inst.L);
        inst.L = function.line;
    }
    private static void HandleBuiltin(QuillInstance inst, IBuiltinFunction func, string[] args)
    {
        var resp = func.Run(inst.Variables, args.Length > 1 ? args[1..] : []);
        if (!resp.Success)
            inst.Errors.Add(new(inst.L, resp.ErrorType!.Value, resp.ErrorMessage!));
    }
}
