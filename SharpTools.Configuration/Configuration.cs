using System;
using System.Threading.Tasks;

namespace SharpTools.Configuration
{
    /// <summary>
    /// This class provides helper functions for working with IConfig instances.
    /// </summary>
    public static class Configuration
    {
        /// <summary>
        /// Reads configuration from app/web.config using the AppSettingsProvider.
        /// </summary>
        /// <returns>T</returns>
        public static T Read<T>()
            where T : class, IConfig<T>, new()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads configuration using the specified provider.
        /// </summary>
        /// <param name="provider">The configuration provider to read with</param>
        /// <returns>T</returns>
        public static T Read<T>(IProvider<T> provider)
            where T : class, IConfig<T>, new()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads configuration from the provided string using the AppSettingsProvider.
        /// </summary>
        /// <param name="config">The appSettings config section as an xml string.</param>
        /// <returns>T</returns>
        public static T Read<T>(string config)
            where T : class, IConfig<T>, new()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reads configuration from the provided string, using the specified provider.
        /// </summary>
        /// <param name="config">The stringified configuration</param>
        /// <param name="provider">The configuration provider to read with</param>
        /// <returns>T</returns>
        public static T Read<T>(string config, IProvider<T> provider)
            where T : class, IConfig<T>, new()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously reads configuration from the provided string, using
        /// the specified provider.
        /// </summary>
        /// <param name="config">The stringified configuratiaon</param>
        /// <param name="provider">The configuration provider to read with</param>
        /// <returns>T wrapped in a Task</returns>
        public static Task<T> ReadAsync<T>(string config, IProvider<T> provider)
            where T : class, IConfig<T>, new()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves the configuration using the default provider
        /// </summary>
        /// <param name="config">The configuration to save</param>
        /// <returns>T</returns>
        public static T Save<T>(T config)
            where T : class, IConfig<T>, new()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Saves the configuration using the specified provider
        /// </summary>
        /// <param name="config">The configuration to save</param>
        /// <param name="provider">The configuration provider to save with</param>
        /// <returns>T</returns>
        public static T Save<T>(this T config, IProvider<T> provider)
            where T : class, IConfig<T>, new()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asycnhronously saves the configuration using the default provider
        /// </summary>
        /// <param name="config">The configuration to save</param>
        /// <returns>T wrapped in a Task</returns>
        public static Task<T> SaveAsync<T>(this T config)
            where T : class, IConfig<T>, new()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asynchronously saves the configuration using the specified provider
        /// </summary>
        /// <param name="config">The configuration to save</param>
        /// <param name="provider">The configuration provider to save with</param>
        /// <returns>T wrapped in a Task</returns>
        public static Task<T> SaveAsync<T>(this T config, IProvider<T> provider)
            where T : class, IConfig<T>, new()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Merges the source configuration into the target, favoring values from the
        /// source configuration over values in the target when there are conflicts.
        /// This is a deep merge, so lists or dictionaries are also merged.
        /// </summary>
        /// <param name="target">The merge target</param>
        /// <param name="source">The merge source</param>
        public static T Merge<T>(this T target, T source)
            where T : class, IConfig<T>, new()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Merges the source configuration into the target. When conflicts are encountered,
        /// the resolver function is called with the conflict info, and is expected to return
        /// the merged value. Conflicts are defined as any time that the target and source both
        /// contain a key where the values are different. Missing keys from the source are ignored,
        /// and missing keys from the target are added from the source.
        /// </summary>
        /// <typeparam name="T">The type of the configuration objects</typeparam>
        /// <param name="target">The target configuration, into which values will be merged</param>
        /// <param name="source">The source configuration, from which values will be taken</param>
        /// <param name="resolver">A function which will resolve configuration conflicts</param>
        /// <returns>T</returns>
        public static T Merge<T>(this T target, T source, Func<Conflict, object> resolver)
            where T : class, IConfig<T>, new()
        {
            throw new NotImplementedException();
        }
    }
}
