using System.Reactive.Subjects;

namespace PluginHost
{
    using System;
    using System.Threading.Tasks;
    using System.Reactive.Linq;

    using PluginHost.App;
    using PluginHost.App.Configuration;
    using System.Reactive;

    class Program
    {
        private static readonly Type _applicationType = typeof (Application);
        private static AppDomain _domain;
        private static Application _currentApplication;
        private static IDisposable _shutdownSubscription;

        private static Subject<DateTime> OnShutdown;

        /// <summary>
        /// Main entry point for the application
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            OnShutdown = new Subject<DateTime>();

            var task = MainAsync(args);
            task.Wait();
        }

        /// <summary>
        /// Handles application start on another thread, only called from Main
        /// </summary>
        static Task MainAsync(string[] args)
        {
            // Make sure Plugins path is present
            var pluginPath = Config.Current.Paths.Plugins.Info;
            if (!pluginPath.Exists)
                pluginPath.Create();

            // Create new AppDomain in which to run application so we can do hot reloads
            var setup = new AppDomainSetup()
            {
                ShadowCopyFiles = "true",
                ShadowCopyDirectories = pluginPath.FullName
            };
            _domain = AppDomain.CreateDomain("PluginHost_App", AppDomain.CurrentDomain.Evidence, setup);

            LoadApplication();

            // When OnShutdown is called, unload the domain and exit
            return OnShutdown
                .ForEachAsync(datetime =>
                {
                    Console.WriteLine("{0} - Application unloaded!", datetime.ToString("O"));
                    AppDomain.Unload(_domain);
                });
        }

        static void LoadApplication()
        {
            // Create instance of Application in new domain
            _currentApplication = null;
            _currentApplication = (Application) _domain.CreateInstanceAndUnwrap(
                _applicationType.Assembly.FullName,
                _applicationType.FullName
            );

            // Shutdown the program if requested by the application
            _currentApplication.OnShutdown.Subscribe((e) => Shutdown());

            // Start the app
            _currentApplication.Init();
            _currentApplication.Start();
        }

        static void Shutdown()
        {
            OnShutdown.OnNext(DateTime.UtcNow);
            OnShutdown.OnCompleted();
        }
    }
}
