using Octgn.Utils;
using Octgn.Windows;
using System;
using System.Threading.Tasks;

namespace Octgn.Loaders
{
    public class NetworkLoader : ILoader
    {
        private readonly log4net.ILog Log
            = log4net.LogManager.GetLogger(typeof(NetworkLoader));

        public string Name { get; } = "Network";

        public Task Load(ILoadingView view) {
            return Task.Run(LoadSync);
        }

        private void LoadSync() {
            Program.SSLHelper = new SSLValidationHelper();

            Log.Info("Setting api path");
            Octgn.Site.Api.ApiClient.DefaultUrl
                = new Uri(AppConfig.WebsitePath);
        }
    }
}
