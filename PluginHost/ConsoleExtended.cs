using System;

namespace PluginHost
{
    public static class ConsoleExtended
    {
        private const string INFO    = "[INFO]";
        private const string SUCCESS = "[SUCCESS]";
        private const string ALERT   = "[ALERT]";
        private const string WARN    = "[WARN]";
        private const string ERROR   = "[ERROR]";

        public static void Info(string message)
        {
            WriteOutput(ConsoleColor.Cyan, INFO, message);
        }

        public static void Succcess(string message)
        {
            WriteOutput(ConsoleColor.Green, SUCCESS, message);
        }

        public static void Alert(string message)
        {
            WriteOutput(ConsoleColor.Magenta, ALERT, message);
        }

        public static void Warn(string message)
        {
            WriteOutput(ConsoleColor.Yellow, WARN, message);
        }

        public static void Error(string message)
        {
            WriteOutput(ConsoleColor.Red, ERROR, message);
        }

        public static void Error(Exception ex)
        {
            WriteOutput(ConsoleColor.Red, ERROR, ex.Message);
        }

        private static void WriteOutput(ConsoleColor color, string type, string message)
        {
            Console.ForegroundColor = color;
            Console.WriteLine("{0,-15}{1}", type, message);
            Console.ResetColor();
        }
    }
}
