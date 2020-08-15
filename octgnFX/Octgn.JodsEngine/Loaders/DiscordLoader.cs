using Octgn.Core.DiscordIntegration;
using Octgn.Windows;
using System;
using System.Threading.Tasks;

namespace Octgn.Loaders
{
    public class DiscordLoader : ILoader
    {
        private readonly log4net.ILog Log
            = log4net.LogManager.GetLogger(typeof(DiscordLoader));

        public DiscordLoader()
        {

        }

        public string Name => "Discord";

        public Task Load(ILoadingView view)
        {
            return Task.Run(() =>
            {
                Program.Discord = new DiscordWrapper();

                Program.Discord.Error += Discord_Error;
            });
        }

        private void Discord_Error(object sender, Exception e)
        {
            Log.Warn($"Discord Error: {e.Message}", e);
        }
    }
}
