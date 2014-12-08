using System.Collections.Generic;

namespace PluginHost.Interface.Configuration.Types
{
    /// <summary>
    /// Represents a collection of logger configurations
    /// </summary>
    public interface ILoggersConfiguration : IEnumerable<ILoggerConfiguration>
    {
        ILoggerConfiguration this[string name] { get; }
    }
}
