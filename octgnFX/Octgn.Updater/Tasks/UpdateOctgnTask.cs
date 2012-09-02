using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Ionic.Zip;

namespace Octgn.Updater.Tasks
{
    public class UpdateOctgnTask : ITask
    {
        public bool StopOnFail { get; set; }

        public UpdateOctgnTask()
        {
            StopOnFail = true;
        }

        public void ExecuteTask(UpdaterContext context)
        {
            var octgnDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "OCTGN");
            var octgnInstallPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location).Parent;
            var octgnUpdatePath = Path.Combine(octgnDataPath, "update.zip");
            context.FireLog("Octgn install path : {0}",octgnInstallPath);
            context.FireLog("Octgn update file  : {0}",octgnUpdatePath);
            context.FireLog("");
            if(!context.FileSystem.File.Exists(octgnUpdatePath))
            {
                context.FireLog("No update found.");
                return;
            }
            context.FireLog("Update exists. Unpacking.");
            using (var zip = ZipFile.Read(octgnUpdatePath))
            {
                foreach( var p in zip )
                {
                    var unPath = Path.Combine(octgnInstallPath.FullName, p.FileName);
                    context.FireLog("Extracting[{0}]: {1}",p.FileName,unPath);
                    p.Extract(octgnInstallPath.FullName,ExtractExistingFileAction.OverwriteSilently);
                }
            }
            context.FireLog("Deleting update file.");
            context.FileSystem.File.Delete(octgnUpdatePath);
        }
    }
}
