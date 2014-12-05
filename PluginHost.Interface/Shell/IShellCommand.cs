namespace PluginHost.Interface.Shell
{
    public interface IShellCommand
    {
        /// <summary>
        /// The name of this command
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Describes this commands functionality
        /// </summary>
        string Description { get; }
        /// <summary>
        /// Given a user input string, this method determines whether or
        /// not this command instance can handle that input.
        /// </summary>
        /// <param name="input">The command + args entered by the user</param>
        /// <returns>Boolean</returns>
        bool CanExecute(ShellInput input);
        /// <summary>
        /// Given a user input string, this method executes the command
        /// based on that input.
        /// </summary>
        /// <param name="args">The arguments provided by the user</param>
        void Execute(params string[] args);
    }
}
