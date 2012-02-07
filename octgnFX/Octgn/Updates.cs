using System;
using System.Deployment.Application;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using Octgn.Launcher;
using Octgn.Properties;

namespace Octgn
{
    internal static class Updates
    {
        // StrCmpLogicalW does natural sorting, rather than ASCII.
        // E.g. it produces "page1, page2, page10" rather than "page1, page10, page2"
        // Requires Windows XP
        // See MSDN: http://msdn.microsoft.com/en-us/library/bb759947(VS.85).aspx
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int StrCmpLogicalW(string lpString1, string lpString2);

        public static void UpgradeSettings()
        {
            // This prevents anything from happening when developing locally

            try
            {
                if (!ApplicationDeployment.IsNetworkDeployed) return;
            }
            catch (Exception)
            {
                return;
            }

            if (new Version(Settings.Default.PreviousVersion) == OctgnApp.OctgnVersion) return;

            Settings.Default.Upgrade();
            Settings.Default.Save();

            // Now erase all previous versions data, except the two most recent ones
            string dataFolder = ApplicationDeployment.CurrentDeployment.DataDirectory;
            string[] allVersionsFolder = Directory.GetDirectories(Path.Combine(dataFolder, ".."));
            Array.Sort(allVersionsFolder, StrCmpLogicalW);
            for (int i = 0; i < allVersionsFolder.Length - 2; ++i)
                Directory.Delete(allVersionsFolder[i], true);
        }

        public static void PerformHouskeeping()
        {
            // This prevents anything from happening when developing locally
            //if (!ApplicationDeployment.IsNetworkDeployed) return;

            bool isFirstRun = !Settings.Default.IsUserConfigured;
            var ver = new Version(Settings.Default.PreviousVersion);
            if (ver == OctgnApp.OctgnVersion) return;

            if (!isFirstRun)
            {
                if (ver < new Version(0, 7, 3))
                {
                    // Database is re-created
                    ShutdownMode oldShutdownMode = Application.Current.ShutdownMode;
                    Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                    new UpgradeMessage().ShowDialog();
                    Application.Current.ShutdownMode = oldShutdownMode;
                }

                //Data.GamesRepository.UpgradeFrom(ver);
            }

            Settings.Default.PreviousVersion = OctgnApp.OctgnVersion.ToString();
            Settings.Default.IsUserConfigured = true;
            Settings.Default.Save();
        }
    }
}