using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using Octgn.Data;
using Octgn.DeckBuilder;
using Octgn.Launcher;
using Octgn.Networking;
using Octgn.Play;
using Octgn.Utils;
using Skylabs.Lobby;
using ChatWindow = Octgn.Windows.ChatWindow;
using Client = Octgn.Networking.Client;
using LauncherWindow = Octgn.Windows.LauncherWindow;
using Main = Octgn.Windows.Main;
using RE = System.Text.RegularExpressions;

namespace Octgn
{
    public static class Program
    {
        public static Windows.DWindow DebugWindow;
        public static Main MainWindow;
        public static LauncherWindow LauncherWindow;
        public static DeckBuilderWindow DeckEditor;
        public static PlayWindow PlayWindow;
        public static List<ChatWindow> ChatWindows;

        public static Game Game;
        public static Skylabs.Lobby.Client LobbyClient;
        public static GameSettings GameSettings = new GameSettings();
        public static GamesRepository GamesRepository;
        internal static Client Client;

        internal static bool IsGameRunning;
        internal static readonly string BasePath;
        internal static readonly string GamesPath;

        internal static ulong PrivateKey = ((ulong) Crypto.PositiveRandom()) << 32 | Crypto.PositiveRandom();

#pragma warning disable 67
        internal static event EventHandler<ServerErrorEventArgs> ServerError;
#pragma warning restore 67

        internal static bool IsHost { get; set; }

        internal static Dispatcher Dispatcher;

        internal static readonly TraceSource Trace = new TraceSource("MainTrace", SourceLevels.Information);
        internal static readonly TraceSource DebugTrace = new TraceSource("DebugTrace", SourceLevels.All);
        internal static readonly CacheTraceListener DebugListener = new CacheTraceListener();
        internal static Inline LastChatTrace;

        private static bool _locationUpdating;

        static Program()
        {
            LobbyClient = new Skylabs.Lobby.Client();
            Debug.Listeners.Add(DebugListener);
            DebugTrace.Listeners.Add(DebugListener);
            Trace.Listeners.Add(DebugListener);
            BasePath = Path.GetDirectoryName(typeof (Program).Assembly.Location) + '\\';
            GamesPath = BasePath + @"Games\";
            LauncherWindow = new LauncherWindow();
            Application.Current.MainWindow = LauncherWindow;
            LobbyClient.Chatting.OnCreateRoom += Chatting_OnCreateRoom;
        }

        static void Chatting_OnCreateRoom(object sender, NewChatRoom room)
        {
            if (ChatWindows.All(x => x.Room.RID != room.RID))
            {
                if(MainWindow != null) MainWindow.Dispatcher.Invoke(new Action(() => ChatWindows.Add(new ChatWindow(room))));
                else if(LauncherWindow != null) LauncherWindow.Dispatcher.Invoke(new Action(() => ChatWindows.Add(new ChatWindow(room))));
            }
        }

        public static void StopGame()
        {
            if (Client != null)
            {
                Client.Disconnect();
                Client = null;
            }
            Game.End();
            Game = null;
            Dispatcher = null;
            Database.Close();
            IsGameRunning = false;
        }

        public static void SaveLocation()
        {
            if (_locationUpdating) return;
            if (LauncherWindow == null || !LauncherWindow.IsLoaded) return;
            _locationUpdating = true;
            Prefs.LoginLocation = new Point(LauncherWindow.Left,LauncherWindow.Top);
            _locationUpdating = false;
        }

        public static void Exit()
        {
            Application.Current.MainWindow = null;
            if (LobbyClient != null)
                LobbyClient.Stop();

            SaveLocation();
            try
            {
                if (DebugWindow != null)
                    if (DebugWindow.IsLoaded)
                        DebugWindow.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (Debugger.IsAttached) Debugger.Break();
            }
            if (LauncherWindow != null)
                if (LauncherWindow.IsLoaded)
                    LauncherWindow.Close();
            if (MainWindow != null)
                if (MainWindow.IsLoaded)
                    MainWindow.Close();
            if (PlayWindow != null)
                if (PlayWindow.IsLoaded)
                    PlayWindow.Close();
            try
            {
                foreach (ChatWindow cw in ChatWindows.Where(cw => cw.IsLoaded))
                {
                    cw.CloseChatWindow();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                if (Debugger.IsAttached) Debugger.Break();
            }
            //Apparently this can be null sometimes?
            if(Application.Current != null)
                Application.Current.Shutdown(0);
        }

        internal static void Print(Player player, string text)
        {
            string finalText = text;
            int i = 0;
            var args = new List<object>(2);
            Match match = Regex.Match(text, "{([^}]*)}");
            while (match.Success)
            {
                string token = match.Groups[1].Value;
                finalText = finalText.Replace(match.Groups[0].Value, "{" + i + "}");
                i++;
                object tokenValue = token;
                switch (token)
                {
                    case "me":
                        tokenValue = player;
                        break;
                    default:
                        if (token.StartsWith("#"))
                        {
                            int id;
                            if (!int.TryParse(token.Substring(1), out id)) break;
                            ControllableObject obj = ControllableObject.Find(id);
                            if (obj == null) break;
                            tokenValue = obj;
                            break;
                        }
                        break;
                }
                args.Add(tokenValue);
                match = match.NextMatch();
            }
            args.Add(player);
            Trace.TraceEvent(TraceEventType.Information,
                             EventIds.Event | EventIds.PlayerFlag(player) | EventIds.Explicit, finalText, args.ToArray());
        }

        internal static void TracePlayerEvent(Player player, string message, params object[] args)
        {
            var args1 = new List<object>(args) {player};
            Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player), message,
                             args1.ToArray());
        }

        internal static void TraceWarning(string message)
        {
            Trace.TraceEvent(TraceEventType.Warning, EventIds.NonGame, message);
        }

        internal static void TraceWarning(string message, params object[] args)
        {
            Trace.TraceEvent(TraceEventType.Warning, EventIds.NonGame, message, args);
        }
    }
}