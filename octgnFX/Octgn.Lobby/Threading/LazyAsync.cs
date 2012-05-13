using System;

namespace Skylabs.Lobby.Threading
{
    public static class LazyAsync
    {
        public static void Invoke(Action a)
        {
            a.BeginInvoke(a.EndInvoke, null);
        }
    }
}