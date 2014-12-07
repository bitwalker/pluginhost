using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

using PluginHost.Interface.Tasks;

namespace PluginHost.App.Tasks
{
    /// <summary>
    /// This class is responsible for publishing events. It does so via Reactive Extension's
    /// Subject class, which behaves like a proxy. Observers subscribe to a specific kind of event,
    /// and when Publish is called with that type of event, the Subject handles proxying that event
    /// to all the subscribers.
    ///
    /// Every subscription is automatically set up with a filter over the type
    /// of the event object, so that those observers only receive messages of that type. There is an
    /// overloaded version of Subscribe that allows further refinement of the events of that type when
    /// needed.
    ///
    /// For example, subscribing to the Tick event by default will cause you to receive one
    /// message per second, one for every Tick. If you wanted to only receive a Tick once per minute though,
    /// you could pass a filter expression which throttles the subscription by an interval. Throttle is
    /// an extension method on IObservable, and ensures that you receive no more than one event of that type
    /// in a given interval. There are many filters available, and what you need will depend on the situation,
    /// see Rx's documentation for details on what is available and how to use them.
    /// </summary>
    public class EventBus : IEventBus
    {
        /// <summary>
        /// The Subject is both an observer and observable which will proxy
        /// events piped to it on to all it's subscribers.
        /// </summary>
        private Subject<object> _subject;
        private bool _shuttingDown = false;
        private bool _disposed     = false;

        public EventBus()
        {
            _subject = new Subject<object>();
        }

        public void Publish<TEvent>(TEvent @event) where TEvent : class
        {
            if (_shuttingDown)
                return;

            _subject.OnNext(@event);
        }

        public void Stop()
        {
            _shuttingDown = true;
            // This will execute all subscriber's OnCompleted callbacks,
            // which by convention is called whenever a publisher is done
            // publishing events.
            _subject.OnCompleted();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            if (!_shuttingDown)
                Stop();

            _subject.Dispose();
            _disposed = true;
        }

        public IDisposable Subscribe<TEvent>(IObserver<TEvent> subscriber)
            where TEvent : class
        {
            return GetSubscription(subscriber);
        }

        public IDisposable Subscribe<TEvent>(IObserver<TEvent> subscriber, Func<IObservable<TEvent>, IObservable<TEvent>> eventFilter)
            where TEvent : class
        {
            return GetSubscription(subscriber, eventFilter);
        }

        private IDisposable GetSubscription<TEvent>(
            IObserver<TEvent> subscriber,
            Func<IObservable<TEvent>, IObservable<TEvent>> eventFilter = null)
        {
            // If we're shutting down, ignore new subscriptions
            if (_shuttingDown)
                return Disposable.Empty;

            var eventSource = _subject
                // Filter events by object type and cast
                .Where(ev => ev.GetType() == typeof(TEvent))
                .Cast<TEvent>();

            // Allow the subscriber to select more precisely what events they care about
            IObservable<TEvent> filteredEventSource;
            if (eventFilter != null)
                filteredEventSource = eventFilter(eventSource);
            else
                filteredEventSource = eventSource;

            return filteredEventSource
                // Execute all subscriber callbacks on a new thread
                .ObserveOn(new EventLoopScheduler())
                // Return the subscription disposable to the subscriber,
                // so that they may unsubscribe themsleves
                .Subscribe(subscriber);
        }
    }
}
