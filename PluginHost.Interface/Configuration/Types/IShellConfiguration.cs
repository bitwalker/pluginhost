namespace PluginHost.Interface.Configuration.Types
{
    using PluginHost.Interface.Logging;

    public interface IShellConfiguration
    {
        /// <summary>
        /// The string to display in the shell prompt
        /// </summary>
        string Prompt { get; set; }
    }
}
