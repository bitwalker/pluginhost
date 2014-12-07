using System;

namespace PluginHost.Interface.Tasks
{
    /// <summary>
    /// Tasks are disposable objects which execute code to accomplish some task.
    /// They can run either as one time events, on a schedule, or in response to
    /// some event. The typical task workflow is performed like so:
    ///
    /// 1. Task object is instantiated
    /// 2. Task.Init is called. Init() should contain code to set up the task, but
    ///    can also be used to execute the task for the first time, in cases where
    ///    a first-run is effectively part of the initialization (for instance, indexing tasks).
    ///    In order to make sure Init() is not called more than once, ensure IsInitialized is set.
    /// 3. Task.Start is called. Start() should contain the core behavior of this task,
    ///    and should take into account that it can be executed more than once, even if
    ///    it's a single-run task, a user could request it to be run multiple times. To
    ///    keep the task from being started while it's already running, ensure IsStarted is set.
    /// 4. Task.Stop is called. Stop() should contain code to stop execution of the task, but
    ///    should dispose of objects set up in Init(), as Stopping is not equivalent to disposal.
    ///    Instead, ensure Dispose() is implemented, and put your teardown code in there, and ensure
    ///    IsDisposed is set.
    /// </summary>
    public interface ITask : IDisposable
    {
        /// <summary>
        /// Has this task been initialized?
        /// </summary>
        bool IsInitialized { get; }
        /// <summary>
        /// Has this task been started?
        /// </summary>
        bool IsStarted { get; }
        /// <summary>
        /// Is this task currently executing?
        /// </summary>
        bool IsExecuting { get; }
        /// <summary>
        /// Has this task been disposed?
        /// </summary>
        bool IsDisposed { get; }
        /// <summary>
        /// Initializes this task for execution
        /// </summary>
        void Init();
        /// <summary>
        /// Start executing this task
        /// </summary>
        void Start();
        /// <summary>
        /// Stop executing this task.
        /// </summary>
        /// <param name="immediate">Whether task execution should be brutally killed or not</param>
        void Stop(bool immediate);
    }
}
