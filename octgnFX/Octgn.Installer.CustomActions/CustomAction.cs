using System;
using System.IO;
using Microsoft.Deployment.WindowsInstaller;
using Octgn.Installer.Shared;

namespace Octgn.Installer.CustomActions
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult CopyOldDataDirectory(Session session) {
            session.Log("Begin CopyOldDataDirectory");

            var oldOctgn = InstalledOctgn.Get();

            var oldFolder = oldOctgn.DataDirectory;

            var newFolder = new DirectoryInfo(session["DATADIRECTORY"]);

            CopyDirectory(session, oldFolder, newFolder);

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult DeleteOldDataDirectory(Session session) {
            session.Log("Begin DeleteOldDataDirectory");

            var oldOctgn = InstalledOctgn.Get();

            var oldFolder = oldOctgn.DataDirectory;

            session.Log($"Deleting {oldFolder.FullName}");

            oldFolder.Delete(true); ;

            return ActionResult.Success;
        }

        private static void CopyDirectory(Session session, DirectoryInfo directory, DirectoryInfo destinationDirectory) {
            if (!destinationDirectory.Exists) {
                destinationDirectory.Create();
            }

            foreach(var file in directory.EnumerateFiles()) {
                var copyTo = Path.Combine(destinationDirectory.FullName, file.Name);
                file.CopyTo(copyTo, true);
            }

            foreach(var subdirectory in directory.EnumerateDirectories()) {
                var copyTo = destinationDirectory.CreateSubdirectory(subdirectory.Name);
                CopyDirectory(subdirectory, copyTo);
            }
        }
    }
}
