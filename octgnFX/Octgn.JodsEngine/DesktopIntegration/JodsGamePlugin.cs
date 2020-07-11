using System;
using Octgn.Sdk;
using Octgn.Sdk.Extensibility;

namespace Octgn.DesktopIntegration
{
    public class JodsGamePlugin : Plugin
    {
        public const string TypeName = "octgn.jodsengine.gameplugin";

        public override void Initialize(Package package) {
            base.Initialize(package);

            //var hasMenu = Components.ContainsType(TypeName);

            //if (!hasMenu) {
            //    var menu = new JodsGameMenu();
            //    menu.Initialize(context);
            //    Components.Add(menu);
            //}
        }
    }
}
