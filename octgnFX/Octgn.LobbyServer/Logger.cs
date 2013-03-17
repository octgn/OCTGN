using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Skylabs.LobbyServer
{

    public static class Logger
    {
        public static void PreLock()
        {
            var method = GetPreviousMethod();
            //WriteLine("#PreLock[{1}]: {0}",method,Thread.CurrentThread.ManagedThreadId);
            WriteTag("PreLock");
        }
        public static void InLock()
        {
            var method = GetPreviousMethod();
            //WriteLine("#InLock[{1}]: {0}", method, Thread.CurrentThread.ManagedThreadId);
            WriteTag("InLock");
        }
        public static void EndLock()
        {
            var method = GetPreviousMethod();
            //WriteLine("#EndLock[{1}]: {0}", method, Thread.CurrentThread.ManagedThreadId);
            WriteTag("EndLock");
        }
        public static void Er(Exception e, params string[] extras)
        {
            string methodName = GetPreviousMethod();
            WriteLine(String.Format("{1}[{4}:{5}][ERROR({2})]{0}:{3}", methodName, "",
                                            Thread.CurrentThread.ManagedThreadId, e.Message,
                                            DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString()));
            WriteLine("==========StackTrace==========");
            WriteLine(e.StackTrace ?? new StackTrace().ToString());
            WriteLine("=============END==============");
            foreach (string s in extras)
                WriteLine(s);
        }
        private static string GetPreviousMethod(int back = 2)
        {
            StackTrace st =  new StackTrace();
            StackFrame[] frames = st.GetFrames();
            string methodName = "UnknownMethod";
            if (frames != null && frames.Length > (back + 1)) methodName = frames[back].GetMethod().Name;
            return methodName;
        }
        private static string MakeHeader(string tname)
        {
            return String.Format("#{0}[{1}][{2}:{3}][{4}]: " , tname , Thread.CurrentThread.ManagedThreadId ,
                          DateTime.Now.ToShortDateString() , DateTime.Now.ToShortTimeString() , GetPreviousMethod(4));
        }
        private static void WriteTag(string name,string format = "", params object[] args)
        {
            WriteLine(MakeHeader(name) + format,args);
        }
        private static void WriteLine(string format, params object[] args)
        {
            Trace.WriteLine(String.Format(format,args));
        }
    }
}