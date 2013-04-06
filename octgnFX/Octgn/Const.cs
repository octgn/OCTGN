namespace Octgn
{
    using System;

    public static class Const
    {
        public const string ClientName = "Octgn.NET";
        public static readonly Version OctgnVersion = typeof(OctgnApp).Assembly.GetName().Version;
        public static readonly Version BackwardCompatibility = new Version(3,1,0,0);
    }
}