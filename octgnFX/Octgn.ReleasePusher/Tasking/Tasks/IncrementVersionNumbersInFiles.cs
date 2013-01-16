namespace Octgn.ReleasePusher.Tasking.Tasks
{
    using System;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;

    public class IncrementVersionNumbersInFiles : ITask
    {
        public void Run(object sender, ITaskContext context)
        {
            context.Log.Info("Running");

            var filesToIgnore = this.GetIgnoreFiles(context,context.Data["ReplaceVersionIgnoreFile"] as string);
            var workingDirectory = context.Data["WorkingDirectory"] as string;
            var currentVersion = (context.Data["CurrentVersion"] as Version).ToString();
            var newVersion = (context.Data["NewVersion"] as Version).ToString();

            var files = context.FileSystem.Directory
                .GetFiles(workingDirectory, "*", SearchOption.AllDirectories)
                .Select(x=>new FileInfoWrapper(new FileInfo(x)));

            foreach (var f in files )
            {
                this.ProcessFile(context,f,filesToIgnore,currentVersion,newVersion);
            }
        }

        public virtual void ProcessFile(ITaskContext context, FileInfoBase file, string[] filesToIgnore, string currentVersion, string newVersion)
        {
            // Should we ignore this?
            if (filesToIgnore.Contains(file.Name)) return;

            // Read the whole file.
            context.Log.InfoFormat("Reading file {0}",file.FullName);
            var text = context.FileSystem.File.ReadAllText(file.FullName);

            // Return if the file contents don't contain the version number
            if (!text.Contains(currentVersion)) return;
            context.Log.InfoFormat("Replacing version number in file {0}",file.FullName);

            // Replace all occurrences of the oldVersion with the newVersion
            text = text.Replace(currentVersion, newVersion);
            context.Log.InfoFormat("Writing file {0}",file.FullName);

            // Write the new file to the file system.
            context.FileSystem.File.WriteAllText(file.FullName, text);
        }

        public virtual string[] GetIgnoreFiles(ITaskContext context,string ignoreList)
        {
            context.Log.InfoFormat("Ignoring files: {0}", ignoreList);
            return (ignoreList).Split(new char[1]{','},StringSplitOptions.RemoveEmptyEntries).Where(x=>!String.IsNullOrWhiteSpace(x)).ToArray();
        }
    }
}
