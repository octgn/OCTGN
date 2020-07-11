using System.Reflection;
using System.Windows;
using log4net;

namespace Octgn
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    using Octgn.DeckBuilder;
    using Octgn.Play;
    using Octgn.Windows;

    public static class WindowManager
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static DWindow DebugWindow { get; set; }
        public static Main Main { get; set; }
        public static DeckBuilderWindow DeckEditor { get; set; }
        public static PlayWindow PlayWindow { get; set; }

        static WindowManager()
        {
        }

        public static bool ApplicationIsActivated()
        {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero)
            {
                return false;       // No window is currently activated
            }

            var procId = Process.GetCurrentProcess().Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }

        /// <summary>
        /// Must be ran on the UI thread
        /// </summary>
        public static void Shutdown()
        {
            Application.Current.MainWindow = null;

            try
            {
                if (WindowManager.DebugWindow != null)
                    if (WindowManager.DebugWindow.IsLoaded)
                        WindowManager.DebugWindow.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (Debugger.IsAttached) Debugger.Break();
            }
            if (WindowManager.PlayWindow != null)
                if (WindowManager.PlayWindow.IsLoaded)
                    WindowManager.PlayWindow.Close();
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
    }
}