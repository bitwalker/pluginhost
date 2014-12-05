using PluginHost.Configuration.Elements;

namespace PluginHost.Configuration
{
    public interface IConfig
    {
        /// <summary>
        /// All directory path configuration for this application
        /// </summary>
        PathsElement Paths { get; set; }
    }
}