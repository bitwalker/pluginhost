using System.Collections.Generic;

namespace PluginHost.Interface.Shell
{
    public struct ShellInput
    {
        public bool IsValid { get; set; }
        public string Command { get; set; }
        public List<string> Arguments { get; set; }

        public static ShellInput Valid(string command)
        {
            return new ShellInput()
            {
                IsValid   = true,
                Command   = command,
                Arguments = new List<string>()
            };
        }

        public static ShellInput Invalid(string input)
        {
            return new ShellInput()
            {
                IsValid   = false,
                Command   = input,
                Arguments = new List<string>()
            };
        }
    }
}
