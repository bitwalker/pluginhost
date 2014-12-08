using System;
using System.Linq;
using System.Collections.Generic;

namespace PluginHost.Interface.Logging
{
    public class LogLevels : IEnumerable<LogLevel>
    {
        private static LogLevels _instance;
        private static readonly IDictionary<string, LogLevel> _levels = new Dictionary<string, LogLevel>()
        {
            { Trace.Name.ToUpperInvariant(), Trace },     { Debug.Name.ToUpperInvariant(), Debug},
            { Success.Name.ToUpperInvariant(), Success }, { Info.Name.ToUpperInvariant(), Info },
            { Alert.Name.ToUpperInvariant(), Alert },     { Warn.Name.ToUpperInvariant(), Warn },
            { Error.Name.ToUpperInvariant(), Error },     { Fatal.Name.ToUpperInvariant(), Fatal }
        };

        /// <summary>
        /// Exposes available logging levels as a query source
        /// </summary>
        public static LogLevels All { get { return _instance ?? (_instance = new LogLevels()); } }


        /// <summary>
        /// Get a log level by name
        /// </summary>
        /// <param name="name">The name of the logging level, or one of it's aliases.</param>
        /// <returns>LogLevel</returns>
        public LogLevel this[string name]
        {
            get
            {
                // First look up by name
                var nameUpper = name.ToUpperInvariant();
                if (_levels.ContainsKey(nameUpper))
                    return _levels[nameUpper];

                // Try looking up by aliases, returning LogLevel.None as the default value
                // if the alias search has no matches
                return _levels
                    .Select(kvp => kvp.Value)
                    .FirstOrDefault(level =>
                        level.Aliases.Contains(nameUpper, StringComparer.InvariantCultureIgnoreCase)
                    );
            }
        }

        /// <summary>
        /// Get a log level based on the provided ordinal value
        /// </summary>
        /// <param name="ordinal">The ordinal value of the logging level to look up</param>
        /// <returns>LogLevel</returns>
        public LogLevel this[int ordinal]
        {
            get
            {
                // Less than 0 represents disabled logging
                if (ordinal < 0)
                    return LogLevel.None;
                // Values greater than the max value may represent another
                // logging systems values, so map it to the largest known level
                else if (ordinal > _levels.Max(kvp => kvp.Value.Ordinal))
                    return Fatal;
                // Otherwise, return the same level
                else
                    return _levels.First(l => l.Value.Ordinal == ordinal).Value;
            }
        }

        public IEnumerator<LogLevel> GetEnumerator()
        {
            return _levels.Select(kvp => kvp.Value).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _levels.Select(kvp => kvp.Value).GetEnumerator();
        }

        /// <summary>
        /// Highly verbose tracing information. Will log everything.
        /// </summary>
        public static readonly LogLevel Trace = LogLevel.Create(0, "Trace");

        /// <summary>
        /// Logs relevant debugging information. Slightly less verbose than Trace
        /// </summary>
        public static readonly LogLevel Debug = LogLevel.Create(1, "Debug", ConsoleColor.White);
        /// <summary>
        /// Logs success messages. Similar to informational, but more specific in it's purpose.
        /// </summary>
        public static readonly LogLevel Success = LogLevel.Create(2, "Success", ConsoleColor.Green);
        /// <summary>
        /// Logs informational messages.
        /// </summary>
        public static readonly LogLevel Info = LogLevel.Create(3, "Info", ConsoleColor.Cyan);
        /// <summary>
        /// Logs alert messages. These are not errors or warnings, but events
        /// more significant than informational in nature.
        /// </summary>
        public static readonly LogLevel Alert = LogLevel.Create(4, "Alert", ConsoleColor.Magenta);
        /// <summary>
        /// Logs warning messages. These are not errors, but events that may be
        /// unexpected, or could potentially identify an error that hasn't been
        /// caught yet.
        /// </summary>
        public static readonly LogLevel Warn = LogLevel.Create(5, "Warn", ConsoleColor.Yellow);
        /// <summary>
        /// Logs errors messages.
        /// </summary>
        public static readonly LogLevel Error = LogLevel.Create(6, "Error", ConsoleColor.Red);
        /// <summary>
        /// Logs exceptional/fatal/critial messages. These are events which are the cause
        /// of critical application failure, and likely resulted in a crash of some kind.
        /// </summary>
        public static readonly LogLevel Fatal = LogLevel.Create(7, "Fatal", ConsoleColor.Red, "Exception", "Critical");
    }
}
