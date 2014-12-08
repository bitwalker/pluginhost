using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace PluginHost.Interface.Logging
{
    /// <summary>
    /// Defines the level at which logging output should be filtered.
    /// The higher the ordinal value of the level, the more significant the information.
    /// Any levels with a less significant ordinal value than the selected level will not
    /// be logged, anything equal to or greater will be logged normally.
    /// </summary>
    [DebuggerDisplay("{Name} = {Ordinal}")]
    public class LogLevel
        : IComparable, IComparable<LogLevel>, IComparable<short>, IComparable<int>,
          IEqualityComparer<LogLevel>
    {
        /// <summary>
        /// This static instance represents disabled logging, and is the default
        /// value for the LogLevel struct.
        /// </summary>
        public static LogLevel None { get { return new LogLevel(); } }

        /// <summary>
        /// The numerical value of this log level
        /// </summary>
        public short Ordinal { get; private set; }

        /// <summary>
        /// The name of this log level
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Alternate names for this log level
        /// </summary>
        public string[] Aliases { get; private set; }

        /// <summary>
        /// The color to use when logging events of this level
        /// </summary>
        public ConsoleColor Color { get; private set; }

        public LogLevel()
        {
            Ordinal = -1;
            Name    = "None";
            Aliases = new string[0];
            Color   = ConsoleColor.Gray;
        }

        public LogLevel(short ordinal, string name, string[] aliases = null, ConsoleColor color = ConsoleColor.Gray)
        {
            Ordinal = ordinal;
            Name    = name;
            Aliases = aliases ?? new string[0];
            Color   = color;
        }

        public static LogLevel Create(short ordinal, string name)
        {
            return new LogLevel(ordinal, name);
        }

        public static LogLevel Create(short ordinal, string name, ConsoleColor color)
        {
            return new LogLevel(ordinal, name, color: color);
        }

        public static LogLevel Create(short ordinal, string name, params string[] aliases)
        {
            return new LogLevel(ordinal, name, aliases);
        }

        public static LogLevel Create(short ordinal, string name, ConsoleColor color, params string[] aliases)
        {
            return new LogLevel(ordinal, name, aliases, color);
        }

        public static implicit operator short(LogLevel level)
        {
            if (level == null)
                return -1;
            return level.Ordinal;
        }

        public override string ToString()
        {
            return Name;
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            if (!(obj is LogLevel))
                throw new ArgumentException(string.Format("The provided value is not a LogLevel"));

            return CompareTo((LogLevel) obj);
        }

        public int CompareTo(LogLevel other)
        {
            if (other == null)
                return 1;
            return Ordinal.CompareTo(other.Ordinal);
        }

        public int CompareTo(short other)
        {
            return Ordinal.CompareTo(other);
        }

        public int CompareTo(int other)
        {
            return Ordinal.CompareTo(other);
        }

        public bool Equals(LogLevel x, LogLevel y)
        {
            if ((x == null && y != null) || (x != null && y == null))
                return false;
            if (x == null || y == null)
                return true;
            if (x.Ordinal == y.Ordinal)
                return true;
            else
                return false;
        }

        public int GetHashCode(LogLevel obj)
        {
            if (obj == null)
                return -1;
            return obj.Ordinal;
        }
    }
}
