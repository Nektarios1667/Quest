namespace Quest.Quill.Functions;

public struct FunctionResponse
{
    public bool Success;
    public string? ErrorType;
    public string? ErrorMessage;
    public Dictionary<string, string> OutputVariables = [];
    public FunctionResponse(bool success, string? errorType = null, string? errorMessage = null, Dictionary<string, string>? outputVariables = null)
    {
        Success = success;
        ErrorType = errorType;
        ErrorMessage = errorMessage;
        OutputVariables = outputVariables ?? [];
    }
}

public interface IBuiltinFunction
{
    public FunctionResponse Run(string[] args);
}
