using System.Threading;

namespace PluginHost.App.Tasks
{
    public interface IEventLoop
    {
        /// <summary>
        /// Starts the event loop
        /// </summary>
        void Start();

        /// <summary>
        /// An alternate start method which allows us to proactively stop execution of
        /// the event loop via a CancellationToken.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token to watch</param>
        void Start(CancellationToken cancellationToken);

        void Stop(bool immediate);
    }
}