using System.Linq;
using System.Collections.Generic;

using PluginHost.Interface.Tasks;

namespace PluginHost.App.Tasks
{
    public interface ITaskManager
    {
        /// <summary>
        /// Whether or not the task manager instance is started
        /// </summary>
        bool IsStarted { get; }
        /// <summary>
        /// A queryable collection of tasks known to this task manager
        /// </summary>
        IQueryable<KeyValuePair<string, ITask>> AvailableTasks { get; }
        /// <summary>
        /// Add a new task, and optionally initialize/start it.
        /// </summary>
        /// <param name="taskName">The name of the new task</param>
        /// <param name="task">The new task</param>
        /// <param name="init">Whether or not to initialize this task when added</param>
        /// <param name="start">Whether or not to start this task when added</param>
        void AddTask(string taskName, ITask task, bool init = true, bool start = true);
        /// <summary>
        /// Remove a task from this task manager
        /// </summary>
        /// <param name="taskName"></param>
        void RemoveTask(string taskName);
        /// <summary>
        /// Start the task manager and associated tasks
        /// </summary>
        void Start();
        /// <summary>
        /// Stop the task manager and associated tasks
        /// </summary>
        void Shutdown();
        /// <summary>
        /// Initialize a given task
        /// </summary>
        /// <param name="taskName">The name of the task to initialize</param>
        void InitTask(string taskName);
        /// <summary>
        /// Start a given task
        /// </summary>
        /// <param name="taskName">The name of the task to start</param>
        void StartTask(string taskName);
        /// <summary>
        /// Stop a given task
        /// </summary>
        /// <param name="taskName">The name of the task to stop</param>
        void StopTask(string taskName);
    }
}