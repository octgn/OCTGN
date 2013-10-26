using System;
using System.IO;
using System.Reflection;
using log4net;
using Microsoft.Win32;
using Octgn.Library;
using Octgn.Library.Exceptions;

namespace Octgn.Core.Util
{
    public class SetupWindows
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        #region Singleton

        internal static SetupWindows SingletonContext { get; set; }

        private static readonly object SetupWindowsSingletonLocker = new object();

        public static SetupWindows Instance
        {
            get
            {
                if (SingletonContext == null)
                {
                    lock (SetupWindowsSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new SetupWindows();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        public void RegisterCustomProtocol(Assembly octgnAssembly)
        {
            try
            {
                var rootKey = Registry.CurrentUser. OpenSubKey("Software",true).OpenSubKey("Classes",true);
                var key = rootKey.OpenSubKey("octgn",true);
                //if (key == null)
                //{
                    var octgnKey = rootKey.CreateSubKey("octgn");
                    octgnKey.SetValue(string.Empty, string.Empty);
                    octgnKey.SetValue("URL Protocol", string.Empty);

                    key = octgnKey.CreateSubKey(@"shell\open\command");
                    //var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OCTGN","OCTGN", "OCTGN.exe");
                    var path = octgnAssembly.Location;
                    key.SetValue(string.Empty, path + " " + "\"%1\"");

                    key = octgnKey.CreateSubKey(@"shell\open\ddeexec");
                    key.SetValue(string.Empty, string.Empty);
                //}

            }
            catch (System.Exception e)
            {
                Log.Warn("RegisterCustomProtocol",e);
            }
        }

        public void RegisterDeckExtension(Assembly octgnAssembly)
        {
            RegisterGlobalOctgnDeckHandeler(octgnAssembly);
            try
            {
                var rootKey = Registry.CurrentUser.OpenSubKey("Software", true).OpenSubKey("Classes", true);
                var root = rootKey.OpenSubKey(".o8d", true);
                //if (key == null)
                //{
                root = rootKey.CreateSubKey(".o8d");
                root.SetValue(string.Empty, "OctgnDeckHandler");
                //root.SetValue(string.Empty, "octgn.deck");
                //var key = root.CreateSubKey("DefaultIcon");

                //var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OCTGN", "OCTGN", "Resources","FileIcons","Deck.ico");

                //key.SetValue(string.Empty,path);

                ////\OCTGN\OCTGN\Resources\FileIcons

                //key = root.CreateSubKey(@"shell\open\command");
                ////var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OCTGN",
                ////    "OCTGN", "OCTGN.exe");
                //key.SetValue(string.Empty, octgnAssembly.Location + " -d " + "\"%1\"");
                //}
            }
            catch (Exception e)
            {
                Log.Warn("RegisterDeckExtension", e);
            }
        }

        public void RegisterGlobalOctgnDeckHandeler(Assembly octgnAssembly)
        {
            try
            {
                var rootKey = Registry.CurrentUser.OpenSubKey("Software", true).OpenSubKey("Classes", true);
                var root = rootKey.OpenSubKey("OctgnDeckHandler", true);
                //if (key == null)
                //{
                root = rootKey.CreateSubKey("OctgnDeckHandler");
                root.SetValue(string.Empty, "octgn.deck");
                var key = root.CreateSubKey("DefaultIcon");

                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OCTGN", "OCTGN", "Resources", "FileIcons", "Deck.ico");

                key.SetValue(string.Empty, path);

                //\OCTGN\OCTGN\Resources\FileIcons

                key = root.CreateSubKey(@"shell\open\command");
                //var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OCTGN",
                //    "OCTGN", "OCTGN.exe");
                key.SetValue(string.Empty, octgnAssembly.Location + " -d " + "\"%1\"");
            }
            catch (Exception e)
            {
                Log.Warn("RegisterDeckExtension", e);
            }
        }
    }
}