using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest
{
    public static class Logger
    {
        private static readonly List<string> MessageLevels = ["Input", "Log", "Warning", "Error"];
        private static readonly List<ConsoleColor> MessageColors = [ConsoleColor.Cyan, ConsoleColor.Blue, ConsoleColor.Yellow, ConsoleColor.Red];
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
                Console.WriteLine($"[{type}] {message}");
                Console.ForegroundColor = ConsoleColor.White; // Reset color
            }
            else
                Console.WriteLine($"[Unknown] {message}");
        }
        public static string Input(string message)
        {
            Console.ForegroundColor = MessageColors[MessageLevels.IndexOf("Input")];
            Console.WriteLine($"[Input] {message}");
            Console.ForegroundColor = ConsoleColor.White; // Reset color
            return Console.ReadLine();
        }
        public static void Print(string message)
        {
            Console.WriteLine(message);
        }
        public static void Log(string message)
        {
            Console.ForegroundColor = MessageColors[MessageLevels.IndexOf("Log")];
            Console.WriteLine($"[Log] {message}");
            Console.ForegroundColor = ConsoleColor.White; // Reset color
        }
        public static void Warning(string message)
        {
            Console.ForegroundColor = MessageColors[MessageLevels.IndexOf("Warning")];
            Console.WriteLine($"[Warning] {message}");
            Console.ForegroundColor = ConsoleColor.White; // Reset color
        }
        public static void Error(string message, bool exit = false)
        {
            Console.ForegroundColor = MessageColors[MessageLevels.IndexOf("Error")];
            Console.WriteLine($"[Error] {message}");
            Console.ForegroundColor = ConsoleColor.White; // Reset color
            if (exit) Environment.Exit(1);
        }
    }
}
