namespace Octgn.PlayTable
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Threading;

    using Microsoft.Win32;

    using Octgn.Core;
    using Octgn.Core.Play;
    using Octgn.Networking;
    using Octgn.Play;
    using Octgn.Windows;

    using log4net;

    public static class Program
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static Client Client
        {
            get
            {
                return K.C.Get<Client>();
            }
        }

        public static GameplayTrace Trace
        {
            get
            {
                return K.C.Get<GameplayTrace>();
            }
        }

        public static IGameEngine GameEngine
        {
            get
            {
                return K.C.Get<IGameEngine>();
            }
        }

        public static IObjectCreator Creator
        {
            get
            {
                return K.C.Get<IObjectCreator>();
            }
        }

        static Program()
        {
            MakeBindings();
        }

        private static void MakeBindings()
        {
            K.C.Bind<GameplayTrace>().ToSelf().InSingletonScope();
            K.C.Bind<Dispatcher>().ToMethod(x => Application.Current.Dispatcher);
            K.C.Bind<Client>().ToSelf().InSingletonScope();
            K.C.Bind<IGameEngine>().To<GameEngine>().InSingletonScope();
            K.C.Bind<GameStateMachine>().ToSelf().InSingletonScope();
            K.C.Bind<IObjectCreator>().To<ObjectCreator>().InSingletonScope();
        }

        public static void LaunchUrl(string url)
        {
            if (GetDefaultBrowserPath() == null)
            {
                //TODO Launch in custom browser window
                Application
                    .Current
                    .Dispatcher
                    .Invoke(new Action(() => new BrowserWindow(url).Show()));
                return;
            }
            Process.Start(url);

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
    }

}