using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octgn.Updater
{
    interface ITaskRunner
    {
        Queue<ITask> TaskStack { get; set; }
        Action<UpdaterContext> TaskRunnerCompleted { get; set; }
        void TaskFailed(UpdaterContext context, ITask task, Exception exception);
        void TaskComplete(UpdaterContext context, ITask task);
        void TaskRunnerComplete(UpdaterContext context);
    }
}
