using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PluginHost.Shell
{
    public class CommandShell
    {
        private const string PROMPT = "pluginhost> ";

        private readonly CancellationToken _token;
        private readonly CommandParser _parser;

        private bool _shuttingDown = false;

        public CommandShell(CancellationToken token)
        {
            _token  = token;
            _parser = new CommandParser();

            _token.Register(Shutdown);
        }

        public async Task Start()
        {
            // Make sure CTRL+C is treated as an escape sequence
            Console.TreatControlCAsInput = false;
            Console.CancelKeyPress += (sender, e) => Shutdown();

            Console.Clear();

            await Task.Run(()=>Loop(), _token);
        }

        public void Shutdown()
        {
            _shuttingDown = true;
        }

        /// <summary>
        /// Loops indefinitely, listening to user input.
        /// When the user types Enter or CTRL+C, the app is shut down.
        /// If Shutdown is called directly, the thread the loop executes on
        /// is cancelled.
        /// </summary>
        private void Loop()
        {
            Console.Write(PROMPT);

            // Since C# has no tail-recursion, use goto to emulate
            // infinite recursion to keep the loop going.
        readkey:
            if (_token.IsCancellationRequested || _shuttingDown)
                return;
            // If no key is available, loop every 100ms until one is.
            // This lets us cancel out of the loop if Shutdown is called
            // with no user input
            if (!Console.KeyAvailable)
            {
                Task.Delay(100, _token);
                goto readkey;
            }

            // Get command entered by user
            var input    = Console.ReadLine();

            // If the input is empty, just print the prompt
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.Write(Environment.NewLine);
                Console.Write(PROMPT);
                goto readkey;
            }

            var handlers = _parser.Parse(input).ToArray();
            // Write an empty line for better readability
            Console.WriteLine();
            // If there is more than one command which can handle the given
            // input, prompt the user to select the command they wish to execute
            if (handlers.Length > 1)
            {
                // Ambiguous input, ask the user to clarify
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Command is ambiguous, select one of the following:");
                var options = string.Join(", ", handlers.Select(h => h.Name));
                Console.WriteLine(options);
                Console.ResetColor();

                Console.Write(PROMPT);
                var selection = Console.ReadLine();
                var selected  = handlers.FirstOrDefault(h => h.Name.Equals(selection));
                if (selected != null)
                    selected.Execute();
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid command selection!");
                    Console.ResetColor();
                }
            }
            else if (handlers.Length == 1)
            {
                var command = handlers.First();
                command.Execute(input);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No such command.");
                Console.ResetColor();
            }
            Console.Write(Environment.NewLine);
            Console.Write(PROMPT);

            goto readkey;
        }
    }
}
