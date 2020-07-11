using System;

namespace Octgn.Desktop.Interfaces.Easy
{
    public class NavigateEventArgs : EventArgs
    {
        public Screen Destination { get; set; }

        public bool IsHandled { get; set; }
    }
}