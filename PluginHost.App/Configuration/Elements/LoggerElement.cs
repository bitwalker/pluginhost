using System.Configuration;

namespace PluginHost.App.Configuration.Elements
{
    using PluginHost.Interface.Logging;
    using PluginHost.Interface.Configuration.Types;

    public class LoggerElement : ConfigurationElement, ILoggerConfiguration
    {
        /// <summary>
        /// The name of the logger type to configure, e.g. ConsoleLogger
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string) this["name"]; }
            set { this["name"] = value; }
        }

        /// <summary>
        /// The name of the logging level to use
        /// </summary>
        [ConfigurationProperty("logLevel", DefaultValue = "debug", IsRequired = true)]
        public string LogLevelName
        {
            get { return (string) this["logLevel"]; }
            set { this["logLevel"] = value; }
        }

        /// <summary>
        /// The LogLevel value based on the configured logging level name
        /// </summary>
        public LogLevel LogLevel
        {
            get { return LogLevels.All[LogLevelName]; }
            set { LogLevelName = value.Name.ToLowerInvariant(); }
        }
    }
}
