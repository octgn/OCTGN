namespace Octgn.Play
{
    using System;

    internal interface ITraceChatHandler
    {
        void ReplaceText(object sender, TraceEventArgs e);
    }

    public class TraceEventArgs : EventArgs
    {
        public bool TraceNotification { get; set; }
    }
}