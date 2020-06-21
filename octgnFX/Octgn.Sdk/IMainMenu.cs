using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Octgn.Sdk
{
    public interface IMainMenuPlugin
    {
        string Background { get; }

        string MenuBackground { get; }

        string ButtonBackground { get; }

        string ButtonBackgroundHover { get; }

        string ButtonBorder { get; }

        string ButtonFont { get; }

        IEnumerable<IMenuItem> MenuItems { get; }

    }

    public interface IMenuItem
    {
        string Text { get; }

        Task OnClick();
    }

    public interface IPlugin
    {
        Task OnStart(OctgnApp app);
    }

    public class DefaultJodsMainMenuPlugin : IPlugin
    {
        public Task OnStart(OctgnApp app) {
            app.MainMenus.Add(new DefaultJodsMainMenu());
            throw new NotImplementedException();
        }
    }

    public class DefaultJodsMainMenu : IMainMenuPlugin
    {
        public string Background => throw new NotImplementedException();

        public string MenuBackground => throw new NotImplementedException();

        public string ButtonBackground => throw new NotImplementedException();

        public string ButtonBackgroundHover => throw new NotImplementedException();

        public string ButtonBorder => throw new NotImplementedException();

        public string ButtonFont => throw new NotImplementedException();

        public IEnumerable<IMenuItem> MenuItems => throw new NotImplementedException();
    }
}
