namespace PluginHost.App.Helpers
{
    /// <summary>
    /// Defines the core types of file change events we care about.
    /// Used in conjunction with DirectoryWatcher.
    /// </summary>
    public enum FileChangedEvent
    {
        Changed = 0,
        Created,
        Removed,
        Ready,
    }
}
