using System;
using System.Reactive.Linq;

using PluginHost.Extensions.Time;
using PluginHost.Interface.Logging;

namespace PluginHost.Interface.Tasks
{
    /// <summary>
    /// The abstract base class for tasks which should be executed on some interval.
    /// Tasks of this type are executed like cron jobs/scheduled tasks, and can be used
    /// for heartbeats, backup jobs, indexing jobs, clean up, etc.
    /// </summary>
    public abstract class ScheduledTask : IObserver<Tick>, ITask
    {
        private bool _shuttingDown = false;
        private IDisposable _subscription;
        private readonly TimeSpan _interval;
        private readonly string _description;
        private readonly bool _quiet;

        public abstract bool IsInitialized { get; protected set; }
        public bool IsStarted { get; private set; }
        public bool IsExecuting { get; private set; }
        protected abstract IEventBus EventBus { get; set; }
        protected abstract ILogger Logger { get; set; }

        /// <summary>
        /// Initializes a new scheduled task.
        /// </summary>
        /// <param name="description">The text description for this task</param>
        /// <param name="interval">The interval on which this task will execute</param>
        /// <param name="quiet">Whether or not the base class should do any logging</param>
        protected ScheduledTask(string description, TimeSpan interval, bool quiet = false)
        {
            _description = description;
            _interval = interval;
            _quiet = quiet;
        }

        public virtual void Start()
        {
            IsStarted = true;
            // Subscribe to Tick events, throttled by the interval
            // this scheduled task executes on.
            _subscription = EventBus.Subscribe(this, ticks => ticks.Sample(_interval));
        }

        public void OnNext(Tick value)
        {
            if (_shuttingDown)
                return;

            if (!_quiet)
            {
                var isoDate = new DateTime(value.CurrentTicks).ToISO8601z();
                Logger.Info("{0} - Executing scheduled task: {1}", isoDate, _description);
            }

            IsExecuting = true;
            Execute();
            IsExecuting = false;
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

            IsStarted   = false;
            IsExecuting = false;

            _shuttingDown = true;
            _subscription.Dispose();
        }
    }
}