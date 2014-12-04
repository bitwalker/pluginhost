namespace PluginHost.Interface.Tasks
{
    public interface ITask
    {
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
