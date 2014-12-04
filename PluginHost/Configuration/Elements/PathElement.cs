using System.IO;
using System.Configuration;

namespace PluginHost.Configuration.Elements
{
    public interface IPathElement
    {
        string Location { get; set; }
        DirectoryInfo Info { get; }
    }

    public class PathElement : ConfigurationElement, IPathElement
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
