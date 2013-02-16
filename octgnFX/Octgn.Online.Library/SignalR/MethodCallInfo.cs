namespace Octgn.Online.Library.SignalR
{
    using System.Reflection;

    public class MethodCallInfo
    {
        public MethodInfo Method { get; set; }
        public object[] Args { get; set; }

    }
}