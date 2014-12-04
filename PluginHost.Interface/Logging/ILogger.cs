using System;

namespace PluginHost.Interface.Logging
{
    public interface ILogger
    {
        void Trace(string message);
        void Trace(string message, params object[] args);

        void Info(string message);
        void Info(string message, params object[] args);

        void Success(string message);
        void Success(string message, params object[] args);

        void Warn(string message);
        void Warn(string message, params object[] args);

        void Alert(string message);
        void Alert(string message, params object[] args);

        void Error(string message);
        void Error(string message, params object[] args);
        void Error(Exception ex);
        void Error(Exception ex, string message, params object[] args);
    }
}
