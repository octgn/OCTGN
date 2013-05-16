namespace Octgn.ReleasePusher.Tasking.Tasks
{
    using System;

    public class GetVersion:ITask
    {
        public void Run(object sender, ITaskContext context)
        {
            this.GetCurrentVersion(context);
            this.GetCurrentReleaseVersion(context);
            this.GetCurrentTestVersion(context);
        }

        public void GetCurrentVersion(ITaskContext context)
        {
            context.Log.Info("Reading Data Settings");
            var rootPath = context.Data["WorkingDirectory"] as string;
            var currentVersionFileRelativePath = context.Data["CurrentVersionFileRelativePath"] as string;

            var fullPath = context.FileSystem.Path.Combine(rootPath, currentVersionFileRelativePath);

            context.Log.InfoFormat("Reading {0}", fullPath);
            var versionText = context.FileSystem.File.ReadAllText(fullPath);

            context.Log.InfoFormat("Formatting {0} into System.Version type.", versionText);
            var currentVersion = ParseVersion(versionText);

            context.Log.InfoFormat("Setting CurrentVersion: {0}", currentVersion);
            context.Data["CurrentVersion"] = currentVersion;
        }

        public void GetCurrentReleaseVersion(ITaskContext context)
        {
            context.Log.Info("Reading Data Settings");
            var rootPath = context.Data["WorkingDirectory"] as string;
            var currentReleaseVersionFileRelativePath = context.Data["CurrentReleaseVersionFileRelativePath"] as string;

            var fullReleasePath = context.FileSystem.Path.Combine(rootPath, currentReleaseVersionFileRelativePath);

            context.Log.InfoFormat("Reading {0}", fullReleasePath);
            var releaseVersionText = context.FileSystem.File.ReadAllText(fullReleasePath);

            context.Log.InfoFormat("Formatting {0} into System.Version type.", releaseVersionText);
            var currentReleaseVersion = ParseVersion(releaseVersionText);

            context.Log.InfoFormat("Setting CurrentReleaseVersion: {0}", currentReleaseVersion);
            context.Data["CurrentReleaseVersion"] = currentReleaseVersion;
        }

        public void GetCurrentTestVersion(ITaskContext context)
        {
            context.Log.Info("Reading Data Settings");
            var rootPath = context.Data["WorkingDirectory"] as string;
            var currentTestVersionFileRelativePath = context.Data["CurrentTestVersionFileRelativePath"] as string;

            var fullTestPath = context.FileSystem.Path.Combine(rootPath, currentTestVersionFileRelativePath);

            context.Log.InfoFormat("Reading {0}", fullTestPath);
            var TestVersionText = context.FileSystem.File.ReadAllText(fullTestPath);

            context.Log.InfoFormat("Formatting {0} into System.Version type.", TestVersionText);
            var currentTestVersion = ParseVersion(TestVersionText);

            context.Log.InfoFormat("Setting CurrentTestVersion: {0}", currentTestVersion);
            context.Data["CurrentTestVersion"] = currentTestVersion;
        }

        public virtual Version ParseVersion(string versionText)
        {
            return Version.Parse(versionText);
        }
    }
}
