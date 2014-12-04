using System;

namespace PluginHost.Interface.Tasks
{
    /// <summary>
    /// Defines the client API for an observable event publisher.
    /// </summary>
    public interface IEventBus : IDisposable
    {
        /// <summary>
        /// Subscribe to a type of event.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to subscribe to</typeparam>
        /// <param name="subscriber">The subscribing instance</param>
        /// <returns>
        /// The subscription as an IDisposable. 
        /// Disposing the subscription will unsubscribe the subscriber from further events.
        /// </returns>
        IDisposable Subscribe<TEvent>(IObserver<TEvent> subscriber)
            where TEvent : class;
        /// <summary>
        /// Subscribe to a type of event and apply a eventFilter over the observable stream of events
        /// to further refine what events are received by the subscriber.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to subscribe to</typeparam>
        /// <param name="subscriber">The subscribing instance</param>
        /// <param name="eventFilter">
        /// A function which takes in an observable, and returns a new one with additional
        /// filters applied to it. Use this to refine what events are received by the subscriber.
        /// </param>
        /// <returns>
        /// The subscription as an IDisposable. 
        /// Disposing the subscription will unsubscribe the subscriber from further events.
        /// </returns>
        IDisposable Subscribe<TEvent>(IObserver<TEvent> subscriber, Func<IObservable<TEvent>, IObservable<TEvent>> eventFilter)
            where TEvent : class;
        /// <summary>
        /// Publish a message to all subscribers of the given type of event.
        /// </summary>
        /// <typeparam name="TEvent">The type of event</typeparam>
        /// <param name="event">The event object to push to all subscribers.</param>
        void Publish<TEvent>(TEvent @event)
            where TEvent : class;
        /// <summary>
        /// Stop the publisher from publishing further messages. It is recommended that
        /// publishers call the OnCompleted event of all subscriptions to ensure that
        /// subscribers are able to clean up their resources and dispose their subscriptions.
        /// </summary>
        void Stop();
    }
}
