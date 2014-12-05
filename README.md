# PluginHost

This project is a proof-of-concept for an application which provides
runtime extensibility via plugins. This application is written in C#,
using .NET v4.5, and is composed of the following components:

## Components

### PluginHost

This project contains the core application logic for the command shell,
dependency management, configuration, and task management. It also contains
the default set of shell commands, and the default ILogger implementations.

### PluginHost.Interface

This project contains shared interfaces necessary for plugins to extend
the command shell, reference and add logging modules, add new tasks, and
reference the parent application's configuration.

### PluginHost.Extensions

This project contains shared common code extensions: a generic comparer
implementation, extension methods for collections, enums, streams, numbers,
strings, and datetime/timespans.

### PluginHost.Heartbeat

An example task which acts as a simple heartbeat by logging an event every
5 seconds.

## Dependency Injection

Injection is handled via the Managed Extensibility Framework (MEF). The
configuration for how dependencies are imported/exported is defined via
convention in the PluginHost.Dependencies.DependencyInjector class. It
works like so:

- On application startup, MEF loads a catalog of exported dependencies,
based on the configured conventions, by loading types from the PluginHost
assembly, as well as any assemblies in the `plugins` path as defined in
`App.config`. 
- In addition to this behavior, it has been extended with a custom directory
watcher which monitors that plugin directory, and tells MEF to reload the
catalog when files are added or removed from that directory. An event is published
which can be subscribed to in order to act on new/removed dependencies (this
is done for implementations of ITask for example).
- To import an instance of a dependency, calling one of the Resolve variations
on the `DependencyInjector` class will fetch the type information for the dependency,
find the constructor the the largest number of parameters, and attempt to fulfill the
requirements of that constructor based on available exports. In this way, dependencies
can be automatically injected into instances on creation.
- If you have an instance of an object that is not tracked by MEF, but for which public
properties of exported types exist, you can inject those properties by using the `Inject`
method of the `DependencyInjector` class.

## Command Shell

The shell is a simple REPL-style interface, which provides a prompt, waits for
user input, and responds to that input by looking up commands which can handle the
given input and executing that command.

Commands all implement the IShellCommand interface, and expose two core methods,
`CanExecute` and `Execute`. When user input is received by the shell, it is parsed
into command text and a list of arguments by the CommandParser, and then commands are
resolved via MEF, and each one is asked whether it can handle the given input. If only a
single command responds, it is executed immediately. If more than one command can handle
the input, the user is prompted to choose which command they wish to execute, and then the
command is executed.

Some default commands are provided directly in the PluginHost project: `exit`, `clear`,
`start`, `tasks`, and `help`. New commands can be provided by extending `IShellCommand`,
and adding the containing assembly to the plugins directory.

## Tasks

Tasks all implement the `ITask` interface, and are loaded at startup via MEF. In addition,
any tasks added via new assemblies in the plugins directory will be added during runtime,
and automatically started if `TaskManager` is running.

Tasks can execute on a schedule (via `ScheduledTask`), in response to an event (via `ObserverTask`),
or based on their own behavior entirely, as either one-off tasks, or with their own lifecycle management.

## Logging

Logging is exposed via the `ILogger` interface, and contains two default implementations, `ConsoleLogger`,
and `EventLogLogger`, both of which do pretty much what you'd expect. You can add new loggers by creating
an implementation of `ILogger`, and adding the containing assembly to the plugins directory. Right now no
work has yet been done on providing a single interface for writing to all loggers, you basically have to
import all of them and write to all of them, or choose one to log with and import just that one.

## License

MIT