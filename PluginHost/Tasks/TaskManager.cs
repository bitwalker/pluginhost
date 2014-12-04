using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using PluginHost.Interface.Tasks;
using PluginHost.Interface.Logging;

namespace PluginHost.Tasks
{
    /// <summary>
    /// Primary entry point for all plugin-based tasks in the PluginHost application.
    /// </summary>
    public class TaskManager
    {
        public const string TaskNameMetadataKey = "TaskName";

        private bool _started = false;
        private bool _shuttingDown = false;
        private EventLoop _eventLoop;
        private ILogger _logger;
        private IDictionary<string, ITask> _tasks;

        public TaskManager(IEnumerable<Lazy<ITask, IDictionary<string, object>>> tasks, Lazy<ILogger> logger)
        {
            _logger = logger.Value;
            _tasks  = new ConcurrentDictionary<string, ITask>();

            foreach (var task in tasks)
            {
                var taskName = task.Metadata[TaskNameMetadataKey] as string;
                AddTask(taskName, task.Value, init: false, start: false);
            }
        }

        public void AddTask(string taskName, ITask task, bool init = true, bool start = true)
        {
            try
            {
                if (_tasks.ContainsKey(taskName))
                {
                    _logger.Warn("Attempted to add a task ({0}) which is already being managed!");
                    _logger.Warn("Request to add new task ({0}) has been denied.");
                    return;
                }

                _logger.Alert("Adding new task ({0})...", taskName);

                if (_started)
                {
                    if (init)  { InitTask(taskName, task); }
                    if (start) { StartTask(taskName, task); }
                }

                _tasks.Add(taskName, task);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _logger.Warn("Non-fatal exception occurred while adding new task. All systems normal.");
            }
        }

        public void RemoveTask(string taskName)
        {
            try
            {
                if (!_tasks.ContainsKey(taskName))
                {
                    _logger.Warn("Removal of task ({0}) was requested, but is not known to the task manager!");
                    return;
                }

                _logger.Alert("Removal of task ({0}) has been requested!");

                var task = _tasks[taskName];
                if (task != null)
                {
                    if (_started)
                    {
                        StopTask(taskName, task);
                    }
                }

                _tasks.Remove(taskName);
                _logger.Success("Task ({0}) has been removed successfully.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _logger.Alert("Non-fatal exception occurred, all systems normal.");
            }
        }

        public void Start()
        {
            if (_shuttingDown)
                throw new Exception("Unexpected call to TaskManager.Start during shutdown.");
            if (_started)
                throw new Exception("Unexpected call to TaskManager.Start when already started.");

            _started = true;
            _eventLoop = new EventLoop(_logger);

            // Start event loop
            _eventLoop.Start();

            // Load and execute tasks
            Run();
        }

        private void InitTask(string taskName, ITask task)
        {
            try
            {
                _logger.Info("Initializing task ({0})...", taskName);
                task.Init();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _logger.Warn("Non-fatal exception occurred while initializing task. All systems normal.");
            }
        }

        private void StartTask(string taskName, ITask task)
        {
            try
            {
                _logger.Info("Starting task ({0})...", taskName);
                task.Start();
                _logger.Success("Task ({0}) has been started.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _logger.Warn("Non-fatal exception occurred while starting task. All systems normal.");
            }
        }

        private void StopTask(string taskName, ITask task)
        {
            try
            {
                _logger.Info("Stopping task ({0})...");
                task.Stop(brutalKill: true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _logger.Warn("Non-fatal exception occurred while stopping task. All systems normal.");
            }
        }

        private void Run()
        {
            if (_shuttingDown)
                return;

            // Initialize tasks
            foreach (var task in _tasks)
            {
                InitTask(task.Key, task.Value);
            }

            _logger.Info("All tasks have been initialized.");

            // Start task execution
            foreach (var task in _tasks)
            {
                StartTask(task.Key, task.Value);
            }

            _logger.Info("All tasks have been started.");
        }

        public void Shutdown()
        {
            _logger.Warn("TaskManager shutting down!");

            if (!_started || _shuttingDown)
                throw new Exception("Unexpected call to TaskManager.Shutdown when already shut down.");

            _shuttingDown = true;
            _started      = false;

            _eventLoop.Stop(true);
        }
    }
}