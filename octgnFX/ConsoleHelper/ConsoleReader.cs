using System;

using System.Text;
using System.Threading;

namespace Skylabs.ConsoleHelper
{
    public class ConsoleReader
    {
        public delegate void ConsoleInputDelegate(ConsoleMessage input);

        public static event ConsoleInputDelegate eConsoleInput;
        //public static ConsoleColor InputColor { get { return _InputColor; } set { _InputColor = value; } }

        //private static ConsoleColor _InputColor = ConsoleColor.Gray;

        private static Thread thread;
        private static Thread rlthread;

        public static ThreadState ThreadState
        {
            get
            {
                return thread.ThreadState;
            }
        }

        public static void Start()
        {
            thread = new Thread(run);
            thread.Name = "ConsoleHandThread";
            thread.Start();
        }

        private static void handleInput(ConsoleMessage cm)
        {
            if(eConsoleInput != null)
            {
                if(eConsoleInput.GetInvocationList().Length > 0)
                    eConsoleInput.Invoke(cm);
            }
        }

        public static void Stop()
        {
            if(thread != null)
            {
                if(thread.ThreadState == System.Threading.ThreadState.Running)
                {
                    Console.In.Close();

                    thread.Abort();
                    thread = null;
                    //thread.Join();
                }
            }
        }

        private static void run()
        {
            Boolean b = true;
            Boolean endLine = false;
            StringBuilder sb = new StringBuilder();
            while(b == true)
            {
                if(Console.KeyAvailable)
                {
                    ConsoleKeyInfo r = Console.ReadKey();
                    switch(r.Key)
                    {
                        case ConsoleKey.Enter:
                            endLine = true;
                            break;
                        default:
                            sb.Append(r.KeyChar);
                            break;
                    }
                }
                if(!String.IsNullOrEmpty(sb.ToString()) && endLine)
                {
                    handleInput(new ConsoleMessage(sb.ToString()));
                    sb = new StringBuilder();
                    endLine = false;
                }
                Thread.Sleep(50);
            }
        }
    }
}