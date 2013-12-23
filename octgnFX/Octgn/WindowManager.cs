namespace Octgn
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    using Octgn.DeckBuilder;
    using Octgn.Play;
    using Octgn.Windows;

    public static class WindowManager
    {
        public static DWindow DebugWindow { get; set; }
        public static Main Main { get; set; }
        public static DeckBuilderWindow DeckEditor { get; set; }
        public static PlayWindow PlayWindow { get; set; }
        public static PreGameLobbyWindow PreGameLobbyWindow { get; set; }
        public static ConcurrentBag<ChatWindow> ChatWindows { get; internal set; }
        public static GrowlNotifications GrowlWindow { get; set; }

        static WindowManager()
        {
            ChatWindows = new ConcurrentBag<ChatWindow>();
            GrowlWindow = new GrowlNotifications();
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


        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
    }
}