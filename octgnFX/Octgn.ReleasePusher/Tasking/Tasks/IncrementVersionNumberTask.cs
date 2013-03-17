namespace Octgn.ReleasePusher.Tasking.Tasks
{
    using System;
    using System.Linq;

    public class IncrementVersionNumberTask : ITask
    {
        public void Run(object sender, ITaskContext context)
        {
            if ((context.Data["Mode"] as string).ToLower() == "test")
            {
                var version = context.Data["CurrentVersion"] as Version;
                context.Log.InfoFormat("Current version: {0}", version);
                context.Data["NewVersion"] = new Version(
                    version.Major, version.Minor, version.Build, version.Revision + 1);
                context.Log.InfoFormat("New Version: {0}", context.Data["NewVersion"] as Version);
            }
            else if ((context.Data["Mode"] as string).ToLower() == "release")
            {
                var version = context.Data["CurrentVersion"] as Version;
                context.Log.InfoFormat("Current version: {0}", version);
                context.Data["NewVersion"] = new Version(
                    version.Major, version.Minor, version.Build + 1, version.Revision);
                context.Log.InfoFormat("New Version: {0}", context.Data["NewVersion"] as Version);
                
            }
        }
    }
}
