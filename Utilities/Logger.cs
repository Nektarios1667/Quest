﻿namespace Quest.Utilities;

public static class Logger
{
    private static readonly List<string> MessageLevels = ["Input", "System", "Log", "Warning", "Error"];
    private static readonly List<ConsoleColor> MessageColors = [ConsoleColor.Cyan, ConsoleColor.Blue, ConsoleColor.Green, ConsoleColor.Yellow, ConsoleColor.Red];
    private static string timestamp => DateTime.Now.ToString("HH:mm:ss.fff");
    public static void CreateLevel(string levelName, ConsoleColor color)
    {
        if (!MessageLevels.Contains(levelName))
        {
            MessageLevels.Add(levelName);
            MessageColors.Add(color);
        }
    }
    public static void Output(string message, string type)
    {
        if (MessageLevels.Contains(type))
        {
            Console.ForegroundColor = MessageColors[MessageLevels.IndexOf(type)];
            Console.WriteLine($"({timestamp}) [{type}] {message}");
            Console.ForegroundColor = ConsoleColor.White; // Reset color
        }
        else
            Console.WriteLine($"[Unknown] {message}");
    }
    public static int InputInt(string message, int fallback = 0)
    {
        string resp = Input(message);
        if (int.TryParse(resp, out int result))
            return result;
        else
        {
            Error("Invalid input- expected an integer.");
            return fallback; // Default value or handle as needed
        }
    }
    public static byte InputByte(string message, byte fallback = 0)
    {
        int resp = InputInt(message);
        if (resp < 0 || resp > 255) return fallback;
        return (byte)resp;
    }
    public static TextureID InputTexture(string message, TextureID fallback = TextureID.Null)
    {
        string resp = Input(message);
        if (Enum.TryParse(resp, true, out TextureID texture) && TextureManager.Metadata[texture].Type == "character")
            return texture;
        else
        {
            Error("Invalid input- expected a valid character texture name.");
            return fallback;
        }
    }
    public static Color InputColor(string message, Color fallback = default)
    {
        string resp = Input(message);
        string[] parts = resp.Split(',');
        if (parts.Length == 3)
        {
            if (int.TryParse(parts[0], out int r) && int.TryParse(parts[1], out int g) && int.TryParse(parts[2], out int b))
                return new Color(r, g, b);
        }
        else if (parts.Length == 4)
        {
            if (int.TryParse(parts[0], out int r) && int.TryParse(parts[1], out int g) && int.TryParse(parts[2], out int b) && int.TryParse(parts[3], out int a))
                return new Color(r, g, b, a);
        }
        Error("Invalid input- expected r,g,b or r,g,b,a color");
        return fallback;
    }
    public static string Input(string message)
    {
        Console.ForegroundColor = MessageColors[MessageLevels.IndexOf("Input")];
        Console.WriteLine($"({timestamp}) [Input] {message}");
        Console.ForegroundColor = ConsoleColor.White; // Reset color
        return Console.ReadLine() ?? "";
    }
    public static void Print(string message)
    {
        Console.WriteLine(message);
    }
    public static void System(string message)
    {
        Console.ForegroundColor = MessageColors[MessageLevels.IndexOf("System")];
        Console.WriteLine($"({timestamp}) [System] {message}");
        Console.ForegroundColor = ConsoleColor.White; // Reset color
    }
    public static void Log(string message)
    {
        if (!DebugManager.LogInfo) return; // Skip logging if not enabled

        Console.ForegroundColor = MessageColors[MessageLevels.IndexOf("Log")];
        Console.WriteLine($"({timestamp}) [Log] {message}");
        Console.ForegroundColor = ConsoleColor.White; // Reset color
    }
    public static void Warning(string message)
    {
        Console.ForegroundColor = MessageColors[MessageLevels.IndexOf("Warning")];
        Console.WriteLine($"({timestamp}) [Warning] {message}");
        Console.ForegroundColor = ConsoleColor.White; // Reset color
    }
    public static void Error(string message, bool exit = false)
    {
        Console.ForegroundColor = MessageColors[MessageLevels.IndexOf("Error")];
        Console.WriteLine($"({timestamp}) [Error] {message}");
        Console.ForegroundColor = ConsoleColor.White; // Reset color
        if (exit) Environment.Exit(1);
    }
}
