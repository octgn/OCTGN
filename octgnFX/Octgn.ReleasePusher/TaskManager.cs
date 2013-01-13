namespace Octgn.ReleasePusher
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class TaskManager
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public ITaskContext TaskContext { get; private set; }
        public IList<ITask> Tasks { get; private set; }

        private bool isRunning;

        public void AddTask(ITask task)
        {
            this.Tasks = new List<ITask>();
            this.TaskContext = default(ITaskContext);
        }

        public void Run()
        {
            lock (this)
            {
                if (isRunning) return;
                isRunning = true;
            }
            try
            {
                Log.Info("Starting Tasks");
                foreach (var task in Tasks)
                {
                    var taskName = task.GetType().Name;
                    try
                    {
                        Log.InfoFormat("Starting Task {0}", taskName);
                        task.Run(this, TaskContext);
                    }
                    catch (ContinuableException e)
                    {
                        Log.Warn(String.Format("Task {0} failed. Continuing to next task.", taskName));
                        Log.Warn(String.Format("Task {0} Exception", taskName), e.InnerException ?? e);
                    }
                    catch (Exception e)
                    {
                        Log.Warn(String.Format("Task {0} failed. Halting tasks.", taskName));
                        Log.Warn(String.Format("Task {0} Exception", taskName), e);
                        throw;
                    }
                    finally
                    {
                        Log.InfoFormat("Completed Task {0}", taskName);
                    }
                }

            }
            finally
            {
                Log.Info("Completed Tasks");
                isRunning = false;
            }
        }

        public void Stop()
        {
            
        }
    }
}
