using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octgn.Updater.Tools
{
    public class ProcessHelpers
    {
        public static string GetFileProcessName(string filePath)
        {

            Process[] procs = Process.GetProcesses();
            string fileName = Path.GetFileName(filePath);

            foreach (Process proc in procs)
            {
                if (proc.MainWindowHandle != new IntPtr(0) && !proc.HasExited)
                {
                    ProcessModule[] arr = new ProcessModule[proc.Modules.Count];
                    foreach (ProcessModule pm in proc.Modules)
                    {
                        if (pm.ModuleName == fileName)
                            return proc.ProcessName;
                    }
                }
            }


            return null;
        }
    }
}
