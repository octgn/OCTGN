namespace KellyElton.SignalR.TypeSafe
{
    using System.Reflection;

    public class MethodCallInfo
    {
        public MethodInfo Method { get; set; }
        public object[] Args { get; set; }

    }
}