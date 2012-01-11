using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Skylabs.LobbyServer
{
    public static class LockLogger
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
            Console.WriteLine(String.Format("{2}[tryLock({3},{0})]{1}",LockObjectName,MethodName,tab,Thread.CurrentThread.ManagedThreadId));
        }
        public static void L(string MethodName, string LockObjectName)
        {
            Console.WriteLine(String.Format("{2}[inLock({3},{0})]{1}",LockObjectName,MethodName,tab,Thread.CurrentThread.ManagedThreadId));
            curIndent++;
        }
        public static void UL(string MethodName, string LockObjectName)
        {
            Console.WriteLine(String.Format("{2}[unLock({3},{0})]{1}",LockObjectName,MethodName,tab,Thread.CurrentThread.ManagedThreadId));
            curIndent--;
        }
        public static void E(string MethodName, string LockObjectName, string e)
        {
                Console.WriteLine(String.Format("{2}[Lockevent({3},{0})]{1}:{4}",LockObjectName,MethodName,tab,Thread.CurrentThread.ManagedThreadId,e));
        }
    }
}
