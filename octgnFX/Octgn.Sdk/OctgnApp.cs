using System;
using System.Collections.ObjectModel;

namespace Octgn.Sdk
{
    public class OctgnApp
    {
        public ObservableCollection<IMainMenuPlugin> MainMenus { get; } = new ObservableCollection<IMainMenuPlugin>();


    }
}
