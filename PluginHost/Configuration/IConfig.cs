namespace PluginHost.Configuration
{
    public interface IConfig
    {
        /// <summary>
        /// The path to the plugins directory
        /// </summary>
        string PluginPath { get; set; }

        /// <summary>
        /// Path where locally stored files should be kept.
        /// </summary>
        string LocalStoragePath { get; set; }
    }
}