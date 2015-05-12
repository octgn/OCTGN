using System;

namespace Octgn.Networking
{
    internal class ServerErrorEventArgs : EventArgs
    {
        public string Message { get; set; }
        public bool Handled { get; set; }
    }
}