namespace Octgn.ReleasePusher.Tasking.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    using log4net;

    public class AddRecentChanges : ITask
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        
        public void Run(object sender, ITaskContext context)
        {
            Log.Info("Updating changelog from recentchanges");
            var workingDirectory = context.Data["WorkingDirectory"] as string;
            var newVersion = (context.Data["NewVersion"] as Version).ToString();
            var mode = (context.Data["Mode"] as string).ToLower();

            var changeLogFilename = Path.Combine(workingDirectory, "CHANGELOG.md");
            var recentChangesFilename = Path.Combine(workingDirectory, "recentchanges.txt");

            Log.InfoFormat("Changelog Filename {0}", changeLogFilename);
            Log.InfoFormat("Recent Changes Filename {0}", recentChangesFilename);

            var clLines = File.ReadAllLines(changeLogFilename);

            var outArr = new List<string>();

            if (mode == "release")
            {
                outArr.Add("#" + newVersion);
            }
            else if (mode == "test")
            {
                outArr.Add("#" + newVersion + " - Test");
            }

            var changeLines = File.ReadAllLines(recentChangesFilename);

            foreach (var l in changeLines)
            {
                outArr.Add("+ " + l);
            }

            outArr.Add("");

            outArr.AddRange(clLines);

            File.WriteAllLines(changeLogFilename,outArr);
            if (mode == "release")
            {
                using (var file = File.Open(recentChangesFilename, FileMode.Truncate, FileAccess.Write, FileShare.None))
                {
                    file.Flush(true);
                    file.Close();
                }
                
            }
        }
    }
}