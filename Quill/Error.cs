using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Quill;
public enum QuillErrorType
{
    UnknownError,
    SyntaxError,
    RuntimeError,
    ParameterMismatch,
    UnknownCommand,
    FunctionNotFound,
    VariableNotFound,
    InvalidName,
    InvalidExpression,
    BlockMismatch,
    OutOfBounds,
    KeyNotFound,
    IOError,
    Fatal,
}
public struct QuillError
{
    public int Line { get; }
    public QuillErrorType ErrorType { get; }
    public string Message { get; }
    public bool Fatal { get; }
    public QuillError(int line, QuillErrorType errorType, string message, bool fatal = false)
    {
        Line = line;
        ErrorType = errorType;
        Message = message;
        Fatal = fatal;
    }
}