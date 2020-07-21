using Octgn.Library.Exceptions;
using Octgn.Windows;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Octgn.Loaders
{
    public sealed class Loader : ILoader
    {
        private readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(Loader));

        public string Name { get; } = "Main";

        public List<ILoader> Loaders { get; } = new List<ILoader>();

        public Loader() {
        }

        public async Task Load(ILoadingView view) {
            foreach (var loader in Loaders) {
                Log.Info($"Loading {loader.Name}");

                view.UpdateStatus($"Loading {loader.Name}");

                try {
                    await loader.Load(view);
                } catch (Exception ex) {
                    var msg = $"Error loading {loader.Name}: {ex.Message}";

                    Log.Error(msg, ex);

                    throw new UserMessageException(
                        UserMessageExceptionMode.Blocking,
                        msg,
                        ex
                    );
                }
            }
        }
    }
}
