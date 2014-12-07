using System.IO;

namespace PluginHost.App.Helpers
{
    /// <summary>
    /// A cleaner version of FileSystemEventArgs used in
    /// conjunction with DirectoryWatcher.
    /// </summary>
    public struct FileChangedEventArgs
    {
        public FileChangedEvent Type { get; set; }
        public string Path { get; set; }

        public static FileChangedEventArgs Map(FileSystemEventArgs e)
        {
            var type = FileChangedEvent.Changed;
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Changed:
                    type = FileChangedEvent.Changed;
                    break;
                case WatcherChangeTypes.Created:
                    type = FileChangedEvent.Created;
                    break;
                case WatcherChangeTypes.Deleted:
                    type = FileChangedEvent.Removed;
                    break;
                default:
                    break;
            }

            return new FileChangedEventArgs() {
                Path = e.FullPath,
                Type = type
            };
        }
    }
}
