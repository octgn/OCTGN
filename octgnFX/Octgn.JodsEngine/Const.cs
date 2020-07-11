namespace Octgn
{
    using System;

    public static class Const
    {
        public const string ClientName = "Octgn.JodsEngine.NET";
        public static readonly Version OctgnVersion = typeof(Const).Assembly.GetName().Version;
        public static readonly Version BackwardCompatibility = new Version(3,4,0,0);
    }
}