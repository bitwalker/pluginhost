using System;

namespace PluginHost.Interface.Tasks
{
    /// <summary>
    /// Used as an event type for scheduled tasks.
    /// </summary>
    public class Tick
    {
        /// <summary>
        /// The number of ticks representing the date and time
        /// that this tick was generated.
        /// </summary>
        public long CurrentTicks { get; private set; }

        public Tick()
        {
            CurrentTicks = DateTime.UtcNow.Ticks;
        }
    }
}