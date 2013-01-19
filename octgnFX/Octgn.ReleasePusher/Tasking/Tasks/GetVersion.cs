namespace Octgn.ReleasePusher.Tasking.Tasks
{
    using System;

    public class GetVersion:ITask
    {
        public void Run(object sender, ITaskContext context)
        {
            context.Log.Info("Reading Data Settings");
            var rootPath = context.Data["WorkingDirectory"] as string;
            var currentVersionFileRelativePath = context.Data["CurrentVersionFileRelativePath"] as string;

            var fullPath = context.FileSystem.Path.Combine(rootPath, currentVersionFileRelativePath);

            context.Log.InfoFormat("Reading {0}",fullPath);
            var versionText = context.FileSystem.File.ReadAllText(fullPath);

            context.Log.InfoFormat("Formatting {0} into System.Version type.",versionText);
            var currentVersion = ParseVersion(versionText);

            context.Log.InfoFormat("Setting CurrentVersion: {0}",currentVersion);
            context.Data["CurrentVersion"] = currentVersion;
            Environment.SetEnvironmentVariable("octgnnewversion",currentVersion.ToString(),EnvironmentVariableTarget.User);
        }

        public virtual Version ParseVersion(string versionText)
        {
            return Version.Parse(versionText);
        }
    }
}
