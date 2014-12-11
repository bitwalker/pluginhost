using System;

namespace SharpTools.Configuration
{
    public class EncryptConfigException : Exception
    {
        public EncryptConfigException() : base("Unable to encrypt configuration") {}
        public EncryptConfigException(string message) : base(message) {}
        public EncryptConfigException(string message, Exception inner) : base(message, inner) {}
    }
}
