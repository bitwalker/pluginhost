using System;
using PluginHost.Interface.Shell;

namespace PluginHost.App.Shell.Commands
{
    public class ReloadCommand : Command
    {
        public ReloadCommand() : base("reload", "Hot reloads the application core.") {}

        public override bool CanExecute(ShellInput input)
        {
            if (StringComparer.InvariantCultureIgnoreCase.Equals("reload", input.Command))
                return true;
            return false;
        }

        public override void Execute(params string[] arguments)
        {
            Application.Current.Reload();
        }
    }
}
