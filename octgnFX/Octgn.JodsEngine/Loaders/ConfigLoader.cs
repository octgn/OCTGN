using Octgn.Library;
using Octgn.Windows;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Octgn.Loaders
{
    public class ConfigLoader : ILoader
    {
        public string Name { get; } = "Config";

        public async Task Load(ILoadingView view) {
            await Task.Run(() => {
                LoadConfig();
            });
        }

        protected void LoadConfig() {
            lock (Config.Sync) {
                if (Config.Instance != null) {
                    return; // already configured
                }

                Config.Instance = new Config();
            }

            Environment.SetEnvironmentVariable(
                "OCTGN_DATA",
                Config.Instance.DataDirectoryFull,
                EnvironmentVariableTarget.Process);

            var path = Path.Combine(
                Config.Instance.Paths.ConfigDirectory,
                "TEST"
            );

            // Check for test mode
            Program.IsReleaseTest = File.Exists(path);
        }
    }
}
