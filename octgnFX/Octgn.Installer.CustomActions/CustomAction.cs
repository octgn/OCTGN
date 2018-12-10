using System;
using System.Linq;
using System.IO;
using Microsoft.Deployment.WindowsInstaller;
using Octgn.Installer.Shared;
using System.Windows.Forms;

namespace Octgn.Installer.CustomActions
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult CopyOldDataDirectory(Session session) {
            session.Log("Begin CopyOldDataDirectory");

            DirectoryInfo fromDirectory = null;
            string toDirectory = null;

            try {
                toDirectory = session.CustomActionData["DATADIRECTORY"];

                session.Log("DATADIRECTORY=" + toDirectory);

                var newFolder = new DirectoryInfo(toDirectory);

                var oldOctgn = InstalledOctgn.Get();

                fromDirectory = oldOctgn.DataDirectory ?? new DirectoryInfo(Paths.Get.DefaultDataDirectory);

                if (fromDirectory.Exists) {
                    session.Log("Old OCTGN DataDirectory exists.");

                    var usingDifferentDirectory = !string.Equals(newFolder.FullName.Trim('\\'), fromDirectory.FullName.Trim('\\'), StringComparison.CurrentCultureIgnoreCase);

                    if (usingDifferentDirectory) {
                        session.Log($"Using a different Data Directory: {newFolder} != {fromDirectory.FullName}");

                        StatusMessage(session, "Moving OCTGN DataDirectory");

                        session.Log($"Copying {fromDirectory.FullName} to {newFolder.FullName}");

                        var totalFileCount = fromDirectory.EnumerateFiles("*.*", SearchOption.AllDirectories).Count();

                        ResetProgressBar(session, totalFileCount + 1);

                        CopyDirectory(session, fromDirectory, newFolder);

                        session.Log($"Deleting {fromDirectory.FullName}");

                        fromDirectory.Delete(true);

                        IncrementProgressBar(session, 1);
                    }
                }

                return ActionResult.Success;
            } catch (Exception ex) {
                session.Log(ex.ToString());

                var record = new Record();
                record.FormatString = $"Error copying old DataDirectory\r\nFrom: {fromDirectory.FullName}\r\nTo: {toDirectory}\r\n{ex}";

                session.Message(InstallMessage.Error | (InstallMessage)(MessageBoxIcon.Error) | (InstallMessage)MessageBoxButtons.OK, record);

                return ActionResult.Failure;
            }
        }

        [CustomAction]
        public static ActionResult CopyOldOctgnFiles(Session session) {
            session.Log("Begin CopyOldOctgnFiles");

            var fromDirectory = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OCTGN"));
            string toDirectory = null;

            try {
                toDirectory = session.CustomActionData["DATADIRECTORY"];

                session.Log("DATADIRECTORY=" + toDirectory);

                var newFolder = new DirectoryInfo(toDirectory);

                if (fromDirectory.Exists) {
                    session.Log("Old OCTGN DataDirectory exists.");

                    // Only if the new data directory is not the old my documents one
                    var usingDifferentDirectory = !string.Equals(newFolder.FullName.Trim('\\'), fromDirectory.FullName.Trim('\\'), StringComparison.CurrentCultureIgnoreCase);

                    if (usingDifferentDirectory) {
                        session.Log($"Using a different Data Directory: {newFolder} != {fromDirectory.FullName}");

                        StatusMessage(session, "Moving old OCTGN Files");

                        ResetProgressBar(session, 1);

                        session.Log($"Moving old OCTGN files {fromDirectory.FullName} to {newFolder.FullName}");

                        var totalFileCount = fromDirectory.EnumerateFiles("*.*", SearchOption.AllDirectories).Count();

                        ResetProgressBar(session, totalFileCount + 1);

                        CopyDirectory(session, fromDirectory, newFolder);

                        session.Log($"Deleting {fromDirectory.FullName}");

                        fromDirectory.Delete(true);

                        IncrementProgressBar(session, 1);
                    }
                }

                return ActionResult.Success;
            } catch (Exception ex) {
                session.Log(ex.ToString());

                var record = new Record();
                record.FormatString = $"Error deleting old DataDirectory: {fromDirectory.FullName}\r\n{ex}";

                session.Message(InstallMessage.Error | (InstallMessage)(MessageBoxIcon.Error) | (InstallMessage)MessageBoxButtons.OK, record);

                return ActionResult.Failure;
            }
        }

        private static void CopyDirectory(Session session, DirectoryInfo directory, DirectoryInfo destinationDirectory) {
            if (!destinationDirectory.Exists) {
                destinationDirectory.Create();
            }

            foreach(var file in directory.EnumerateFiles()) {
                var copyTo = Path.Combine(destinationDirectory.FullName, file.Name);

                session.Log($"Copying {file.FullName} to {copyTo}");

                file.CopyTo(copyTo, true);

                session.Log($"Copied {file.FullName} to {copyTo}");

                IncrementProgressBar(session, 1);
            }

            foreach(var subdirectory in directory.EnumerateDirectories()) {
                var copyTo = destinationDirectory.CreateSubdirectory(subdirectory.Name);

                CopyDirectory(session, subdirectory, copyTo);
            }
        }

        internal static void StatusMessage(Session session, string status) {
            var record = new Record(1);

            record[1] = status;

            session.Message(InstallMessage.ActionData, record);
        }


        public static MessageResult ResetProgressBar(Session session, int totalStatements) {
            var record = new Record(3);
            record[1] = 0; // "Reset" message 
            record[2] = totalStatements;  // total ticks 
            record[3] = 0; // forward motion 
            return session.Message(InstallMessage.Progress, record);
        }

        public static MessageResult IncrementProgressBar(Session session, int progressPercentage) {
            var record = new Record(3);
            record[1] = 2; // "ProgressReport" message 
            record[2] = progressPercentage; // ticks to increment 
            record[3] = 0; // ignore 
            return session.Message(InstallMessage.Progress, record);
        }
    }
}
