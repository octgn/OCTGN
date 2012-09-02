using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Octgn.Updater.Tasks
{
    public class KillOctgnTask : ITask
    {
        public bool StopOnFail { get; set; }
        
        public KillOctgnTask()
        {
            StopOnFail = true;
        }

        public void ExecuteTask(UpdaterContext context)
        {
            context.FireLog("Shutting down all instances of OCTGN...");
            for (int i = 0; i < 120;i++ )
            {
                var proc = Process.GetProcessesByName("OCTGN").FirstOrDefault();
                if (proc == null)
                {
                    context.FireLog("All instances of OCTGN shut down.");
                    return;
                }
                proc.Kill();
                Thread.Sleep(1000);
            }
            throw new ApplicationException("Can't shutdown OCTGN. Please kill it manually.");
        }
    }
}
