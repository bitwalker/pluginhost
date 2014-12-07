using System;

using PluginHost.Interface.Shell;

namespace PluginHost.App.Shell.Commands
{
    public class ClearCommand : Command
    {
        public ClearCommand() : base("clear", "Clears the screen.") {}

        public override bool CanExecute(ShellInput input)
        {
            if (StringComparer.InvariantCultureIgnoreCase.Equals("clear", input.Command))
                return true;
            return false;
        }

        public override void Execute(params string[] arguments)
        {
            Console.Clear();
        }
    }
}
