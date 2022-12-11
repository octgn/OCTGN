//using Octgn.Sdk.Extensibility;
//using Octgn.Sdk.Extensibility.Desktop;
//using System;

//namespace Octgn.DesktopIntegration
//{
//    [PluginDetails(TypeName)]
//    public class JodsGameMenu : MenuPlugin
//    {
//        public const string TypeName = "octgn.jodsengine.gamemenu";

//        public JodsGameMenu() {
//            this.MenuItems.Add(new SinglePlayerMenuItem());
//            this.MenuItems.Add(new MenuItem() {
//                Id = "octgn.jodsengine.gamemenu.multiplayer",
//                Text = "MultiPlayer"
//            });
//            this.MenuItems.Add(new MenuItem() {
//                Id = "octgn.jodsengine.gamemenu.deckeditor",
//                Text = "Deck Editor",
//                Click = "launch \"%JODSENGINE_DEBUGPATH%\" -e -p"
//            });
//        }
//    }
//}
