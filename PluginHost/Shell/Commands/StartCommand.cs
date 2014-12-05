using System;

using PluginHost.Interface.Shell;

namespace PluginHost.Shell.Commands
{
    public class StartCommand : Command
    {
        public StartCommand() : base("start", "Starts task execution.") {}


        public override bool CanExecute(ShellInput input)
        {
            if (StringComparer.InvariantCultureIgnoreCase.Equals("start", input.Command))
                return true;
            return false;
        }

        public override void Execute(params string[] arguments)
        {
            if (!Program.App.Tasks.IsStarted)
            {
                Program.App.Tasks.Start();
            }
            else
            {
                if (arguments.Length > 0)
                {
                    foreach (var taskName in arguments)
                    {
                        Program.App.Tasks.InitTask(taskName);
                        Program.App.Tasks.StartTask(taskName);
                    }
                }
            }
        }
    }
}
