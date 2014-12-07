using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using PluginHost.Interface.Tasks;
using PluginHost.Interface.Logging;

namespace PluginHost.App.Tasks
{
    /// <summary>
    /// Primary entry point for all plugin-based tasks in the PluginHost application.
    /// </summary>
    public class TaskManager : ITaskManager
    {
        private bool _started = false;
        private bool _shuttingDown = false;

        [Import(AllowRecomposition = true)]
        private ILogger _logger { get; set; }
        [Import(AllowRecomposition = true)]
        private IEventLoop _eventLoop { get; set; }
        [ImportMany(AllowRecomposition = true)]
        private IEnumerable<Lazy<ITask, ITaskMetadata>> _tasks { get; set; }

        public bool IsStarted { get { return _started; } }

        public IQueryable<KeyValuePair<string, ITask>> AvailableTasks
        {
            get
            {
                return _tasks
                    .Select(t => new KeyValuePair<string, ITask>(t.Metadata.TaskName, t.Value))
                    .AsQueryable();
            }
        }

        public void AddTask(string taskName, ITask task, bool init = true, bool start = true)
        {
            //try
            //{
            //    if (_tasks.ContainsKey(taskName))
            //    {
            //        _logger.Warn("Attempted to add a task ({0}) which is already being managed!", taskName);
            //        _logger.Warn("Request to add new task ({0}) has been denied.", taskName);
            //        return;
            //    }

            //    _logger.Alert("Adding new task ({0})...", taskName);

            //    _tasks.Add(taskName, task);

            //    if (_started)
            //    {
            //        if (init)  { InitTask(taskName); }
            //        if (start) { StartTask(taskName); }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    _logger.Error(ex);
            //    _logger.Warn("Non-fatal exception occurred while adding new task. All systems normal.");
            //}
        }

        public void RemoveTask(string taskName)
        {
            //try
            //{
            //    if (!_tasks.ContainsKey(taskName))
            //    {
            //        _logger.Warn("Removal of task ({0}) was requested, but is not known to the task manager!");
            //        return;
            //    }

            //    _logger.Alert("Removal of task ({0}) has been requested!");

            //    if (_started)
            //    {
            //        StopTask(taskName);
            //    }

            //    _tasks.Remove(taskName);
            //    _logger.Success("Task ({0}) has been removed successfully.");
            //}
            //catch (Exception ex)
            //{
            //    _logger.Error(ex);
            //    _logger.Alert("Non-fatal exception occurred, all systems normal.");
            //}
        }

        public void Start()
        {
            if (_shuttingDown)
            {
                _logger.Warn("Cannot start TaskManager, as shutdown is in progress.");
                return;
            }
            if (_started)
                return;

            _started = true;

            // Start event loop
            _eventLoop.Start();

            // Load and execute tasks
            Run();
        }

        public void Shutdown()
        {
            _logger.Warn("TaskManager shutting down!");

            if (!_started || _shuttingDown)
                return;

            // Set flags
            _shuttingDown = true;
            _started      = false;

            // Stop publishing events
            _eventLoop.Stop(true);

            // Stop all tasks
            foreach (var _task in _tasks)
            {
                _task.Value.Stop(brutalKill: true);
            }
        }

        public void InitTask(string taskName)
        {
            var task = _tasks.FirstOrDefault(t => t.Metadata.TaskName == taskName);
            if (task == null)
                return;

            InitTask(task);
        }

        private void InitTask(Lazy<ITask, ITaskMetadata> task)
        {
            if (task.Value.IsInitialized)
                return;
            try
            {
                _logger.Info("Initializing task ({0})...", task.Metadata.TaskName);
                task.Value.Init();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _logger.Warn("Non-fatal exception occurred while initializing task. All systems normal.");
            }
        }

        public void StartTask(string taskName)
        {
            var task = _tasks.FirstOrDefault(t => t.Metadata.TaskName == taskName);
            if (task == null)
                return;

            StartTask(task);
        }

        private void StartTask(Lazy<ITask, ITaskMetadata> task)
        {
            if (task.Value.IsStarted)
                return;

            try
            {
                _logger.Info("Starting task ({0})...", task.Metadata.TaskName);
                task.Value.Start();
                _logger.Success("Task ({0}) has been started.", task.Metadata.TaskName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _logger.Warn("Non-fatal exception occurred while starting task. All systems normal.");
            }
        }

        public void StopTask(string taskName)
        {
            var task = _tasks.FirstOrDefault(t => t.Metadata.TaskName == taskName);
            if (task == null)
                return;

            StopTask(task);
        }

        private void StopTask(Lazy<ITask, ITaskMetadata> task)
        {
            if (!task.Value.IsStarted)
                return;

            try
            {
                _logger.Info("Stopping task ({0})...", task.Metadata.TaskName);
                task.Value.Stop(brutalKill: true);
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

            foreach (var task in _tasks)
            {
                InitTask(task);
            }

            _logger.Success("All tasks have been initialized.");

            // Start task execution
            foreach (var task in _tasks)
            {
                StartTask(task);
            }

            _logger.Success("All tasks have been started.");
        }
    }
}