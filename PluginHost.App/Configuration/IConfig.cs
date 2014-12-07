using PluginHost.App.Configuration.Elements;

namespace PluginHost.App.Configuration
{
    public interface IConfig
    {
        /// <summary>
        /// All directory path configuration for this application
        /// </summary>
        PathsElement Paths { get; set; }
    }
}