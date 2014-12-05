using System;
using System.Threading;
using System.Threading.Tasks;

using PluginHost.Shell;
using PluginHost.Extensions.Time;

namespace PluginHost
{
    class Program
    {
        internal static Application App;
        internal static CommandShell Shell;
        private  static CancellationTokenSource _tokenSource;

        /// <summary>
        /// Main entry point for the application
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Task t = MainAsync(args);
            t.Wait();
        }

        /// <summary>
        /// Handles application start on another thread, only called from Main
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        async static Task MainAsync(string[] args)
        {
            _tokenSource = new CancellationTokenSource();

            App = new Application();
            App.Init();

            Shell = new CommandShell(_tokenSource.Token);
            await Shell.Start();
        }

        public static void Shutdown()
        {
            if (_tokenSource != null)
                _tokenSource.Cancel(throwOnFirstException: false);
            if (App != null)
                App.Stop();
            if (Shell != null)
                Shell.Shutdown();

            // Wait a sec to give everything time to shut down
            Task.Delay(1.Seconds()).Wait();
        }
    }
}
