using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Octgn.ReleasePusher
{
    public class Pusher
    {
        public static void Main(string[] args)
        {
            var ver = GetVersion();

            if(args.Length == 1 && args[0].ToLowerInvariant() == "createinstaller")
            {
                var templatePath = @"C:\server\OCTGN\installer\InstallTemplate.nsi";
                var templateOutPath = @"C:\server\OCTGN\installer\Install.nsi";
                UpdateStatus("Opening {0} ",templatePath);
                var installTemplate = File.ReadAllText(templatePath);
                installTemplate = installTemplate.Replace("{{version}}", ver.ToString());
                UpdateStatus("Saving {0}",templateOutPath);
                File.WriteAllText(templateOutPath,installTemplate);
                return;
            }

            UpdateStatus("Reading {0}",Settings.Default.OldVersionFile);
            var oldVer = Version.Parse(File.ReadAllText(Settings.Default.OldVersionFile));

            if (ver <= oldVer)
            {
                UpdateStatus("Current version is not greater than the old one. {0} - {1}",ver,oldVer);
                PauseForKey();
                return;
            }

            var bnum = Environment.GetEnvironmentVariable("CCNetNumericLabel");
            UpdateStatus("Newer version detected. {0} - {1}", ver, oldVer);
            var installFilePath = @"c:\server\OCTGN\installer\OCTGN-Setup-" + ver.ToString() + ".exe";
            var updateZipPath = @"C:\server\OCTGNBuilds\" + bnum + @"\update.zip";

            UpdateStatus("Installer Path: {0}", installFilePath);
            UpdateStatus("Update Path   : {0}", updateZipPath);

            var installFile = new FileInfo(installFilePath);
            var updateFile = new FileInfo(updateZipPath);

            var newInstallPath = Path.Combine(Settings.Default.ServerPath, "download", installFile.Name);
            var newUpdatePath = Path.Combine(Settings.Default.ServerPath, "download", updateFile.Name);

            var newInstallPathRelative = Path.Combine("download", installFile.Name);
            var newUpdatePathRelative = Path.Combine("download", updateFile.Name);

            UpdateStatus("New Installer Path: {0}", newInstallPath);
            UpdateStatus("New Update Path   : {0}", newUpdatePath);

            if (File.Exists(newInstallPath))
                File.Delete(newInstallPath);
            File.Copy(installFilePath, newInstallPath);
            if (File.Exists(newUpdatePath))
                File.Delete(newUpdatePath);
            File.Copy(updateZipPath, newUpdatePath);

            CreateUpdateXmlFile(ver, newInstallPathRelative, newUpdatePathRelative);

            PauseForKey();
            UpdateStatus("Done.");

        }
        private static void PauseForKey()
        {
#if(DEBUG)
            Console.ReadKey();
#endif
        }
        private static void CreateUpdateXmlFile(Version ver, string installPathRelative, string updatePathRelative)
        {
            var currentVersionPath = Path.Combine(Settings.Default.ServerPath, "currentversion.txt");
            UpdateStatus("Creating Updater XML File at {0}", currentVersionPath);
            if(File.Exists(currentVersionPath))
                File.Delete(currentVersionPath);
            var template = Settings.Default.CurrentVersionTemplate;
            template = template
                .Replace("{version}", ver.ToString())
                .Replace("{installPath}", installPathRelative)
                .Replace("{updatePath}", updatePathRelative);
            File.WriteAllText(currentVersionPath,template);
            UpdateStatus("Done creating Updater XML File");

        }
        private static Version GetVersion()
        {
            UpdateStatus("Getting Current Version.");
            var vstream = Assembly.GetAssembly(typeof (Program)).GetManifestResourceStream("Octgn.CurrentVersion.txt");
            var versionString = "";
            using(var sr = new StreamReader(vstream))
            {
                versionString = sr.ReadToEnd().Trim();
            }
            return Version.Parse(versionString);
        }
        private static void UpdateStatus(string message, params object[] args)
        {
            Console.WriteLine("[Release Pusher {0} {1}]: {2}",DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(),String.Format(message, args));
        }
    }
}
