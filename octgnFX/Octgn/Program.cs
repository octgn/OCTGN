using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Octgn.Play;
using Skylabs.Lobby;
using RE = System.Text.RegularExpressions;

namespace Octgn
{
    public static class Program
    {
        public static DWindow DebugWindow;
        public static Game Game;
        public static LobbyClient lobbyClient;
        public static Skylabs.LobbyServer.Server lobbyServer;
        public static GameSettings GameSettings = new GameSettings();
        public static Octgn.Launcher.Main ClientWindow;
        public static Launcher.LauncherWindow LauncherWindow;
        public static DeckBuilder.DeckBuilderWindow DeckEditor;
        internal static bool IsGameRunning = false;
        // TODO: Refactoring > those paths belong to the Octgn.Data or somewhere else
        internal readonly static string BasePath;
        internal readonly static string GamesPath;
        public static Data.GamesRepository GamesRepository;

        internal static ulong PrivateKey = ((ulong)Crypto.PositiveRandom()) << 32 | Crypto.PositiveRandom();

        internal static Server.Server Server;
        internal static Networking.Client Client;
        internal static event EventHandler<ServerErrorEventArgs> ServerError;

        internal static bool IsHost { get { return Server != null; } }

        internal static Dispatcher Dispatcher;

        internal readonly static TraceSource Trace = new TraceSource("MainTrace", SourceLevels.Information);
        internal readonly static TraceSource DebugTrace = new TraceSource("DebugTrace", SourceLevels.All);
        internal readonly static CacheTraceListener DebugListener = new CacheTraceListener();
        internal static System.Windows.Documents.Inline LastChatTrace;

#if(DEBUG)
        public static DEBUGLobbySettings LobbySettings = DEBUGLobbySettings.Default;
#else
        public static lobbysettings LobbySettings = lobbysettings.Default;
#endif

        static Program()
        {
            DebugTrace.Listeners.Add(DebugListener);
            BasePath = Path.GetDirectoryName(typeof(Program).Assembly.Location) + '\\';
            GamesPath = BasePath + @"Games\";
#if(DEBUG)
            lobbyServer = new Skylabs.LobbyServer.Server(System.Net.IPAddress.Any, LobbySettings.ServerPort);
            lobbyServer.Start();
#endif
            Exception e = new Exception();
            string s = e.Message.Substring(0);
            Program.LauncherWindow = new Launcher.LauncherWindow();
            Program.LauncherWindow.Show();
        }

        public static void StopGame()
        {
            Client.Disconnect(); Client = null;
            if(Server != null)
            { Server.Stop(); Server = null; }
            Game.End(); Game = null;
            Dispatcher = null;
            Database.Close();
            IsGameRunning = false;
        }

        public static void Exit()
        {
            try
            {
                DebugWindow.Close();
            }
            catch(Exception e)
            {
            }
            if(LauncherWindow != null)
                if(LauncherWindow.IsLoaded)
                    LauncherWindow.Close();
            if(ClientWindow != null)
                if(ClientWindow.IsLoaded)
                    ClientWindow.Close();
            if(lobbyServer != null)
                lobbyServer.Stop();
        }

        internal static void OnServerError(string serverMessage)
        {
            var args = new ServerErrorEventArgs() { Message = serverMessage };
            if(ServerError != null)
                ServerError(null, args);
            if(args.Handled) return;

            MessageBox.Show(Application.Current.MainWindow,
                "The server has returned an error:\n" + serverMessage,
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // TODO: Does this belong here?
        internal static void Print(Player player, string text)
        {
            string finalText = text;
            int i = 0;
            List<object> args = new List<object>(2);
            RE.Match match = RE.Regex.Match(text, "{([^}]*)}");
            while(match.Success)
            {
                string token = match.Groups[1].Value;
                finalText = finalText.Replace(match.Groups[0].Value, "{" + i + "}");
                i++;
                object tokenValue = token;
                switch(token)
                {
                    case "me": tokenValue = player; break;
                    default:
                        if(token.StartsWith("#"))
                        {
                            int id;
                            if(!int.TryParse(token.Substring(1), out id)) break;
                            var obj = ControllableObject.Find(id);
                            if(obj == null) break;
                            tokenValue = obj;
                            break;
                        }
                        break;
                }
                args.Add(tokenValue);
                match = match.NextMatch();
            }
            args.Add(player);
            Program.Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player) | EventIds.Explicit, finalText, args.ToArray());
        }

        // TODO: Refactoring: those helper methods belong somewhere else (near the tracing classes)
        internal static void TracePlayerEvent(Player player, string message, params object[] args)
        {
            List<object> args1 = new List<object>(args);
            args1.Add(player);
            Trace.TraceEvent(TraceEventType.Information, EventIds.Event | EventIds.PlayerFlag(player), message, args1.ToArray());
        }

        internal static void TraceWarning(string message)
        {
            Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.NonGame, message);
        }

        internal static void TraceWarning(string message, params object[] args)
        {
            Program.Trace.TraceEvent(TraceEventType.Warning, EventIds.NonGame, message, args);
        }
    }
}