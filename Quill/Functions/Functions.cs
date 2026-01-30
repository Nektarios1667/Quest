namespace Quest.Quill.Functions;

public struct FunctionResponse
{
    public bool Success;
    public QuillErrorType? ErrorType;
    public string? ErrorMessage;
    public FunctionResponse(bool success, QuillErrorType? errorType = null, string? errorMessage = null)
    {
        Success = success;
        ErrorType = errorType;
        ErrorMessage = errorMessage;
    }
}

public interface IBuiltinFunction
{
    public FunctionResponse Run(Dictionary<string, string> variables, string[] args);
}
