using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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
        [Import(AllowRecomposition = true)]
        private ILogger _logger = null;
        private IDisposable _exportsChangedSubscription;
        private CancellationTokenSource _tokenSource;

        [Import(AllowRecomposition = true)]
        internal ITaskManager Tasks { get; set; }
        internal CommandShell Shell { get; private set; }

        public static Application Current { get; private set; }

        /// <summary>
        /// Triggered when the Application should be reloaded
        /// </summary>
        public event EventHandler OnReload;
        /// <summary>
        /// Triggered when Application is shutting down
        /// </summary>
        public event EventHandler OnShutdown;

        public Application()
        {
            _tokenSource = new CancellationTokenSource();

            // Initialize directories
            if (!Config.Current.Paths.LocalStorage.Info.Exists)
                Config.Current.Paths.LocalStorage.Info.Create();
            if (!Config.Current.Paths.Plugins.Info.Exists)
                Config.Current.Paths.Plugins.Info.Create();

            DependencyInjector.Current.Inject(this);

            _exportsChangedSubscription = DependencyInjector.Current.ExportChanged
                .Where(e => e.Metadata.ContainsKey(TaskMetadata.MetadataKey))
                .Subscribe(UpdateTasks);

            Shell = new CommandShell(_tokenSource.Token);

            // This is a dirty hack, but it allows us to work around the need
            // for creating by reflection while still pretending this is a 'singleton' class.
            Current = this;
        }

        public void Start()
        {
            Shell.Start();
        }

        public void Reload()
        {
            _logger.Alert("Reloading application...");
            if (OnReload != null)
                OnReload(null, EventArgs.Empty);
        }

        public void Stop()
        {
            try
            {
                _logger.Info("Shutting down...");

                // Shutdown task engine
                Tasks.Shutdown();

                // Kill the shell
                _tokenSource.Cancel(throwOnFirstException: false);

                _logger.Success("Goodbye!");

                if (OnShutdown != null)
                    OnShutdown(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private void UpdateTasks(ExportChangedEventArgs e)
        {
            var taskMeta = e.Metadata[TaskMetadata.MetadataKey] as ITaskMetadata;
            if (taskMeta == null)
                return;

            var taskName = taskMeta.TaskName;
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
                    if (!meta.ContainsKey(TaskMetadata.MetadataKey))
                        return false;

                    var taskMeta = meta[TaskMetadata.MetadataKey] as ITaskMetadata;
                    if (taskMeta == null || taskMeta.TaskName == null)
                        return false;
                    return taskMeta.TaskName.Equals(taskName);
                });

            return task;
        }
    }
}
