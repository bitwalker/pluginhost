using System.Configuration;

namespace PluginHost.Configuration.Elements
{
    public interface IPathsElement
    {
        IPathElement Plugins { get; set; }
        IPathElement LocalStorage { get; set; }
    }

    public class PathsElement : ConfigurationElement
    {
        [ConfigurationProperty("plugins", DefaultValue = "Plugins", IsRequired = false)]
        public IPathElement Plugins
        {
            get { return (PathElement) this["plugins"]; }
            set { this["plugins"] = value; }
        }

        /// <summary>
        /// Path where locally stored files should be kept.
        /// </summary>
        [ConfigurationProperty("localStorage", DefaultValue = "LocalStorage", IsRequired = false)]
        public IPathElement LocalStorage
        {
            get { return (PathElement) this["localStorage"]; }
            set { this["localStorage"] = value; }
        }
    }
}
