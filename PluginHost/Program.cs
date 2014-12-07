namespace PluginHost
{
    using System;
    using System.Threading.Tasks;
    using System.Reactive.Linq;

    using PluginHost.App;
    using PluginHost.App.Configuration;
    using System.Reactive;
    using System.Reactive.Subjects;

    class Program : MarshalByRefObject
    {
        private static readonly Type _applicationType = typeof (Application);

        private AppDomain _domain;
        private Application _currentApplication;
        private Subject<DateTime> OnShutdown;

        private Program()
        {
            OnShutdown = new Subject<DateTime>();
        }

        /// <summary>
        /// Main entry point for the application
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            try
            {
                var program = new Program();
                program.Run(args).Wait();
            }
            catch (AggregateException ex)
            {
                ex.Handle(e =>
                {
                    ConsoleExtended.Error(e.Message);
                    return true;
                });
            }

            // Wait for input so user has chance to read logs
            Console.Read();
        }

        /// <summary>
        /// Handles application start on another thread, only called from Main
        /// </summary>
        private Task Run(string[] args)
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
            return OnShutdown.ForEachAsync(OnExit);
        }

        private void LoadApplication(bool reloading = false)
        {
            // Create instance of Application in new domain
            _currentApplication = null;
            _currentApplication = (Application) _domain.CreateInstanceAndUnwrap(
                _applicationType.Assembly.FullName,
                _applicationType.FullName
            );

            // Handle reloading the application when requested
            _currentApplication.OnReload += OnReload;
            // Shutdown the program if requested by the application
            _currentApplication.OnShutdown += Shutdown;

            // Start the app
            _currentApplication.Start();
        }

        private void OnReload(object sender, EventArgs e)
        {
            LoadApplication(reloading: true);
        }

        private void Shutdown(object sender, EventArgs e)
        {
            OnShutdown.OnNext(DateTime.UtcNow);
            OnShutdown.OnCompleted();
        }

        private void OnExit(DateTime now)
        {
            ConsoleExtended.Alert("Application unloaded!");
            AppDomain.Unload(_domain);
        }
    }
}
