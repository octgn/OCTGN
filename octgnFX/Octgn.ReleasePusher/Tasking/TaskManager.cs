namespace Octgn.ReleasePusher.Tasking
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
        private bool stop;

        public TaskManager()
        {
            this.Tasks = new List<ITask>();
            this.TaskContext = new TaskContext(null);
        }

        public void AddTask(ITask task)
        {
            Tasks.Add(task);
        }

        public void Run()
        {
            lock (this)
            {
                if (this.isRunning) return;
                this.isRunning = true;
            }
            try
            {
                Log.Info("Starting Tasks");
                foreach (var task in this.Tasks)
                {
                    if (this.stop)
                    {
                        Log.InfoFormat("Manual Stop");
                        return;
                    }

                    var taskName = task.GetType().Name;

                    try
                    {

                        Log.InfoFormat("Starting Task {0}", taskName);
                        this.TaskContext = new TaskContext(log4net.LogManager.GetLogger(task.GetType()),TaskContext.Data);
                        task.Run(this, this.TaskContext);
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
                        foreach (var data in this.TaskContext.Data)
                        {
                            Log.DebugFormat("[{0}]: {1}",data.Key,data.Value);
                        }
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
                lock (this)
                {
                    this.isRunning = false;
                    this.stop = false;
                }
            }
        }

        public void Stop()
        {
            Log.Info("Stop Called");
            this.stop = true;
        }
    }
}
