using System;

using PluginHost.Interface.Shell;

namespace PluginHost.App.Shell.Commands
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
            if (!Application.Current.Tasks.IsStarted)
            {
                Application.Current.Tasks.Start();
            }
            else
            {
                if (arguments.Length > 0)
                {
                    foreach (var taskName in arguments)
                    {
                        Application.Current.Tasks.InitTask(taskName);
                        Application.Current.Tasks.StartTask(taskName);
                    }
                }
            }
        }
    }
}
