using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;

using PluginHost.Extensions.Time;
using PluginHost.Tasks;
using PluginHost.Dependencies;
using PluginHost.Interface.Tasks;
using PluginHost.Interface.Logging;

namespace PluginHost
{
    class Program
    {
        private TaskManager _taskManager;
        private IDisposable _exportsChangedSubscription;

        public Lazy<ILogger> Logger;

        private Program()
        {
            DependencyInjector.Current.Inject(this);
        }

        static void Main(string[] args)
        {
            Task t = MainAsync(args);
            t.Wait();
        }

        async static Task MainAsync(string[] args)
        {
            var app = new Program();
            await app.Run();
        }

        /// <summary>
        /// Start running the task manager and begin waiting for user input
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            var tasks = DependencyInjector.Current
                .LazyResolveMany<ITask, IDictionary<string, object>>();

            _taskManager = new TaskManager(tasks, Logger);
            _taskManager.Start();

            _exportsChangedSubscription = DependencyInjector.Current.ExportChanged
                .Where(e => e.Metadata.ContainsKey(TaskManager.TaskNameMetadataKey))
                .Subscribe(UpdateTasks);

            await Wait();
        }

        /// <summary>
        /// Starts waiting for user cancellation on a new thread
        /// </summary>
        /// <returns></returns>
        private Task Wait()
        {
            return Task.Run(() => Loop());
        }

        private void Loop()
        {
            // Since C# has no tail-recursion, use goto to emulate
            // infinite recursion to keep the loop going.
        readkey:
            var key = Console.ReadKey(intercept: true);
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    Shutdown();
                    return;
                case ConsoleKey.C:
                    if (key.Modifiers.HasFlag(ConsoleModifiers.Control))
                    {
                        Shutdown();
                        return;
                    }
                    break;
                default:
                    break;
            }
            goto readkey;
        }

        private void Shutdown()
        {
            try
            {
                Logger.Value.Info("Shutting down...");

                // Shutdown task engine and wait 5 seconds to give
                // the processes time to clean up
                _taskManager.Shutdown();
                Task.Delay(5.Seconds());

                Logger.Value.Success("Goodbye!");
            }
            catch (Exception ex)
            {
                Logger.Value.Error(ex);
            }
        }

        private void UpdateTasks(ExportChangedEventArgs e)
        {
            var taskName = e.Metadata[TaskManager.TaskNameMetadataKey] as string;
            switch (e.Type)
            {
                case ExportChangeType.Added:
                    var task = ResolveTask(taskName);
                    _taskManager.AddTask(taskName, task);
                    break;
                case ExportChangeType.Removed:
                default:
                    _taskManager.RemoveTask(taskName);
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

                    return meta[TaskManager.TaskNameMetadataKey].Equals(taskName);
                });

            return task;
        }
    }
}
