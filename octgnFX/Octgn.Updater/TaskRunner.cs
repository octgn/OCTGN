using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Octgn.Updater
{
    public abstract class TaskRunner : ITaskRunner
    {
        public Queue<ITask> TaskStack { get; set; }
        public Action<UpdaterContext> TaskRunnerCompleted { get; set; }

        protected internal TaskRunner()
        {
            TaskStack = new Queue<ITask>();
        }

        protected internal void AddTask(ITask task)
        {
            TaskStack.Enqueue(task);
        }

        protected internal void Start(UpdaterContext context)
        {
            if (context.TaskComplete != null) context.TaskComplete -= OnTaskComplete;
            if (context.TaskFailed != null) context.TaskFailed -= OnTaskFailed;
            context.TaskComplete += OnTaskComplete;
            context.TaskFailed += OnTaskFailed;
            new Action(() => LaunchNextTask(context)).BeginInvoke(null,null);
        }

        protected internal void LaunchNextTask(UpdaterContext context)
        {
            if(TaskStack.Count == 0)
            {
                new Action(() => OnTaskRunnerComplete(context)).BeginInvoke(null, null);
                return;
            }
           var thisTask = TaskStack.Peek();
           try
           {
               thisTask.ExecuteTask(context);
           }
           catch (Exception e)
           {
               context.FireTaskFailed(thisTask,e);
               return;
           }
           context.FireTaskComplete(thisTask);
        }

        private void OnTaskFailed(UpdaterContext context, ITask updateTask, Exception exception)
        {
            var poppedTask = TaskStack.Dequeue();
            TaskFailed(context,updateTask,exception);
            if(updateTask.StopOnFail)
                TaskStack.Clear();
            LaunchNextTask(context);
        }

        private void OnTaskComplete(UpdaterContext context,ITask task)
        {
            var poppedTask = TaskStack.Dequeue();
            TaskComplete(context,task);
            LaunchNextTask(context);
        }

        private void OnTaskRunnerComplete(UpdaterContext context)
        {
            TaskRunnerComplete(context);
            if(TaskRunnerCompleted != null)
                TaskRunnerCompleted.Invoke(context);
        }

        public abstract void TaskFailed(UpdaterContext context, ITask task, Exception exception);
        public abstract void TaskComplete(UpdaterContext context, ITask task);
        public abstract void TaskRunnerComplete(UpdaterContext context);
    }
}
