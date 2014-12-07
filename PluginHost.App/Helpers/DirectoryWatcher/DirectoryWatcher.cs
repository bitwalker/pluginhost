using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

using PluginHost.Extensions.Time;
using PluginHost.Interface.Logging;

namespace PluginHost.App.Helpers
{
    /// <summary>
    /// Defines a disposable file system observer specifically designed
    /// to monitor a directory path for changes to files matching a given
    /// filter.
    ///
    /// Once created, call Watch to start a background task which will
    /// monitor the given directory indefinitely. In order to listen to
    /// events published by the watcher, you must subscribe to the event
    /// stream via the Events property. You can filter that subscription
    /// for specific events, or listen to all of them.
    ///
    /// Currently, DirectoryWatcher only publishes two types of FileChangedEvents,
    /// Ready, and Removed. Created and Changed occur before a file is necessarily
    /// accessible, and because of this, DirectoryWatcher handles those event types
    /// internally, monitoring files that change until they are available for use, at
    /// which point the Ready event is published. Removals are effective immediately.
    ///
    /// DirectoryWatcher is not meant to be reused - create it, call Watch, and
    /// then call Dispose (or wrap in a using) once done with it. Dispose is the
    /// only way to shutdown and release the watcher task.
    /// </summary>
    public class DirectoryWatcher : IDisposable
    {
        private const int ERROR_SHARING_VIOLATION = 32;
        private const int ERROR_LOCK_VIOLATION    = 33;
        private const NotifyFilters NOTIFY_FILTERS =
            NotifyFilters.CreationTime |
            NotifyFilters.LastWrite |
            NotifyFilters.Size;


        private string _path;
        private string _filter;
        private bool _isDisposed;
        private FileSystemWatcher _watcher;
        private IDisposable _eventObserver;
        // This is different than the public Events Subject,
        // because it receives all events, unbuffered and unfiltered.
        // Events receives the buffered/filtered event stream which is
        // more suitable for observers.
        private Subject<FileChangedEventArgs> _events;
        private ILogger _logger;

        public Subject<FileChangedEventArgs> Events;

        public DirectoryWatcher(ILogger logger, string path, string filter = "*.*")
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or empty!");

            _logger = logger;

            if (Path.IsPathRooted(path))
                _path = path;
            else
                _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

            if (string.IsNullOrWhiteSpace(filter))
                filter = "*.*";

            _filter  = filter;
            _watcher = new FileSystemWatcher();
            _events  = new Subject<FileChangedEventArgs>();
            Events   = new Subject<FileChangedEventArgs>();

            _watcher.Path   = _path;
            _watcher.Filter = _filter;
            _watcher.NotifyFilter = NOTIFY_FILTERS;
            _watcher.Created += new FileSystemEventHandler(OnFileSystemEvent);
            _watcher.Changed += new FileSystemEventHandler(OnFileSystemEvent);
        }

        /// <summary>
        /// Starts a new background task which handles monitoring the provided
        /// directory path for file system events which effect files matching
        /// the provided filter.
        /// </summary>
        /// <returns>Task</returns>
        public async Task Watch()
        {
            if (_isDisposed)
                throw new Exception("Attempted to use FileObserver after it was disposed!");

            // Subscribe to Created and Changed events, but throttled by a short
            // period of time to prevent checking the file's availability when it's
            // already a given that it will fail. We will always get at least one event
            // when throttling. OnChange is bound as the handler for this subscription.
            _eventObserver = _events
                .Where(ev => ev.Type == FileChangedEvent.Created ||
                             ev.Type == FileChangedEvent.Changed ||
                             ev.Type == FileChangedEvent.Removed)
                .Throttle(50.Milliseconds())
                .Subscribe(OnChange);


            // Start watching
            _watcher.EnableRaisingEvents = true;

            _logger.Info("Watching for dependencies in {0} with filter {1}", _path, _filter);

            // Wait until either a timeout occurs, or _events.OnCompleted is called,
            // which is done by OnReady when a Ready event is received.
            await _events
                .Where(ev => ev.Type == FileChangedEvent.Ready || ev.Type == FileChangedEvent.Removed)
                .ForEachAsync(PublishEvent);
        }

        /// <summary>
        /// Should be called when this DirectoryWatcher is no longer needed.
        /// It will stop the watcher, notify subscribers that it is shutting
        /// down, and clean up it's internals.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _watcher.EnableRaisingEvents = false;

            if (_eventObserver != null)
                _eventObserver.Dispose();

            // Signal observers that we're done publishing events
            _events.OnCompleted();
            Events.OnCompleted();

            _watcher.Dispose();
            _watcher = null;

            _isDisposed = true;
        }

        /// <summary>
        /// Called when the internal FileSystemWatcher sends an event notification
        /// </summary>
        /// <param name="state">The current FileSystemWatcher state</param>
        /// <param name="e">The FileSystemEventArgs associated with this event</param>
        private void OnFileSystemEvent(object state, FileSystemEventArgs e)
        {
            _events.OnNext(FileChangedEventArgs.Map(e));
        }

        /// <summary>
        /// Publishes a FileChangedEvent to the internal event stream
        /// on removal, or on change as long as the file is available.
        /// </summary>
        /// <param name="e">The FileChangedEventArgs for this event</param>
        private void OnChange(FileChangedEventArgs e)
        {
            if (e.Type == FileChangedEvent.Removed)
            {
                _events.OnNext(e);
            }
            else if (!IsFileLocked(e.Path))
            {
                // File is fully ready
                e.Type = FileChangedEvent.Ready;
                _events.OnNext(e);
            }
        }

        /// <summary>
        /// Called when a change event occurs and the file was
        /// either removed, or was created/changed and is available for use.
        /// This publishes an event to the public event stream.
        /// </summary>
        /// <param name="e">The FileChangedEventArgs for this event</param>
        private void PublishEvent(FileChangedEventArgs e)
        {
            Events.OnNext(e);
        }

        /// <summary>
        /// Checks to see if a file is currently locked, which tells
        /// us whether or not this file is still being written to, or
        /// otherwise in use by the process making the change.
        /// </summary>
        /// <param name="file">The path to the file to check</param>
        /// <returns>Boolean</returns>
        private static bool IsFileLocked(string file)
        {
            FileStream stream = null;

            // Attempt to open the file with full permissions and no sharing allowed.
            // This ensures that not only can we open the file, but that nobody else has it open either.
            try
            {
                stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException ex)
            {
                // We could get exceptions for any number of reasons, but the file is only "locked"
                // if one of the following lower-level error codes is given.
                if (ex.HResult == ERROR_LOCK_VIOLATION || ex.HResult == ERROR_SHARING_VIOLATION)
                    return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            return false;
        }
    }
}