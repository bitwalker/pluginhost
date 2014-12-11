using System;

namespace SharpTools.Configuration
{
    public class ReadConfigException : Exception
    {
        public ReadConfigException(string message) : base(message) {}
        public ReadConfigException(string message, Exception inner) : base(message, inner) {}
    }
}
