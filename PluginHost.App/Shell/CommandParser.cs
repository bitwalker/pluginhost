using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using PluginHost.App.Dependencies;
using PluginHost.Interface.Shell;

namespace PluginHost.App.Shell
{
    public class CommandParser
    {
        private static readonly Regex _commandPattern = new Regex(@"(?x)
          ^([A-Za-z]{1}[0-9A-Za-z_-]+)              # Command name, at least one alpha, followed by alphanumeric, _ or -
           (?:(?:\s+?(?:(?:""([^""]*)"")|(\S*)))+)? # followed by any number of quoted/unquoted arguments
	    ", RegexOptions.Singleline);

        public IEnumerable<IShellCommand> Parse(string input)
        {
            var parsed = ParseInput(input);
            if (!parsed.IsValid)
                yield break;

            var handlers  = new List<IShellCommand>();
            var available = GetCommands();
            foreach (var command in available)
            {
                if (command.Value.CanExecute(parsed))
                    yield return command.Value;
            }
        }

        public ShellInput ParseInput(string input)
        {
            if (!_commandPattern.IsMatch(input))
                return ShellInput.Invalid(input);

            var match   = _commandPattern.Match(input);
            var command = match.Groups[1].Value;
            var args    = GatherArguments(match).ToArray();

            if (string.IsNullOrWhiteSpace(command))
                return ShellInput.Invalid(input);

            if (args.Length == 0)
                return ShellInput.Valid(command);

            var result = ShellInput.Valid(command);
            result.Arguments.AddRange(args);

            return result;
        }

        private IEnumerable<Lazy<IShellCommand>> GetCommands()
        {
            return DependencyInjector.Current
                .LazyResolveMany<IShellCommand>();
        }

        private static IEnumerable<string> GatherArguments(Match match)
        {
            // Gather quoted arguments
            var quoted = new Capture[match.Groups[2].Captures.Count];
            match.Groups[2].Captures.CopyTo(quoted, 0);
            // Gather unquoted arguments
            var unquoted = new Capture[match.Groups[3].Captures.Count];
            match.Groups[3].Captures.CopyTo(unquoted, 0);
            // Join them together and order them as they appear in the input
            return quoted.Concat(unquoted)
                .OrderBy(c => c.Index)
                .Select(c => c.Value);
        }
    }
}
