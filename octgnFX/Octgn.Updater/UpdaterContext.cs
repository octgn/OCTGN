using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octgn.Updater
{
    public class UpdaterContext
    {
        internal IFileSystem FileSystem { get; set; }

        internal Action<UpdaterContext,ITask,Exception> TaskFailed { get; set; }
        internal Action<UpdaterContext,string> Log { get; set; }
        internal Action<UpdaterContext,ITask> TaskComplete { get; set; }

        internal UpdaterContext()
        {
            FileSystem = new FileSystem();
        }
        
        internal void FireTaskFailed(ITask task, Exception e)
        {
            if(TaskFailed != null)
                TaskFailed.Invoke(this,task, e);
        }

        internal void FireLog(String format, params object[] args)
        {
            if(Log != null)
                Log.Invoke(this,String.Format(format,args));
        }

        internal void FireTaskComplete(ITask task)
        {
            if(TaskComplete != null)
                TaskComplete.Invoke(this,task);
        }
    }
}
