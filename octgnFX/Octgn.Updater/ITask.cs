using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octgn.Updater
{
    public interface ITask
    {
        bool StopOnFail { get; set; }
        void ExecuteTask(UpdaterContext context);
    }
}
