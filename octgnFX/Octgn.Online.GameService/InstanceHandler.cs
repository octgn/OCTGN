/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using log4net;
using Microsoft.Win32;
using System;

namespace Octgn.Online.GameService
{
    public class InstanceHandler : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Singleton

        internal static InstanceHandler SingletonContext { get; set; }

        private static readonly object InstanceHandlerSingletonLocker = new object();

        public static InstanceHandler Instance {
            get {
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

        private static string KeyName {
            get {
                if (AppConfig.Instance.TestMode)
                {
                    return "GameService-Test";
                }
                return "GameService";
            }
        }

        public static readonly RegistryKey Root = GetKey();

        private static RegistryKey GetKey()
        {
            RegistryKey key = null;
            RegistryKey key2 = null;
            try
            {
                using (key = Registry.CurrentUser.OpenSubKey("Software", true)
                    ?? Registry.CurrentUser.CreateSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    using (key2 = key.OpenSubKey("OCTGN", true)
                        ?? key.CreateSubKey("OCTGN", RegistryKeyPermissionCheck.ReadWriteSubTree))
                    {

                        var key3 = key2.OpenSubKey(KeyName, true)
                            ?? key2.CreateSubKey(KeyName, RegistryKeyPermissionCheck.ReadWriteSubTree);

                        return key3;
                    }
                }
            }
            catch
            {
                key?.Dispose();
                key2?.Dispose();
                throw;
            }
        }

        public string Path {
            get {
                var ret = (string)Root.GetValue(nameof(Path), null);
                return ret;
            }
            set {
                Root.SetValue(nameof(Path), value, RegistryValueKind.String);
            }
        }

        public int ProcessId {
            get {
                var ret = (int)Root.GetValue(nameof(ProcessId), -1);
                return ret;
            }
            set {
                Root.SetValue(nameof(ProcessId), value, RegistryValueKind.DWord);
            }
        }

        public bool KillMe {
            get {
                var ret = (int)Root.GetValue(nameof(KillMe), 0) > 0;
                return ret;
            }
            set {
                Root.SetValue(nameof(KillMe), value == false ? 0 : 1, RegistryValueKind.DWord);
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
            Log.Info(nameof(KillOther));
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

        public void Dispose()
        {
            Root?.Dispose();
        }
    }
}