using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Octgn.Updater.Runners;

namespace Octgn.Updater
{
    using System.Configuration;

    internal static class Updater
    {
        public static List<string> LogList { get; set; }
        public static UpdateRunner Runner { get; set; }
        public static string ServerPath { get; set; }
            /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        static void Main()
            {
                ServerPath = ConfigurationManager.AppSettings["ServerPath"]; 
            Application.ThreadException += ApplicationOnThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            LogList = new List<string>();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ApplicationExit += ApplicationOnApplicationExit;
            var frm = new frmLog();
            Application.Run(new frmLog());
        }

        private static void ApplicationOnThreadException(object sender, ThreadExceptionEventArgs threadExceptionEventArgs)
        {
            var ex = threadExceptionEventArgs.Exception;
            UpdateFailed fail = new UpdateFailed(ex);
            fail.ShowDialog();
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            var ex = unhandledExceptionEventArgs.ExceptionObject as Exception;
            var fail = new UpdateFailed(ex);
            fail.ShowDialog();
        }

        private static void ApplicationOnApplicationExit(object sender, EventArgs eventArgs)
        {
            var fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            
            var logPath = Path.Combine(fi.DirectoryName,  "Logs");
            if(!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);
            LogList.Add(String.Format("Opening octgn {0}", Path.Combine(fi.DirectoryName, "../", "octgn.exe")));
            var fullLogPath = Path.Combine(logPath,DateTime.Now.ToFileTimeUtc().ToString() + ".update.log");
            File.WriteAllLines(fullLogPath,LogList);
            Process.Start(Path.Combine(fi.DirectoryName,"../","octgn.exe"));
        }
    }
}
