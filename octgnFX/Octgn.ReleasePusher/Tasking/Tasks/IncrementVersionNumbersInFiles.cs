namespace Octgn.ReleasePusher.Tasking.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;

    public class IncrementVersionNumbersInFiles : ITask
    {
        public void Run(object sender, ITaskContext context)
        {
            context.Log.Info("Running");

            var workingDirectory = context.Data["WorkingDirectory"] as string;
            var currentVersion = (context.Data["CurrentVersion"] as Version).ToString();
            var currentReleaseVersion = (context.Data["CurrentReleaseVersion"] as Version).ToString();
            var newVersion = (context.Data["NewVersion"] as Version).ToString();

            var files = context.FileSystem.Directory
                .GetFiles(workingDirectory, "*", SearchOption.AllDirectories)
                .Select(x=>new FileInfoWrapper(new FileInfo(x)))
                .Where(x=> this.GetUpdateFiles(context).Contains(x.FullName,StringComparer.InvariantCultureIgnoreCase));

            foreach (var f in files )
            {
                this.ProcessFile(context, f, currentVersion, currentReleaseVersion, newVersion);
            }
        }

        public virtual void ProcessFile(ITaskContext context, FileInfoBase file, string currentVersion, string currentReleaseVersion, string newVersion)
        {
            var rel = file.FullName.Replace(context.Data["WorkingDirectory"] as string, "").TrimStart('/', '\\');

            // Read the whole file.
            context.Log.InfoFormat("Reading file {0}",file.FullName);
            var text = context.FileSystem.File.ReadAllText(file.FullName);

            // Return if the file contents don't contain the version number
            if (!text.Contains(currentVersion) && !text.Contains(currentReleaseVersion)) return;
            context.Log.InfoFormat("Replacing version number in file {0}",file.FullName);

            // Replace all occurrences of the oldVersion with the newVersion
            text = text.Replace(currentVersion, newVersion);
            text = text.Replace(currentReleaseVersion, newVersion);
            context.Log.InfoFormat("Writing file {0}",file.FullName);

            // Write the new file to the file system.
            context.FileSystem.File.WriteAllText(file.FullName, text);
        }

        public virtual string[] GetUpdateFiles(ITaskContext context)
        {
            var list = new List<String>();
            list.Add(this.CreateUpdateString(context,"currentversion.txt"));
            list.Add(this.CreateUpdateString(context,"nuget\\Octgn.Library.nuspec"));
            list.Add(this.CreateUpdateString(context, "octgnFX\\Octgn\\Properties\\AssemblyInfo.cs"));
            list.Add(this.CreateUpdateString(context, "octgnFX\\Octgn.Data\\Properties\\AssemblyInfo.cs"));
            list.Add(this.CreateUpdateString(context, "octgnFX\\Octgn.DeckBuilderPluginExample\\DeckBuilderPluginExample.cs"));
            list.Add(this.CreateUpdateString(context, "octgnFX\\Octgn.Library\\Properties\\AssemblyInfo.cs"));
            list.Add(this.CreateUpdateString(context, "octgnFX\\Octgn.LobbyServer\\Properties\\AssemblyInfo.cs"));
            list.Add(this.CreateUpdateString(context, "octgnFX\\Octgn.Online.GameService\\Properties\\AssemblyInfo.cs"));
            list.Add(this.CreateUpdateString(context, "octgnFX\\Octgn.ReleasePusher\\Properties\\AssemblyInfo.cs"));
            list.Add(this.CreateUpdateString(context, "octgnFX\\Octgn.Server\\Properties\\AssemblyInfo.cs"));
            list.Add(this.CreateUpdateString(context, "octgnFX\\Octgn.StandAloneServer\\Properties\\AssemblyInfo.cs"));
            list.Add(this.CreateUpdateString(context, "octgnFX\\Octgn.Test\\Properties\\AssemblyInfo.cs"));
            list.Add(this.CreateUpdateString(context, "octgnFX\\Skylabs.Lobby\\Properties\\AssemblyInfo.cs"));
            list.Add(this.CreateUpdateString(context, "octgnFX\\Octgn\\CurrentVersion.txt"));
            if ((context.Data["Mode"] as string).ToLower() == "release")
            {
                list.Add(this.CreateUpdateString(context, "deploy\\currentversion.txt"));
                list.Add(this.CreateUpdateString(context, "installer\\Install.nsi"));
                list.Add(this.CreateUpdateString(context, "octgnFX\\Octgn\\CurrentReleaseVersion.txt"));
            }
            else if ((context.Data["Mode"] as string).ToLower() == "test")
            {
                list.Add(this.CreateUpdateString(context, "deploy\\currentversiontest.txt"));
                list.Add(this.CreateUpdateString(context, "installer\\InstallTest.nsi"));
            }
            else
            {
                throw new InvalidOperationException("Mode must be set to release or test");
            }
            return list.ToArray();
        }

        internal virtual string CreateUpdateString(ITaskContext context, string relativePath)
        {
            return context.FileSystem.Path.Combine(context.Data["WorkingDirectory"] as string, relativePath);
        }
    }
}
