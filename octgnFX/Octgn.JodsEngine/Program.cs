/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Threading;
using Octgn.Data;
using Octgn.DataNew.Entities;
using Octgn.Library;
using Octgn.Networking;
using Octgn.Play;
using Octgn.Utils;
using Player = Octgn.Play.Player;
using System.Reflection;
using System.Windows.Media;
using Microsoft.Win32;
using Octgn.Core.Play;
using Octgn.Windows;
using log4net;
using Octgn.Controls;
using Octgn.Online.Hosting;
using Octgn.Launchers;

namespace Octgn
{
    public static class Program
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static GameEngine GameEngine;

        public static string CurrentOnlineGameName = "";

        public static GameSettings GameSettings { get; } = new GameSettings();
        internal static IClient Client;
        public static event Action OnOptionsChanged;


        internal static bool IsGameRunning;

        internal static bool InPreGame;

#pragma warning disable 67
        internal static event EventHandler<ServerErrorEventArgs> ServerError;
#pragma warning restore 67

        internal static bool IsHost { get; set; }
        internal static GameMode GameMode { get; set; }

        internal static Dispatcher Dispatcher;

        public static GameMessageDispatcher GameMess { get; internal set; }

        public static ILauncher Launcher { get; internal set; }

        public static bool DeveloperMode { get; internal set; }

        /// <summary>
        /// Is properly set at Program.Start()
        /// </summary>
        public static bool IsReleaseTest { get; internal set; }

        public static string SessionKey { get; set; }
        public static string UserId { get; set; }
        public static HostedGame CurrentHostedGame { get; internal set; }
        public static SSLValidationHelper SSLHelper { get; internal set; }

        static Program()
        {
            //Do not put anything here, it'll just lead to pain and confusion
        }

        internal static void FireOptionsChanged()
        {
            if (OnOptionsChanged != null)
                OnOptionsChanged.Invoke();
        }

        public static void StopGame()
        {
            //X.Instance.Try(ChatLog.ClearEvents);
            Program.GameMess?.Clear();
			X.Instance.Try(()=>Program.Client?.Rpc?.Leave(Player.LocalPlayer));
            if (Client != null)
            {
                Client.Shutdown();
                Client = null;
            }
            if (GameEngine != null)
                GameEngine.End();
            GameEngine = null;
            Dispatcher = null;
            IsGameRunning = false;
        }

        public static void Exit()
        {
            try { SSLHelper?.Dispose(); }
            catch (Exception e) {
                Log.Error( "SSLHelper Dispose Exception", e );
            };
            Sounds.Close();
            try
            {
                Program.Client?.Rpc?.Leave(Player.LocalPlayer);
            }
            catch (Exception e)
            {
                Log.Error( "Exit() Player leave exception", e );
            }
            LogManager.Shutdown();
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                WindowManager.Shutdown();

                //Apparently this can be null sometimes?
                if (Application.Current != null)
                    Application.Current.Shutdown(0);
            }));

        }

        internal static void Print(Player player, string text, string color = null)
        {
            var p = Parse(player, text);
            if (color == null)
            {
                GameMess.Notify(p.Item1, p.Item2);
            }
            else
            {
                Color? c = null;
                if (String.IsNullOrWhiteSpace(color))
                {
                    c = Colors.Black;
                }
                if (c == null)
                {
                    try
                    {
                        if (color.StartsWith("#") == false)
                        {
                            color = color.Insert(0, "#");
                        }
                        if (color.Length == 7)
                        {
                            color = color.Insert(1, "F");
                            color = color.Insert(1, "F");
                        }
                        c = (Color)ColorConverter.ConvertFromString(color);
                    }
                    catch
                    {
                        c = Colors.Black;
                    }
                }
                GameMess.NotifyBar(c.Value, p.Item1, p.Item2);
            }
        }

        internal static Tuple<string, object[]> Parse(Player player, string text)
        {
            string finalText = text;
            int i = 0;
            var args = new List<object>(2);
            Match match = Regex.Match(text, "{([^}]*)}");
            while (match.Success)
            {
                string token = match.Groups[1].Value;
                finalText = finalText.Replace(match.Groups[0].Value, "##$$%%^^LEFTBRACKET^^%%$$##" + i + "##$$%%^^RIGHTBRACKET^^%%$$##");
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
            finalText = finalText.Replace("{", "").Replace("}", "");
            finalText = finalText.Replace("##$$%%^^LEFTBRACKET^^%%$$##", "{").Replace(
                "##$$%%^^RIGHTBRACKET^^%%$$##", "}");
            return new Tuple<string, object[]>(finalText, args.ToArray());
        }

        public static void LaunchUrl(string url)
        {
            if (url == null) return;
            if (GetDefaultBrowserPath() == null)
            {
                Dispatcher d = Dispatcher;
                if (d == null) d = Application.Current.Dispatcher;
                if (d == null) d = System.Windows.Threading.Dispatcher.CurrentDispatcher;
                if (d == null && Application.Current != null && Application.Current.MainWindow != null) d = Application.Current.MainWindow.Dispatcher;
                if (d == null) return;
                d.Invoke(new Action(() => new BrowserWindow(url).Show()));
                return;
            }
            Process.Start(url);
        }

        public static void LaunchApplication(string path, params string[] args)
        {
            var psi = new ProcessStartInfo(path, String.Join(" ", args));
            try
            {
                psi.UseShellExecute = true;
                Process.Start(psi);
            }
            catch (Win32Exception e)
            {
                if (e.NativeErrorCode != 1223)
                    Log.Warn("LaunchApplication Error " + path + " " + psi.Arguments, e);
            }
            catch (Exception e)
            {
                Log.Warn("LaunchApplication Error " + path + " " + psi.Arguments, e);
            }

        }

        public static string GetDefaultBrowserPath()
        {
            string defaultBrowserPath = null;
            try
            {
                RegistryKey regkey;

                // Check if we are on Vista or Higher
                OperatingSystem OS = Environment.OSVersion;
                if ((OS.Platform == PlatformID.Win32NT) && (OS.Version.Major >= 6))
                {
                    regkey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\shell\\Associations\\UrlAssociations\\http\\UserChoice", false);
                    if (regkey != null)
                    {
                        defaultBrowserPath = regkey.GetValue("Progid").ToString();
                    }
                    else
                    {
                        regkey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Classes\\IE.HTTP\\shell\\open\\command", false);
                        defaultBrowserPath = regkey.GetValue("").ToString();
                    }
                }
                else
                {
                    regkey = Registry.ClassesRoot.OpenSubKey("http\\shell\\open\\command", false);
                    defaultBrowserPath = regkey.GetValue("").ToString();
                }



            }
            catch (Exception e)
            {
                Log.Error(e);
            }
            return defaultBrowserPath;
        }

        public static void DoCrazyException(Exception e, string action)
        {
            var res = TopMostMessageBox.Show(action + Environment.NewLine + Environment.NewLine + "Are you going to be ok?", "Oh No!",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
        }
    }
}
