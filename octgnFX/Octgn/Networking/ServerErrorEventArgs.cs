using System;

namespace Octgn
{
	internal class ServerErrorEventArgs : EventArgs
	{
		public string Message { get; set; }
		public bool Handled { get; set; }
	}
}
