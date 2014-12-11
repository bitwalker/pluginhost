using System;
using System.Configuration;

namespace PluginHost.App.Configuration.Elements
{
    using PluginHost.Interface.Configuration.Types;

    public class LoggingElement : ConfigurationElement, ILoggingConfiguration
    {
        [ConfigurationProperty("loggers")]
        public ILoggersConfiguration Loggers
        {
            get { return this["loggers"] as LoggersElement; }
            set { this["loggers"] = value; }
        }

        [ConfigurationProperty("enabled")]
        public bool IsEnabled
        {
            get { return Convert.ToBoolean(this["enabled"]); }
            set { this["enabled"] = value; }
        }
    }
}
