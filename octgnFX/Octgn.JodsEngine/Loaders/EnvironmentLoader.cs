using Octgn.Windows;
using System;
using System.Threading.Tasks;

namespace Octgn.Loaders
{
    public class EnvironmentLoader : ILoader
    {
        private readonly log4net.ILog Log
            = log4net.LogManager.GetLogger(typeof(EnvironmentLoader));

        public string Name { get; } = "Environment";

        public Task Load(ILoadingView view) {
            return Task.Run(LoadSync);
        }

        private void LoadSync() {
            Log.Info("Getting Launcher");

            Program.Launcher = CommandLineHandler.Instance.HandleArguments(
                Environment.GetCommandLineArgs());

            if (Program.Launcher == null) {
                Log.Warn($"no launcher from command line args");

                return;
            }

            Program.DeveloperMode = CommandLineHandler.Instance.DevMode;
        }
    }
}
