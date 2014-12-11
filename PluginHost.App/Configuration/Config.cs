using System.Configuration;
using PluginHost.Interface.Configuration.Types;

namespace PluginHost.App.Configuration
{
    using PluginHost.Interface.Configuration;
    using PluginHost.App.Configuration.Elements;

    public class Config : ConfigurationSection, IConfig
    {
        private static Config _cached;

        /// <summary>
        /// Get the current configuration, by loading via ConfigurationManager.
        /// The config is cached on first load, so subsequent requests will hit
        /// the cached instance.
        /// </summary>
        public static Config Current
        {
            get
            {
                if (_cached == null)
                    _cached = (Config) ConfigurationManager.GetSection("pluginHost");
                return _cached;
            }
        }

        [ConfigurationProperty("paths")]
        public IPathsConfiguration Paths
        {
            get { return this["paths"] as PathsElement; }
            set { this["paths"] = value; }
        }

        [ConfigurationProperty("logging")]
        public ILoggingConfiguration Logging
        {
            get { return this["logging"] as LoggingElement; }
            set
            {
                this["logging"] = value;
            }
        }

        [ConfigurationProperty("shell")]
        public IShellConfiguration Shell
        {
            get { return this["shell"] as ShellElement; }
            set
            {
                (this["shell"] as ShellElement).Prompt = value.Prompt;
            }
        }
    }
}
