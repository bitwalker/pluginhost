using System;

using PluginHost.Interface.Shell;

namespace PluginHost.App.Shell.Commands
{
    public class ExitCommand : Command
    {
        public ExitCommand() : base("exit", "Exits the application.") {}

        public override bool CanExecute(ShellInput input)
        {
            if (StringComparer.InvariantCultureIgnoreCase.Equals("exit", input.Command))
                return true;
            return false;
        }

        public override void Execute(params string[] arguments)
        {
            Application.Current.Stop();
        }
    }
}
