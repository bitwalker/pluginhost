using System;

namespace SharpTools.Configuration
{
    public class WriteConfigException : Exception
    {
        public WriteConfigException(string message) : base(message) {}
        public WriteConfigException(string message, Exception inner) : base(message, inner) {}
    }
}
