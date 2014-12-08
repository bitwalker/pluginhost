using System;
using System.Configuration;
using System.Linq;
using System.Text;

namespace PluginHost.App.Logging
{
    using PluginHost.App.Configuration;
    using PluginHost.Extensions.Comparers;
    using PluginHost.Extensions.Enums;
    using PluginHost.Extensions.Text;
    using PluginHost.Interface.Logging;

    public class ConsoleLogger : ILogger
    {
        private static readonly GenericComparer<FormattingToken, string> _tokenComparer =
            GenericComparer<FormattingToken>.Create(t => t.Token);

        private bool _isEnabled;
        private LogLevel _level;

        public ConsoleLogger()
        {
            var config = Config.Current.Logging.Loggers["ConsoleLogger"];
            if (config == null)
                throw new ConfigurationErrorsException("Missing configuration for ConsoleLogger!");

            _level = config.LogLevel;
            _isEnabled = Config.Current.Logging.IsEnabled && _level > LogLevel.None;
        }

        public void Trace(string message)
        {
            WriteOutput(LogLevels.Trace, message);
        }

        public void Trace(string message, params object[] args)
        {
            WriteOutput(LogLevels.Trace, message, args);
        }

        public void Debug(string message)
        {
            WriteOutput(LogLevels.Debug, message);
        }

        public void Debug(string message, params object[] args)
        {
            WriteOutput(LogLevels.Debug, message, args);
        }

        public void Success(string message)
        {
            WriteOutput(LogLevels.Success, message);
        }

        public void Success(string message, params object[] args)
        {
            WriteOutput(LogLevels.Success, message, args);
        }

        public void Info(string message)
        {
            WriteOutput(LogLevels.Info, message);
        }

        public void Info(string message, params object[] args)
        {
            WriteOutput(LogLevels.Info, message, args);
        }

        public void Alert(string message)
        {
            WriteOutput(LogLevels.Alert, message);
        }

        public void Alert(string message, params object[] args)
        {
            WriteOutput(LogLevels.Alert, message, args);
        }

        public void Warn(string message)
        {
            WriteOutput(LogLevels.Warn, message);
        }

        public void Warn(string message, params object[] args)
        {
            WriteOutput(LogLevels.Warn, message, args);
        }

        public void Error(string message)
        {
            WriteOutput(LogLevels.Error, message);
        }

        public void Error(string message, params object[] args)
        {
            WriteOutput(LogLevels.Error, message, args);
        }

        public void Error(Exception ex)
        {
            if (!IsEnabled(LogLevels.Error)) return;

            var formatted = FormatException(ex);
            WriteOutput(LogLevels.Error, formatted);
        }

        public void Error(Exception ex, string message, params object[] args)
        {
            if (!IsEnabled(LogLevels.Error)) return;

            var formatted = FormatException(ex, message);
            WriteOutput(LogLevels.Error, formatted);
        }

        public void Fatal(string message)
        {
            WriteOutput(LogLevels.Fatal, message);
        }

        public void Fatal(string message, params object[] args)
        {
            WriteOutput(LogLevels.Fatal, message, args);
        }

        public void Fatal(Exception ex)
        {
            if (!IsEnabled(LogLevels.Fatal)) return;

            var formatted = FormatException(ex);
            WriteOutput(LogLevels.Fatal, formatted);
        }

        public void Fatal(Exception ex, string message, params object[] args)
        {
            if (!IsEnabled(LogLevels.Fatal)) return;

            var formatted = FormatException(ex, message);
            WriteOutput(LogLevels.Error, formatted);
        }

        private bool IsEnabled(LogLevel level)
        {
            return _isEnabled && level >= _level;
        }

        private void WriteOutput(LogLevel level, string output, params object[] args)
        {
            if (!IsEnabled(level)) return;

            var logLevel = level.GetName();

            Console.ForegroundColor = level.Color;

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

        private string FormatException(Exception ex, string message = "")
        {
            StringBuilder builder = null;
            if (string.IsNullOrWhiteSpace(message))
                builder = new StringBuilder();
            else
                builder = new StringBuilder(message + Environment.NewLine);

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
