using System;

namespace agsXMPP.Util
{
    static class Runtime
    {
        public static bool IsMono()
        {
            Type t = Type.GetType ("Mono.Runtime");
            if (t != null)
                 return true;
            
            return false;
        }

        public static bool IsUnix()
        {
            int p = (int) Environment.OSVersion.Platform;
            if ((p == 4) || (p == 6) || (p == 128))
                return true;
            
            return false;
        }
    }
}