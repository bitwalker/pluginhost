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
        private IDisposable _subscription;
        private readonly string _description;
        private readonly bool _quiet;

        public bool IsDisposed { get; protected set; }
        public abstract bool IsInitialized { get; protected set; }
        public bool IsStarted { get; protected set; }
        public bool IsExecuting { get; protected set; }
        protected abstract IEventBus EventBus { get; set; }
        protected abstract ILogger Logger { get; set; }

        /// <summary>
        /// Initializes a new observer task
        /// </summary>
        /// <param name="description">The text description for this task</param>
        /// <param name="quiet">Whether or not the base class should do any logging</param>
        protected ObserverTask(string description, bool quiet = false)
        {
            _description = description;
            _quiet = quiet;
        }

        /// <summary>
        /// Initialize this task for execution
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// Start executing this task
        /// </summary>
        public virtual void Start()
        {
            IsStarted = true;
            // Subscribe to events of type T
            _subscription = EventBus.Subscribe(this);
        }

        /// <summary>
        /// Executes the workload for this scheduled task.
        /// </summary>
        /// <returns></returns>
        protected abstract void Execute();

        /// <summary>
        /// Called when this task should be stopped. Ensure when overriding this
        /// method, that you call base.Stop() in your implementation.
        /// </summary>
        /// <param name="immediate">Whether the task should be stopped immediately</param>
        public virtual void Stop(bool immediate = false)
        {
            if (!IsStarted)
                return;

            IsExecuting = false;
            IsStarted   = false;

            if (_subscription != null)
            {
                _subscription.Dispose();
                _subscription = null;
            }
        }

        public virtual void Dispose()
        {
            if (_subscription != null)
            {
                _subscription.Dispose();
                _subscription = null;
            }
        }

        /// <summary>
        /// Called for every event in the event stream this task subscribes to
        /// </summary>
        /// <param name="value">The value of the event</param>
        public void OnNext(T value)
        {
            if (!IsStarted)
                return;

            if (!_quiet)
            {
                var isoDate = DateTime.UtcNow.ToISO8601z();
                Logger.Info("{0} - Executing: {1}", isoDate, _description);
            }

            IsExecuting = true;
            Execute();
            IsExecuting = false;
        }

        /// <summary>
        /// Called for any error produced by the event stream
        /// </summary>
        /// <param name="error"></param>
        public void OnError(Exception error)
        {
            Logger.Error(error);
        }

        /// <summary>
        /// Called when the event stream stops publishing
        /// </summary>
        public void OnCompleted()
        {
            Stop();
        }
    }
}
