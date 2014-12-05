namespace PluginHost.Interface.Tasks
{
    public interface ITask
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
        void Stop(bool brutalKill);
    }
}
