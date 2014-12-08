using System.Collections.Generic;

namespace PluginHost.Interface.Configuration.Types
{
    using PluginHost.Interface.Logging;

    public interface ILoggingConfiguration
    {
        /// <summary>
        /// Configuration for individual loggers
        /// </summary>
        ILoggersConfiguration Loggers { get; set; }
        /// <summary>
        /// Whether or not logging is enabled globally
        /// </summary>
        bool IsEnabled { get; set; }
    }
}
