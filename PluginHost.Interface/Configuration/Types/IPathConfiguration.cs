using System.IO;

namespace PluginHost.Interface.Configuration.Types
{
    public interface IPathConfiguration
    {
        string Name { get; set; }
        string Location { get; set; }
        DirectoryInfo Info { get; }
    }
}
