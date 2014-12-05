using System;
using System.Configuration;
using System.IO;

using PluginHost.Configuration.Elements;

namespace PluginHost.Configuration
{
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
        public PathsElement Paths
        {
            get { return this["paths"] as PathsElement; }
            set { this["paths"] = value; }
        }
    }
}
