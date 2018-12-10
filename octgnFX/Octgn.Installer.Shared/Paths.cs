using System;
using System.IO;

namespace Octgn.Installer.Shared
{
    public class Paths
    {
        public static Paths Get {
            get {
                if (_cachedPaths == null) {
                    var paths = new Paths();
                    paths.DefaultInstallPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "OCTGN");
                    paths.DefaultDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OCTGN");

                    _cachedPaths = paths;
                }

                return _cachedPaths;
            }
        }

        private static Paths _cachedPaths;

        public string DefaultInstallPath { get; set; }

        public string DefaultDataDirectory { get; set; }
    }
}
