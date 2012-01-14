using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Skylabs.LobbyServer
{
    public static class Logger
    {
        static int curIndent = 0;
        static string tab
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < curIndent; i++)
                {
                    sb.Append("    ");
                }
                return sb.ToString();
            }
        }
        public static void TL(string MethodName, string LockObjectName)
        {
            Console.WriteLine(String.Format("{2}[{4}:{5}][tryLock({3},{0})]{1}", LockObjectName, MethodName, tab, Thread.CurrentThread.ManagedThreadId, DateTime.Now.ToShortDateString(),DateTime.Now.ToShortTimeString()));
        }
        public static void L(string MethodName, string LockObjectName)
        {
            Console.WriteLine(String.Format("{2}[{4}:{5}][inLock({3},{0})]{1}", LockObjectName, MethodName, tab, Thread.CurrentThread.ManagedThreadId, DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString()));
            curIndent++;
        }
        public static void UL(string MethodName, string LockObjectName)
        {
            curIndent--;
            Console.WriteLine(String.Format("{2}[{4}:{5}][unLock({3},{0})]{1}", LockObjectName, MethodName, tab, Thread.CurrentThread.ManagedThreadId, DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString()));
        }
        public static void LE(string MethodName, string LockObjectName, string e)
        {
            Console.WriteLine(String.Format("{2}[{5}:{6}][Lockevent({3},{0})]{1}:{4}", LockObjectName, MethodName, tab, Thread.CurrentThread.ManagedThreadId, e, DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString()));
        }
    }
}
