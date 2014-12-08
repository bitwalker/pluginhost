namespace PluginHost.Interface.Configuration
{
    using PluginHost.Interface.Logging;
    using PluginHost.Interface.Configuration.Types;

    public interface IConfig
    {
        /// <summary>
        /// Configuration for application paths
        /// </summary>
        IPathsConfiguration Paths { get; set; }
        /// <summary>
        /// Configuration for the command shell
        /// </summary>
        IShellConfiguration Shell { get; set; }
        /// <summary>
        /// Configuration for application logging
        /// </summary>
        ILoggingConfiguration Logging { get; set; }
    }
}