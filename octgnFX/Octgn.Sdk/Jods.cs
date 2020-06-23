using Octgn.Sdk.Extensibility;
using Octgn.Sdk.Extensibility.MainMenu;
using Octgn.Sdk.Packaging;
using System;
using System.Collections.Generic;

namespace Octgn.Sdk
{
    public class JodsPackage : IPackageExtension
    {
        public void Config(PackageConfig config) {
            config.Register<DefaultJodsMainMenu>();
        }
    }

    public class DefaultJodsMainMenu : Plugin, IMainMenuPlugin
    {
        public IEnumerable<IMenuItem> MenuItems { get; }

        public MainMenuPluginTheme Theme { get; }

        public DefaultJodsMainMenu(IPackage package)
            : base(package) {
            MenuItems = new IMenuItem[] {

            };
        }
    }
}
