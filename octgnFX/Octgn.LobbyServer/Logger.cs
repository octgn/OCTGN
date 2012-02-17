using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Skylabs.LobbyServer
{
    public static class Logger
    {
        private const int CurIndent = 1;

        private static string Tab
        {
            get
            {
                var sb = new StringBuilder();
                for (int i = 0; i < CurIndent; i++)
                {
                    sb.Append("    ");
                }
                return sb.ToString();
            }
        }

        public static void Er(Exception e, params string[] extras)
        {
            var st = new StackTrace();
            StackFrame[] frames = st.GetFrames();
            string methodName = "UnknownMethod";
            if (frames != null)
                for (int i = 0; i < frames.Length; i++)
                {
                    if (frames[i].GetMethod().Name != MethodBase.GetCurrentMethod().Name) continue;
                    if (i + 1 >= frames.Length) continue;
                    methodName = frames[i + 1].GetMethod().Name;
                    break;
                }
            Console.WriteLine(String.Format("{1}[{4}:{5}][ERROR({2})]{0}:{3}", methodName, Tab,
                                            Thread.CurrentThread.ManagedThreadId, e.Message,
                                            DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString()));
            Console.WriteLine("==========StackTrace==========");
            Console.WriteLine(e.StackTrace ?? st.ToString());
            Console.WriteLine("=============END==============");
            foreach (string s in extras)
                Console.WriteLine(s);
        }
    }
}