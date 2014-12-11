using System.Threading.Tasks;

namespace SharpTools.Configuration
{
    /// <summary>
    /// This interface describes the API for classes which serialize/deserialize
    /// instances of IConfiguration. This could be to a app/web.config file, a json
    /// file, a database, and more.
    /// </summary>
    public interface IProvider<T> where T : class, IConfig<T>, new()
    {
        /// <summary>
        /// Reads configuration from the default source.
        /// </summary>
        /// <returns>T</returns>
        T Read();
        /// <summary>
        /// Asynchronously reads configuration from the default source.
        /// </summary>
        /// <returns>T</returns>
        Task<T> ReadAsync();
        /// <summary>
        /// Reads configuration from the given string.
        /// </summary>
        /// <param name="config">The stringified configuration</param>
        /// <returns>T</returns>
        T Read(string config);
        /// <summary>
        /// Asynchronously reads configuration from the given string.
        /// </summary>
        /// <param name="config">The stringified configuration</param>
        /// <returns>T</returns>
        Task<T> ReadAsync(string config);

        /// <summary>
        /// Saves the configuration.
        /// </summary>
        void Save(T config);
        /// <summary>
        /// Asynchronously saves the configuration.
        /// </summary>
        Task SaveAsync(T config);
    }
}
