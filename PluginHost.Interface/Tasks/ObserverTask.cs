using System;

using PluginHost.Extensions.Time;
using PluginHost.Interface.Logging;

namespace PluginHost.Interface.Tasks
{
    /// <summary>
    /// The abstract base class for tasks which should be executed based on some event.
    /// This type of task is useful for responding to events published by other components
    /// of the system, such as receipt of a remote command.
    /// </summary>
    /// <typeparam name="T">The type of event to subscribe to</typeparam>
    public abstract class ObserverTask<T> : IObserver<T>, ITask
        where T : class
    {
        private bool _shuttingDown = false;
        private IDisposable _subscription;
        private readonly string _description;

        public IEventBus EventBus { get; set; }
        public ILogger Logger { get; set; }

        protected ObserverTask(string description)
        {
            _description = description;
        }

        public virtual void Start()
        {
            // Subscribe to events of type T
            _subscription = EventBus.Subscribe(this);
        }

        public void OnNext(T value)
        {
            if (_shuttingDown)
                return;

            var isoDate = DateTime.UtcNow.ToISO8601z();
            Logger.Info("{0} - Executing: {1}", isoDate, _description);
            Execute();
        }

        public void OnError(Exception error)
        {
            Logger.Error(error);
        }

        public void OnCompleted()
        {
            Stop(true);
        }

        /// <summary>
        /// Initialize this task for execution
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// Executes the workload for this scheduled task.
        /// </summary>
        /// <returns></returns>
        protected abstract void Execute();

        /// <summary>
        /// Called when this task should be stopped.
        /// Ensure you call base.Stop in your implementation, to make
        /// sure the instance is fully cleaned up.
        /// </summary>
        /// <param name="brutalKill">Whether the task should be killed or not</param>
        public virtual void Stop(bool brutalKill)
        {
            if (_shuttingDown)
                return;

            _shuttingDown = true;
            _subscription.Dispose();
        }
    }
}
