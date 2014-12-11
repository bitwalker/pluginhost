using System;

namespace SharpTools.Configuration
{
    public class DecryptConfigException : Exception
    {
        public DecryptConfigException() : base("Unable to decrypt configuration") {}
        public DecryptConfigException(string message) : base(message) {}
        public DecryptConfigException(string message, Exception inner) : base(message, inner) {}
    }
}
