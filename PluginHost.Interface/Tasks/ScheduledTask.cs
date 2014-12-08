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

        public bool IsDisposed { get; protected set; }
        public abstract bool IsInitialized { get; protected set; }
        public bool IsStarted { get; protected set; }
        public bool IsExecuting { get; protected set; }
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
            // Subscribe to Tick events, throttled by the interval
            // this scheduled task executes on.
            _subscription = EventBus.Subscribe(this, ticks => ticks.Sample(_interval));
        }

        /// <summary>
        /// Executes the workload for this scheduled task.
        /// </summary>
        protected abstract void Execute();

        /// <summary>
        /// Called when this task should be stopped.
        /// </summary>
        /// <param name="brutalKill">Whether the task should be killed or not</param>
        public virtual void Stop(bool brutalKill)
        {
            if (_shuttingDown)
                return;

            IsStarted = false;
            IsExecuting = false;

            _shuttingDown = true;
            _subscription.Dispose();

            Kill(brutalKill);
        }

        /// <summary>
        /// Called when this task is being shutdown.
        /// </summary>
        /// <param name="brutalKill">Whether this shutdown should be expedited or not.</param>
        protected abstract void Kill(bool brutalKill);

        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Called for every event in the event stream this task has subscribed to
        /// </summary>
        /// <param name="value">The value of the event</param>
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

        /// <summary>
        /// Called when the event stream produces an error
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
            Stop(true);
        }
    }
}