using System;
using System.IO;
using System.Reflection;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Octgn.ReleasePusher
{
    using System.Configuration;
    using System.Linq;

    using Octgn.ReleasePusher.Tasking;
    using Octgn.ReleasePusher.Tasking.Tasks;

    public class Pusher
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger (MethodBase.GetCurrentMethod().DeclaringType);
        internal static TaskManager TaskManager { get; set; }
        public static int Main(string[] args)
        {
            Log.InfoFormat("Arguments: {0}",String.Join(" ",args));
            if (args[0].ToLower() == "setup")
            {
                Log.Info("Doing setup tasks");

                var mode = args.FirstOrDefault(x => x.ToLower().StartsWith("/m"));
                if(string.IsNullOrWhiteSpace(mode))
                    throw new ArgumentException("/mTEST or /mRELEASE needs to be specified.");
                mode = mode.Substring(2);
                Log.InfoFormat("Doing release for {0} mode",mode);

                TaskManager = SetupTaskManager(mode);

                TaskManager.Run();
                PauseForKey();
                return 0;
            }
            Log.Error("No arguments");
            return -1;

        }

        internal static TaskManager SetupTaskManager(string mode)
        {
            var taskManager = new TaskManager();
            taskManager.AddTask(new GetVersion());
            taskManager.AddTask(new IncrementVersionNumberTask());
            taskManager.AddTask(new IncrementVersionNumbersInFiles());
            taskManager.AddTask(new AddRecentChanges());
            taskManager.AddTask(new CreatePushBatFile());

            // Get working directory
            var workingDirectory = Assembly.GetAssembly(typeof(Pusher)).Location;
            workingDirectory = new DirectoryInfo(workingDirectory).Parent.Parent.Parent.Parent.Parent.FullName;
            taskManager.TaskContext.Data["WorkingDirectory"] = workingDirectory;

            // Get CurrentVersion.txt relative path
            var curVerRelPath = Path.Combine("octgnFX", "Octgn");
            curVerRelPath = Path.Combine(curVerRelPath, "CurrentVersion.txt");
            taskManager.TaskContext.Data["CurrentVersionFileRelativePath"] = curVerRelPath;

            // Get CurrentReleaseVersion.txt relative path
            var curRelVerRelPath = Path.Combine("octgnFX", "Octgn");
            curRelVerRelPath = Path.Combine(curRelVerRelPath, "CurrentReleaseVersion.txt");
            taskManager.TaskContext.Data["CurrentReleaseVersionFileRelativePath"] = curRelVerRelPath;

            // Get CurrentReleaseVersion.txt relative path
            var curTestVerRelPath = Path.Combine("octgnFX", "Octgn");
            curTestVerRelPath = Path.Combine(curTestVerRelPath, "CurrentTestVersion.txt");
            taskManager.TaskContext.Data["CurrentTestVersionFileRelativePath"] = curTestVerRelPath;

            // Load all of our app.config settings into the data section.
            for (var i = 0; i < ConfigurationManager.AppSettings.Count;i++ )
            {
                taskManager.TaskContext.Data[ConfigurationManager.AppSettings.AllKeys[i]] =
                    ConfigurationManager.AppSettings[i];
            }

            taskManager.TaskContext.Data["Mode"] = mode;

            return taskManager;
        }

        private static void PauseForKey()
        {
#if(DEBUG)
            Console.ReadKey();
#endif
        }
    }
}
