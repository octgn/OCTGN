using System;
using System.Threading.Tasks;

namespace Octgn.Sdk.Extensibility.MainMenu
{
    public interface IMainMenuPlugin : IPlugin
    {
        MainMenuPluginTheme Theme { get; }
    }

    public class MainMenuPluginTheme
    {
        public string Background { get; set; }

        public string MenuBackground { get; set; }

        public string ButtonBackground { get; set; }

        public string ButtonBackgroundHover { get; set; }

        public string ButtonBorder { get; set; }

        public string ButtonFont { get; set; }
    }

    public interface IMenuItem
    {
        string Text { get; }

        Task OnClick();
    }
}
