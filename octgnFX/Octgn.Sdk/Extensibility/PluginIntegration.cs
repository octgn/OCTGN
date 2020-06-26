using Octgn.Sdk.Data;
using Octgn.Sdk.Extensibility.MainMenu;
using System;

namespace Octgn.Sdk.Extensibility
{
    public class PluginIntegration
    {
        public T Load<T>(PluginRecord pluginRecord) where T : IPlugin {
            var mmp = new MainMenuPlugin() {
                Theme = new MainMenuPluginTheme() {
                    Background = "Red"
                }
            };

            return (T)(IPlugin)mmp;
        }
    }
}
