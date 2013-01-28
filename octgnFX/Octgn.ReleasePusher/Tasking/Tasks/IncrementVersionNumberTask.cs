namespace Octgn.ReleasePusher.Tasking.Tasks
{
    using System;

    public class IncrementVersionNumberTask : ITask
    {
        public void Run(object sender, ITaskContext context)
        {
            var version = context.Data["CurrentVersion"] as Version;
            context.Log.InfoFormat("Current version: {0}",version);
            context.Data["NewVersion"] = new Version(version.Major, version.Minor, version.Build, version.Revision + 1);
            context.Log.InfoFormat("New Version: {0}",context.Data["NewVersion"] as Version);
        }
    }
}
