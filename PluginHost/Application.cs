using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

using PluginHost.Tasks;
using PluginHost.Dependencies;
using PluginHost.Configuration;
using PluginHost.Extensions.Time;
using PluginHost.Interface.Tasks;
using PluginHost.Interface.Logging;

namespace PluginHost
{
    internal class Application
    {
        private ILogger _logger;
        private IDisposable _exportsChangedSubscription;

        internal ITaskManager Tasks;

        public void Init()
        {
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
        }

        public void Start()
        {
            Tasks.Start();
        }

        public void Stop()
        {
            try
            {
                _logger.Info("Shutting down...");

                // Shutdown task engine and wait 5 seconds to give
                // the processes time to clean up
                Tasks.Shutdown();

                _logger.Success("Goodbye!");
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
