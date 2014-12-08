namespace PluginHost.Interface.Configuration.Types
{
    using PluginHost.Interface.Logging;

    /// <summary>
    /// Represents the configuration for a specific logger type
    /// </summary>
    public interface ILoggerConfiguration
    {
        /// <summary>
        /// The name of the logger type to configure, e.g. ConsoleLogger
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// The logging level for this logger
        /// </summary>
        LogLevel LogLevel { get; set; }
    }
}
