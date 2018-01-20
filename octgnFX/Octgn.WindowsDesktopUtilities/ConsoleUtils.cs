using Octgn.Communication;
using System;
using System.Runtime.InteropServices;

namespace Octgn.WindowsDesktopUtilities
{
    public static class ConsoleUtils
    {
        private static ILogger Log = LoggerFactory.Create(nameof(ConsoleUtils));

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        public static bool IsConsoleWindowVisible => GetConsoleWindow() != IntPtr.Zero;

        /// <summary>
        /// Try and wait for a key from the console. If the application is in a state where it
        /// can't receive input from the console, this returns false and doesn't wait.
        /// </summary>
        /// <returns>True if it waited for a key, otherwise false</returns>
        public static bool TryWaitForKey() {
            if (!IsConsoleWindowVisible) return false;
            Log.Info($"{nameof(TryWaitForKey)}: There's a console window, waiting for a key stroke to exit.");
            Console.WriteLine("Waiting for key...");
            Console.ReadKey();
            return true;
        }
    }
}