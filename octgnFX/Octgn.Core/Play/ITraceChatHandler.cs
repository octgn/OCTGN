namespace Octgn.Play
{
    using System;

    public interface ITraceChatHandler
    {
        void Set(string message);
        void ReplaceText(object sender, TraceEventArgs e);
    }

    public class TraceEventArgs : EventArgs
    {
        public bool TraceNotification { get; set; }
    }
}