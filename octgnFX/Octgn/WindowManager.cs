using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using log4net;

namespace Octgn
{
    using System;
    using System.Collections.Concurrent;
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
        //public static PreGameLobbyWindow PreGameLobbyWindow { get; set; }
        public static ConcurrentBag<ChatWindow> ChatWindows { get; internal set; }
        public static GrowlNotifications GrowlWindow { get; set; }
        public static UiLagWindow UiLagWindow { get; set; }

        private static Thread _uiLagWindowThread;
        private static Dispatcher _uiLagWindowDispatcher;

        static WindowManager()
        {
            ChatWindows = new ConcurrentBag<ChatWindow>();
            GrowlWindow = new GrowlNotifications();
            _uiLagWindowThread = new Thread(() =>
            {
                UiLagWindow = new UiLagWindow();
                _uiLagWindowDispatcher = Dispatcher.CurrentDispatcher;
                System.Windows.Threading.Dispatcher.Run();
            });

            _uiLagWindowThread.Name = "Lag Window UI Thread";
            _uiLagWindowThread.SetApartmentState(ApartmentState.STA);
            _uiLagWindowThread.Start();

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
                if (UiLagWindow.IsLoaded)
                {
                    UiLagWindow.Close();
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            } 
            UiLagWindow = null;
            _uiLagWindowDispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
            _uiLagWindowThread.Join();

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
            try
            {
                foreach (var w in WindowManager.ChatWindows.ToArray())
                {
                    try
                    {
                        if (w.IsLoaded) w.CloseDown();
                        w.Dispose();
                    }
                    catch (Exception e)
                    {
                        Log.Warn("Close chat window error", e);
                    }
                }
                WindowManager.ChatWindows = new ConcurrentBag<ChatWindow>();
            }
            catch (Exception e)
            {
                Log.Warn("Close chat window enumerate error", e);
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