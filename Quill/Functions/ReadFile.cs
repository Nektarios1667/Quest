using System.IO;

namespace Quest.Quill.Functions;
public class ReadFile : IBuiltinFunction
{
    public FunctionResponse Run(Dictionary<string, string> vars, string[] args)
    {
        if (args.Length != 1)
            return new(false, QuillErrorType.ParameterMismatch, $"Expected 1 parameter, got {args.Length}");
        string path = args[0];
        try
        {
            string content = File.ReadAllText(path);
            vars["[return]"] = content;
            return new(true);
        }
        catch (FileNotFoundException)
        {
            return new(false, QuillErrorType.IOError, $"The file at path '{path}' was not found.");
        }
        catch (UnauthorizedAccessException)
        {
            return new(false, QuillErrorType.IOError, $"Access to the file at path '{path}' was denied.");
        }
        catch (IOException e)
        {
            return new(false, QuillErrorType.IOError, $"An I/O error occurred while reading the file at path '{path}': {e.Message}");
        }
        catch (Exception e)
        {
            return new(false, QuillErrorType.UnknownError, e.Message);
        }
    }
}
