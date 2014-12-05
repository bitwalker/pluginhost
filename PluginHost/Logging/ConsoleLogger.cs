using System;
using System.Linq;
using System.Text;

using PluginHost.Extensions.Comparers;
using PluginHost.Extensions.Enums;
using PluginHost.Extensions.Text;
using PluginHost.Interface.Logging;

namespace PluginHost.Logging
{
    public class ConsoleLogger : ILogger
    {
        private static class ConsoleColors
        {
            public static readonly ConsoleColor Trace   = ConsoleColor.Gray;
            public static readonly ConsoleColor Info    = ConsoleColor.Cyan;
            public static readonly ConsoleColor Success = ConsoleColor.Green;
            public static readonly ConsoleColor Warn    = ConsoleColor.Yellow;
            public static readonly ConsoleColor Alert   = ConsoleColor.Magenta;
            public static readonly ConsoleColor Error   = ConsoleColor.Red;
        }

        private static readonly GenericComparer<FormattingToken, string> _tokenComparer =
            GenericComparer<FormattingToken>.Create(t => t.Token);

        public void Trace(string message)
        {
            WriteOutput(LogLevel.TRACE, ConsoleColors.Trace, message);
        }

        public void Trace(string message, params object[] args)
        {
            WriteOutput(LogLevel.TRACE, ConsoleColors.Trace, message, args);
        }

        public void Info(string message)
        {
            WriteOutput(LogLevel.INFO, ConsoleColors.Info, message);
        }

        public void Info(string message, params object[] args)
        {
            WriteOutput(LogLevel.INFO, ConsoleColors.Info, message, args);
        }

        public void Success(string message)
        {
            WriteOutput(LogLevel.SUCCESS, ConsoleColors.Success, message);
        }

        public void Success(string message, params object[] args)
        {
            WriteOutput(LogLevel.SUCCESS, ConsoleColors.Success, message, args);
        }

        public void Warn(string message)
        {
            WriteOutput(LogLevel.WARN, ConsoleColors.Warn, message);
        }

        public void Warn(string message, params object[] args)
        {
            WriteOutput(LogLevel.WARN, ConsoleColors.Warn, message, args);
        }

        public void Alert(string message)
        {
            WriteOutput(LogLevel.ALERT, ConsoleColors.Alert, message);
        }

        public void Alert(string message, params object[] args)
        {
            WriteOutput(LogLevel.ALERT, ConsoleColors.Alert, message, args);
        }

        public void Error(string message)
        {
            WriteOutput(LogLevel.ERROR, ConsoleColors.Error, message);
        }

        public void Error(string message, params object[] args)
        {
            WriteOutput(LogLevel.ERROR, ConsoleColors.Error, message, args);
        }

        public void Error(Exception ex)
        {
            var formatted = FormatException(ex);
            WriteOutput(LogLevel.ERROR, ConsoleColors.Error, formatted);
        }

        public void Error(Exception ex, string message, params object[] args)
        {
            WriteOutput(LogLevel.ERROR, ConsoleColors.Error, message, args);

            var formatted = FormatException(ex);
            WriteOutput(LogLevel.ERROR, ConsoleColors.Error, formatted);
        }

        private void WriteOutput(LogLevel level, ConsoleColor color, string output, params object[] args)
        {
            var logLevel = level.GetName();

            Console.ForegroundColor = color;

            // Prepend log level to each line of the output
            var lines = output.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var formattedLine = FormatLine(line, args);
                Console.WriteLine("{0,-15}{1}", "[" + logLevel + "]", formattedLine);
            }

            Console.ResetColor();
        }

        private string FormatLine(string line, object[] args)
        {
            if (args == null || !args.Any())
                return line;

            var tokens = line.ExtractFormatTokens();
            foreach (var token in tokens.Distinct(_tokenComparer))
            {
                line = line.Replace(token.Token, args[token.ArgsIndex].ToString());
            }

            return line;
        }

        private string FormatException(Exception ex)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("[EXCEPTION] {0}", ex.Message);
            builder.AppendLine();


            return FormatException(ex.InnerException, builder);
        }

        private string FormatException(Exception ex, StringBuilder builder, int depth = 1)
        {
            if (ex == null)
                return builder.ToString();

            builder.AppendFormat("[EXCEPTION (INNER {0})] {1}", depth, ex.Message);
            builder.AppendLine();
            builder = AppendStackTrace(builder, ex.StackTrace);

            return FormatException(ex.InnerException, builder, ++depth);
        }

        private StringBuilder AppendStackTrace(StringBuilder builder, string stackTrace)
        {
            // Indent the stack trace to provide better visual structure
            var lines = stackTrace
                .Split(new [] {'\r','\n'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var stackLine in lines)
            {
                builder.AppendFormat("\t{0}", stackLine);
                builder.AppendLine();
            }

            return builder;
        }
    }
}
