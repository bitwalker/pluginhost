namespace SharpTools.Configuration
{
    /// <summary>
    /// This interface describes the required API for code-based configuration classes.
    /// It is automatically included when you implement the abstract BaseConfiguration
    /// class, but it is not required to do so.
    /// </summary>
    public interface IConfig<T>
        where T : class, new()
    {
        /// <summary>
        /// Gets the encryption key to use when encrypting config values.
        /// For example, you could return a constant string, or read in a
        /// private key. It's best practice to store this key in a file
        /// exernally, and read it in when needed.
        /// </summary>
        string GetEncryptionKey();
    }
}
