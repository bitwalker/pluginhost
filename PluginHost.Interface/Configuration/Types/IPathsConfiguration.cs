namespace PluginHost.Interface.Configuration.Types
{
    public interface IPathsConfiguration
    {
        IPathConfiguration Plugins { get; set; }
        IPathConfiguration LocalStorage { get; set; }
    }
}
