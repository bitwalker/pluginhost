using System;
using System.Configuration;
using System.IO;

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
                    _cached = (Config) ConfigurationManager.GetSection("pluginHostConfiguration");
                return _cached;
            }
        }

        [ConfigurationProperty("paths")]
        public PathsElement Paths
        {
            get { return (PathsElement) this["paths"]; }
            set { this["paths"] = value; }
        }
    }

    public class PathsElement : ConfigurationElement
    {
        [ConfigurationProperty("plugins", DefaultValue = "Plugins", IsRequired = false)]
        public PathElement Plugins
        {
            get { return (PathElement) this["plugins"]; }
            set { this["plugins"] = value; }
        }

        /// <summary>
        /// Path where locally stored files should be kept.
        /// </summary>
        [ConfigurationProperty("localStorage", DefaultValue = "LocalStorage", IsRequired = false)]
        public PathElement LocalStorage
        {
            get { return (PathElement) this["localStorage"]; }
            set { this["localStorage"] = value; }
        }
    }

    public class PathElement : ConfigurationElement
    {
        /// <summary>
        /// The string representation of the path
        /// </summary>
        [ConfigurationProperty("location", DefaultValue = "LocalStorage", IsRequired = false)]
        public string Location
        {
            get { return (string) this["location"]; }
            set { this["location"] = value; }
        }

        /// <summary>
        /// The directory info for this path
        /// </summary>
        public DirectoryInfo Info
        {
            get
            {
                return new DirectoryInfo(Location);
            }
        }
    }
}
