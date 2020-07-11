using Octgn.Sdk.Extensibility.Desktop;

namespace Octgn.DesktopIntegration
{
    public class JodsGameMenu : MenuPlugin
    {
        public JodsGameMenu() {
            this.MenuItems.Add(new MenuItem() {
                Id = "octgn.jodsengine.gamemenu.singleplayer",
                Text = "Single Player"
            });
            this.MenuItems.Add(new MenuItem() {
                Id = "octgn.jodsengine.gamemenu.multiplayer",
                Text = "MultiPlayer"
            });
            this.MenuItems.Add(new MenuItem() {
                Id = "octgn.jodsengine.gamemenu.deckeditor",
                Text = "Deck Editor"
            });
        }
    }
}
