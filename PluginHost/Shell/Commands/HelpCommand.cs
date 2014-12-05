using System;
using System.Linq;

using PluginHost.Dependencies;
using PluginHost.Extensions.Collections;
using PluginHost.Interface.Shell;

namespace PluginHost.Shell.Commands
{
    public class HelpCommand : Command
    {
        public HelpCommand() : base("help", "List available commands.") {}

        public override bool CanExecute(ShellInput input)
        {
            if (StringComparer.InvariantCultureIgnoreCase.Equals("help", input.Command))
                return true;
            return false;
        }

        public override void Execute(params string[] arguments)
        {
            var commands = DependencyInjector.Current
                .ResolveMany<IShellCommand>()
                .ToArray();

            if (commands.Length == 0)
            {
                Console.WriteLine("There are no commands available.");
            }
            else
            {
                WriteColor(ConsoleColor.Magenta, "The following commands are available:");
                Console.WriteLine(Environment.NewLine);
                commands.Map(command =>
                {
                    Console.WriteLine("{0,-10}{1}", command.Name, command.Description);
                });
            }
        }
    }
}
