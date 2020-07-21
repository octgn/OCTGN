using Octgn.Core;
using Octgn.Windows;
using System;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media;

namespace Octgn.Loaders
{
    public class GraphicsLoader : ILoader
    {
        private readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(GraphicsLoader));

        public string Name { get; } = "Graphics";

        public Task Load(ILoadingView view) {
            return Task.Run(LoadSync);
        }

        private void LoadSync() {
            try {
                Log.Debug("Setting rendering mode.");
                RenderOptions.ProcessRenderMode = Prefs.UseHardwareRendering ? RenderMode.Default : RenderMode.SoftwareOnly;
            } catch (Exception ex) {
                // if the system gets mad, best to leave it alone.
                Log.Warn($"Error setting rendering mode: {ex.Message}", ex);
            }
        }
    }
}
