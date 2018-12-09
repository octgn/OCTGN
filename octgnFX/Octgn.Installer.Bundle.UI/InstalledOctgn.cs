using Microsoft.Win32;
using System;
using System.IO;

namespace Octgn.Installer.Bundle.UI
{
    public class InstalledOctgn {
        public static InstalledOctgn Get() {
            var installedOctgn = new InstalledOctgn();

            var oldPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            oldPath = Path.Combine(oldPath, "Octgn", "OCTGN");

            installedOctgn.InstalledDirectory = new DirectoryInfo(oldPath);

            //TODO: This should be able to check the registry or something, the previous installer should have left some artifact we can use. This may be unneccisary though.
            if (Directory.Exists(oldPath)) {
                installedOctgn.IsIncompatible = true;
                installedOctgn.IsInstalled = true;
            } else {
                installedOctgn.IsIncompatible = false;

                // Get install path from registry
                using (var subKey = Registry.CurrentUser.OpenSubKey(@"Software\OCTGN")) {
                    if (subKey != null) {
                        var dataDirectoryString = (string)subKey.GetValue(@"DataDirectory");

                        if (!string.IsNullOrWhiteSpace(dataDirectoryString)) {
                            var dataDirectory = new DirectoryInfo(dataDirectoryString);

                            installedOctgn.InstalledDirectory = dataDirectory;
                            installedOctgn.IsInstalled = true;
                        }
                    }
                }
            }

            return installedOctgn;
        }

        public bool IsInstalled { get; set; }

        public bool IsIncompatible { get; set; }

        public DirectoryInfo InstalledDirectory { get; set; }
    }
}