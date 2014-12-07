using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

using PluginHost.App.Shell;
using PluginHost.App.Tasks;
using PluginHost.App.Dependencies;
using PluginHost.App.Configuration;
using PluginHost.Extensions.Time;
using PluginHost.Interface.Tasks;
using PluginHost.Interface.Logging;

namespace PluginHost.App
{
    public class Application : MarshalByRefObject
    {
        private ILogger _logger;
        private IDisposable _exportsChangedSubscription;
        private CancellationTokenSource _tokenSource;

        internal ITaskManager Tasks;
        internal CommandShell Shell;

        public static Application Current { get; private set; }

        /// <summary>
        /// Triggered when the Application should be reloaded
        /// </summary>
        public Subject<DateTime> OnReload;
        /// <summary>
        /// Triggered when Application is shutting down
        /// </summary>
        public Subject<DateTime> OnShutdown;

        public Application()
        {
            // This is a dirty hack, but it allows us to work around the need
            // for creating by reflection while still pretending this is a 'singleton' class.
            Current = this;
            OnReload = new Subject<DateTime>();
            OnShutdown = new Subject<DateTime>();
        }

        public void Init()
        {
            _tokenSource = new CancellationTokenSource();

            // Initialize directories
            if (!Config.Current.Paths.LocalStorage.Info.Exists)
                Config.Current.Paths.LocalStorage.Info.Create();
            if (!Config.Current.Paths.Plugins.Info.Exists)
                Config.Current.Paths.Plugins.Info.Create();

            _logger = DependencyInjector.Current.Resolve<ILogger>();
            Tasks   = DependencyInjector.Current.Resolve<ITaskManager>();

            _exportsChangedSubscription = DependencyInjector.Current.ExportChanged
                .Where(e => e.Metadata.ContainsKey(TaskManager.TaskNameMetadataKey))
                .Subscribe(UpdateTasks);

            Shell = new CommandShell(_tokenSource.Token);
        }

        public void Start()
        {
            Shell.Start();
        }

        public void Reload()
        {
            _logger.Alert("Reloading application...");
            OnReload.OnNext(DateTime.UtcNow);
        }

        public void Stop()
        {
            try
            {
                _logger.Info("Shutting down...");

                // Trigger all cancellable tasks
                _tokenSource.Cancel(throwOnFirstException: false);

                // Shutdown task engine and wait 5 seconds to give
                // the processes time to clean up
                Tasks.Shutdown();

                _logger.Success("Goodbye!");

                OnShutdown.OnNext(DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void UpdateTasks(ExportChangedEventArgs e)
        {
            var taskMeta = e.Metadata[TaskManager.TaskNameMetadataKey] as TaskMetadata;
            if (taskMeta == null)
                return;

            var taskName = taskMeta.Name;
            switch (e.Type)
            {
                case ExportChangeType.Added:
                    var task = ResolveTask(taskName);
                    Tasks.AddTask(taskName, task);
                    break;
                case ExportChangeType.Removed:
                default:
                    Tasks.RemoveTask(taskName);
                    break;
            }
        }

        private ITask ResolveTask(string taskName)
        {
            var task = DependencyInjector.Current
                .Resolve<ITask, IDictionary<string, object>>(meta =>
                {
                    if (!meta.ContainsKey(TaskManager.TaskNameMetadataKey))
                        return false;

                    var taskMeta = meta[TaskManager.TaskNameMetadataKey] as TaskMetadata;
                    if (taskMeta == null || taskMeta.Name == null)
                        return false;
                    return taskMeta.Name.Equals(taskName);
                });

            return task;
        }
    }
}
