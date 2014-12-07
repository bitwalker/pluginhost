using System.IO;
using System.Configuration;

namespace PluginHost.App.Configuration.Elements
{
    public interface IPathElement
    {
        string Name { get; set; }
        string Location { get; set; }
        DirectoryInfo Info { get; }
    }

    public class PathElement : ConfigurationElement, IPathElement
    {
        /// <summary>
        /// The name of this path
        /// </summary>
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string) this["name"]; }
            set { this["name"] = value; }
        }

        /// <summary>
        /// The string representation of the path
        /// </summary>
        [ConfigurationProperty("location", DefaultValue = "LocalStorage", IsRequired = true)]
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
