using System.Configuration;

namespace PluginHost.App.Configuration.Elements
{
    using PluginHost.Interface.Configuration.Types;

    public class ShellElement : ConfigurationElement, IShellConfiguration
    {
        /// <summary>
        /// The prompt to display when the shell is active
        /// </summary>
        [ConfigurationProperty("prompt", IsRequired = true)]
        public string Prompt
        {
            get { return (string) this["prompt"]; }
            set { this["prompt"] = value; }
        }
    }
}
