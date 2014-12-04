using System;
using System.Text;
using System.Diagnostics;
using System.Reflection;

using PluginHost.Interface.Logging;

namespace PluginHost.Logging
{
    /// <summary>
    /// Logs to the Windows Event Log, under the Application log as PluginHost.
    /// </summary>
    public class EventLogLogger : ILogger
    {
        // The event log to write to. There is really no good
        // reason to use anything other than the application log.
        private const string EVENT_LOG = "Application";
        // The name of the event source representing events from
        // this application. Defaults to the name of this application's
        // entry assembly/executable, otherwise uses the name provided
        // via the secondary constructor.
        private readonly string _eventSource;

        public EventLogLogger()
            : this(Assembly.GetEntryAssembly().GetName().Name) {}

        public EventLogLogger(string eventSourceName)
        {
            if (string.IsNullOrWhiteSpace(eventSourceName))
                throw new ArgumentException("EventLogLogger: Event source name cannot be null or empty!");

            _eventSource = eventSourceName;

            InitializeEventLog();
        }

        public void Trace(string message)
        {
            WriteOutput(LogLevel.TRACE, message);
        }

        public void Trace(string message, params object[] args)
        {
            WriteOutput(LogLevel.TRACE, message, args);
        }

        public void Info(string message)
        {
            WriteOutput(LogLevel.INFO, message);
        }

        public void Info(string message, params object[] args)
        {
            WriteOutput(LogLevel.INFO, message, args);
        }

        public void Success(string message)
        {
            WriteOutput(LogLevel.SUCCESS, message);
        }

        public void Success(string message, params object[] args)
        {
            WriteOutput(LogLevel.SUCCESS, message, args);
        }

        public void Warn(string message)
        {
            WriteOutput(LogLevel.WARN, message);
        }

        public void Warn(string message, params object[] args)
        {
            WriteOutput(LogLevel.WARN, message, args);
        }

        public void Alert(string message)
        {
            WriteOutput(LogLevel.ALERT, message);
        }

        public void Alert(string message, params object[] args)
        {
            WriteOutput(LogLevel.ALERT, message, args);
        }

        public void Error(string message)
        {
            WriteOutput(LogLevel.ERROR, message);
        }

        public void Error(string message, params object[] args)
        {
            WriteOutput(LogLevel.ERROR, message, args);
        }

        public void Error(Exception ex)
        {
            var formatted = FormatException(ex);
            WriteOutput(LogLevel.ERROR, formatted);
        }

        public void Error(Exception ex, string message, params object[] args)
        {
            var formatted = FormatException(ex);
            var joined = string.Format(message, args) + Environment.NewLine + formatted;
            WriteOutput(LogLevel.ERROR, joined);
        }

        private void WriteOutput(LogLevel level, string output, params object[] args)
        {
            var formatted = string.Format(output, args);

            try
            {
                switch (level)
                {
                    case LogLevel.TRACE:
                    case LogLevel.INFO:
                    case LogLevel.SUCCESS:
                        var infoType = EventLogEntryType.Information;
                        EventLog.WriteEntry(_eventSource, formatted, infoType, (int) level, (short) level);
                        break;
                    case LogLevel.WARN:
                    case LogLevel.ALERT:
                        var warnType = EventLogEntryType.Warning;
                        EventLog.WriteEntry(_eventSource, formatted, warnType, (int) level, (short) level);
                        break;
                    case LogLevel.ERROR:
                    case LogLevel.EXCEPTION:
                    default:
                        var errorType = EventLogEntryType.Error;
                        EventLog.WriteEntry(_eventSource, formatted, errorType, (int) level, (short) level);
                        break;
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
