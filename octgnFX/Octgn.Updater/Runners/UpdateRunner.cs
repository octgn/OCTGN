using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octgn.Updater.Tasks;

namespace Octgn.Updater.Runners
{
    public class UpdateRunner : TaskRunner
    {
        public bool FailedUpdate { get; set; }
        public UpdateRunner()
        {
            FailedUpdate = false;
            AddTask(new KillOctgnTask());
            AddTask(new UpdateOctgnTask());
        }
        public override void TaskFailed(UpdaterContext context, ITask task, Exception exception)
        {
            FailedUpdate = true;
            context.FireLog("Error: {0}",exception.Message);
        }

        public override void TaskComplete(UpdaterContext context, ITask task)
        {
        }

        public override void TaskRunnerComplete(UpdaterContext context)
        {
            context.FireLog("Update Runner Complete.");
        }
    }
}
