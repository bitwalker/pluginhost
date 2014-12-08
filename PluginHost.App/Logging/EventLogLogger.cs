using System;
using System.Configuration;
using System.Text;
using System.Diagnostics;
using System.Reflection;

namespace PluginHost.App.Logging
{
    using PluginHost.App.Configuration;
    using PluginHost.Interface.Logging;

    /// <summary>
    /// Logs to the Windows Event Log, under the Application log as PluginHost.
    /// </summary>
    public class EventLogLogger : ILogger
    {
        // The event log to write to. There is really no good
        // reason to use anything other than the application log.
        private const string EVENT_LOG = "Application";
        // The name of the event source representing events from this application.
        private readonly string _eventSource;
        private readonly LogLevel _level;
        private bool _isEnabled;

        public EventLogLogger()
        {
            var config = Config.Current.Logging.Loggers["EventLogLogger"];
            if (config == null)
                throw new ConfigurationErrorsException("Missing configuration for EventLogLogger!");

            _eventSource = Assembly.GetEntryAssembly().GetName().Name;
            _level       = config.LogLevel;
            _isEnabled   = Config.Current.Logging.IsEnabled && _level > LogLevel.None;

            InitializeEventLog();
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

        public void Warn(string message)
        {
            WriteOutput(LogLevels.Warn, message);
        }

        public void Warn(string message, params object[] args)
        {
            WriteOutput(LogLevels.Warn, message, args);
        }

        public void Alert(string message)
        {
            WriteOutput(LogLevels.Alert, message);
        }

        public void Alert(string message, params object[] args)
        {
            WriteOutput(LogLevels.Alert, message, args);
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

            var formatted = FormatException(ex);
            var joined = string.Format(message, args) + Environment.NewLine + formatted;
            WriteOutput(LogLevels.Error, joined);
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

            var formatted = FormatException(ex);
            var joined = string.Format(message, args) + Environment.NewLine + formatted;
            WriteOutput(LogLevels.Fatal, joined);
        }

        private bool IsEnabled(LogLevel level)
        {
            return _isEnabled && level >= _level;
        }

        private void WriteOutput(LogLevel level, string output, params object[] args)
        {
            if (!IsEnabled(level)) return;

            var formatted = string.Format(output, args);

            try
            {
                if (level < LogLevels.Warn)
                {
                    var infoType = EventLogEntryType.Information;
                    EventLog.WriteEntry(_eventSource, formatted, infoType, (int) level, (short) level);
                    return;
                }
                else if (level < LogLevels.Error)
                {
                    var warnType = EventLogEntryType.Warning;
                    EventLog.WriteEntry(_eventSource, formatted, warnType, (int) level, (short) level);
                    return;
                }
                else
                {
                    var errorType = EventLogEntryType.Error;
                    EventLog.WriteEntry(_eventSource, formatted, errorType, (int) level, (short) level);
                    return;
                }
            }
            catch (Exception ex)
            {
                // Write to the trace listener that this failed, just in case anything is listening
                System.Diagnostics.Trace.TraceError("EventLogLogger: Failed to write entry - {0}", ex.Message);
            }
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
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var stackLine in lines)
            {
                builder.AppendFormat("\t{0}", stackLine);
                builder.AppendLine();
            }

            return builder;
        }

        private void InitializeEventLog()
        {
            if (!EventLog.SourceExists(_eventSource))
                EventLog.CreateEventSource(_eventSource, EVENT_LOG);
        }
    }
}
