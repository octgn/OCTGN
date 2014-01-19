namespace Octgn.Online.GameService
{
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    using log4net;

    using Microsoft.Win32;

    using Octgn.Library;

    public class InstanceHandler
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
        #region Singleton

        internal static InstanceHandler SingletonContext { get; set; }

        private static readonly object InstanceHandlerSingletonLocker = new object();

        public static InstanceHandler Instance
        {
            get
            {
                if (SingletonContext == null)
                {
                    lock (InstanceHandlerSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new InstanceHandler();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        private static string KeyName
        {
            get
            {
                if (AppConfig.Instance.TestMode)
                {
                    return "GameService-Test";
                }
                return "GameService";
            }
        }

        private static RegistryKey GetKey()
        {
            var key = Registry.CurrentUser.OpenSubKey("Software", true);
            if (key == null)
            {
                key = Registry.CurrentUser.CreateSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree);
            }
            var key2 = key.OpenSubKey("OCTGN", true);
            if (key2 == null)
            {
                key2 = key.CreateSubKey("OCTGN", RegistryKeyPermissionCheck.ReadWriteSubTree);
            }

            var key3 = key2.OpenSubKey(KeyName, true);
            if (key3 == null)
            {
                key3 = key2.CreateSubKey(KeyName, RegistryKeyPermissionCheck.ReadWriteSubTree);
            }
            key.Dispose();
            key2.Dispose();
            return key3;
        }

        public string Path
        {
            get
            {
                using (var root = GetKey())
                {
                    var ret = (string)root.GetValue("Path", null);
                    return ret;
                }
            }
            set
            {
                using (var root = GetKey())
                {
                    root.SetValue("Path", value, RegistryValueKind.String);
                }
            }
        }

        public int ProcessId
        {
            get
            {
                using (var root = GetKey())
                {
                    var ret = (int)root.GetValue("ProcessId", -1);
                    return ret;
                }
            }
            set
            {
                using (var root = GetKey())
                {
                    root.SetValue("ProcessId", value, RegistryValueKind.DWord);
                }
            }
        }

        public bool KillMe
        {
            get
            {
                using (var root = GetKey())
                {
                    var ret = (int)root.GetValue("KillMe", 0) > 0;
                    return ret;
                }
            }
            set
            {
                using (var root = GetKey())
                {
                    root.SetValue("KillMe", value == false ? 0 : 1, RegistryValueKind.DWord);
                }
            }
        }

        public bool OtherExists()
        {
            if (Path == null) return false;
            var proc = Process.GetProcesses().FirstOrDefault(x => x.Id == this.ProcessId);
            if (proc == null) return false;

            return true;
        }

        public void KillOther()
        {
            KillMe = true;
            Log.Info("KillOther");
            while (OtherExists())
            {
                Thread.Sleep(1000);
            }
            Log.Info("KillOther finished.");
        }

        public void SetupValues()
        {
            KillMe = false;
            ProcessId = Process.GetCurrentProcess().Id;
            Path = Directory.GetCurrentDirectory();
        }
    }
}