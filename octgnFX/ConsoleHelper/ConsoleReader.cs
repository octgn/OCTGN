using System;
using System.Text;
using System.Threading;

namespace Skylabs.ConsoleHelper
{
    public class ConsoleReader
    {
        #region Delegates

        public delegate void ConsoleInputDelegate(ConsoleMessage input);

        #endregion

        //public static ConsoleColor InputColor { get { return _InputColor; } set { _InputColor = value; } }

        //private static ConsoleColor _InputColor = ConsoleColor.Gray;

        private static Thread _thread;

        public static ThreadState ThreadState
        {
            get { return _thread.ThreadState; }
        }

        public static event ConsoleInputDelegate EConsoleInput;

        public static void Start()
        {
            _thread = new Thread(Run) {Name = "ConsoleHandThread"};
            _thread.Start();
        }

        private static void HandleInput(ConsoleMessage cm)
        {
            if (EConsoleInput == null) return;
            if (EConsoleInput.GetInvocationList().Length > 0)
                EConsoleInput.Invoke(cm);
        }

        public static void Stop()
        {
            if (_thread == null) return;
            if (_thread.ThreadState != ThreadState.Running) return;
            Console.In.Close();

            _thread.Abort();
            _thread = null;
            //thread.Join();
        }

        private static void Run()
        {
            var endLine = false;
            var sb = new StringBuilder();
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var r = Console.ReadKey();
                    switch (r.Key)
                    {
                        case ConsoleKey.Enter:
                            endLine = true;
                            break;
                        default:
                            sb.Append(r.KeyChar);
                            break;
                    }
                }
                if (!String.IsNullOrEmpty(sb.ToString()) && endLine)
                {
                    HandleInput(new ConsoleMessage(sb.ToString()));
                    sb = new StringBuilder();
                    endLine = false;
                }
                Thread.Sleep(50);
            }
        }
    }
}