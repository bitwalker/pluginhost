using System;

using PluginHost.Extensions.Time;
using PluginHost.Interface.Tasks;
using PluginHost.Interface.Logging;

namespace PluginHost.Heartbeat
{
    public class HeartbeatTask : ScheduledTask
    {
        public override bool IsInitialized { get; protected set; }
        protected override IEventBus EventBus { get; set; }
        protected override ILogger Logger { get; set; }

        public HeartbeatTask(ILogger logger, IEventBus eventBus)
            : base("Heartbeat", 5.Seconds(), quiet: true)
        {
            Logger   = logger;
            EventBus = eventBus;
        }

        public override void Init()
        {
            IsInitialized = true;
            Logger.Info("Heartbeat plugin loaded!");
        }

        protected override void Execute()
        {
            var isoDate = DateTime.UtcNow.ToISO8601z();
            Logger.Info("{0} - All systems normal.", isoDate);
        }
    }
}
