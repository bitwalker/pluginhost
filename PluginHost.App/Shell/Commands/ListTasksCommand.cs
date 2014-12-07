using System;
using System.Linq;
using PluginHost.Extensions.Collections;
using PluginHost.Interface.Shell;

namespace PluginHost.App.Shell.Commands
{
    public class ListTasksCommand : Command
    {
        public ListTasksCommand() : base("tasks", "List available tasks and their status.") {}

        public override bool CanExecute(ShellInput input)
        {
            if (StringComparer.InvariantCultureIgnoreCase.Equals("tasks", input.Command))
                return true;
            return false;
        }

        public override void Execute(params string[] args)
        {
            var available = Application.Current.Tasks.AvailableTasks.ToArray();
            if (available.Length > 0)
            {
                available.Map(t =>
                {
                    Console.Write(t.Key);
                    Console.Write("\t");
                    if (t.Value.IsStarted)
                        WriteSuccess("Started");
                    else
                        WriteNominal("Stopped");
                    Console.Write(", ");
                    if (t.Value.IsExecuting)
                        WriteSuccess("Executing");
                    else
                        WriteNominal("Idle");
                    Console.Write(Environment.NewLine);
                });
            }
            else
            {
                Console.WriteLine("There are no tasks currently available.");
            }
        }

        private void WriteSuccess(string statusText)
        {
            WriteColor(ConsoleColor.Green, statusText);
        }

        private void WriteNominal(string statusText)
        {
            WriteColor(ConsoleColor.Yellow, statusText);
        }

        private void WriteError(string statusText)
        {
            WriteColor(ConsoleColor.Red, statusText);
        }
    }
}
