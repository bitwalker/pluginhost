using System.Configuration;

namespace PluginHost.App.Configuration
{
    using PluginHost.Interface.Configuration;

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
        public Elements.PathsElement Paths
        {
            get { return this["paths"] as Elements.PathsElement; }
            set { this["paths"] = value; }
        }

        [ConfigurationProperty("logging")]
        public Elements.LoggingElement Logging
        {
            get { return this["logging"] as Elements.LoggingElement; }
            set { this["logging"] = value; }
        }
    }
}
