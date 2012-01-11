using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Skylabs.LobbyServer
{
    public static class LockLogger
    {
        int curIndent = 0;
        string tab
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
            Console.WriteLine(String.Format("{2}[tryLock({0})]{1}")LockObjectName,MethodName,tab);
        }
        public static void L(string MethodName, string LockObjectName)
        {
            Console.WriteLine(String.Format("{2}[inLock({0})]{1}")LockObjectName,MethodName,tab);
            curIndent++;
        }
        public static void UL(string MethodName, string LockObjectName)
        {
            Console.WriteLine(String.Format("{2}[unLock({0})]{1}")LockObjectName,MethodName,tab);
            curIndent--;
        }
        public static void E(string MethodName, string LockObjectName, string e)
        {
                Console.WriteLine(String.Format("{2}[Lockevent({0})]{1}:{3}")LockObjectName,MethodName,tab,e);
        }
    }
}
