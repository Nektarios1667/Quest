using System.IO;

namespace Quest.Utilities;

public static class FileTools
{
    public static void CheckFileExists(string path)
    {
        if (File.Exists(path)) return;
        Logger.Warning($"File {path} not found. Creating new file.");
        File.Create(path);
    }
    public static void CheckDirExists(string path)
    {
        if (Directory.Exists(path)) return;
        Logger.Warning($"Directory {path} not found. Creating new directory.");
        Directory.CreateDirectory(path);
    }
}
