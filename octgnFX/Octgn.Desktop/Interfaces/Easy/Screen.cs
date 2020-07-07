using System;
using System.Windows.Controls;

namespace Octgn.Desktop.Interfaces.Easy
{
    public abstract class Screen : UserControl
    {
        public NavigationService NavigationService { get; internal set; }
    }
}
