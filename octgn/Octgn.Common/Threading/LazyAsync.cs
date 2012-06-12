using System;

namespace Octgn.Common.Threading
{
    public static class LazyAsync
    {
        public static void Invoke(Action a)
        {
            a.BeginInvoke(a.EndInvoke, null);
        }
    }
}