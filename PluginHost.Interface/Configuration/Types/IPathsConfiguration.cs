namespace PluginHost.Interface.Configuration.Types
{
    public interface IPathsConfiguration
    {
        IPathConfiguration Plugins { get; }
        IPathConfiguration LocalStorage { get; }
    }
}
