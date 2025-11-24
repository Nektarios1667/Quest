using System;
using System.IO;

namespace Quest.Quill.Functions;
public class ReadFile : IBuiltinFunction
{
    public FunctionResponse Run(Dictionary<string, string> args)
    {
        if (!args.TryGetValue("path", out string? path))
            return new(false, "ParameterMismatch", $"Parameter 'path' is undefined");
        try
        {
            string content = File.ReadAllText(path);
            return new(true, null, null, new() {{ "[content]", $"'{content}'" }});
        }
        catch (FileNotFoundException)
        {
            return new(false, "FileNotFound", $"The file at path '{path}' was not found.");
        }
        catch (UnauthorizedAccessException)
        {
            return new(false, "AccessDenied", $"Access to the file at path '{path}' was denied.");
        }
        catch (IOException e)
        {
            return new(false, "IOError", $"An I/O error occurred while reading the file at path '{path}': {e.Message}");
        }
        catch (Exception e)
        {
            return new(false, "Error", e.Message);
        }
    }
}
