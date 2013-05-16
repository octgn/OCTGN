namespace Octgn.ReleasePusher.Test
{
    using System;
    using System.Threading;

    using FakeItEasy;

    using NUnit.Framework;

    using Octgn.ReleasePusher.Tasking;

    [TestFixture]
    public class TaskManagerTest
    {
        [Test]
        public void Constructor()
        {
            var taskManager = new TaskManager();
            Assert.NotNull(taskManager.TaskContext);
            Assert.NotNull(taskManager.Tasks);
            Assert.AreEqual(0, taskManager.Tasks.Count);
        }

        [Test]
        public void AddTask()
        {
            var taskManager = new TaskManager();
            var task = A.Fake<ITask>();

            Assert.AreEqual(0,taskManager.Tasks.Count);
            taskManager.AddTask(task);
            Assert.AreEqual(1,taskManager.Tasks.Count);
            Assert.AreEqual(task,taskManager.Tasks[0]);
        }

        [Test]
        public void Run()
        {
            var taskManager = new TaskManager();
            var task = A.Fake<ITask>();
            var taskBad = A.Fake<ITask>();
            var taskBadContinue = A.Fake<ITask>();

            A.CallTo(() => taskBad.Run(A<object>.Ignored, A<ITaskContext>.Ignored)).Throws<Exception>();
            A.CallTo(() => taskBadContinue.Run(A<object>.Ignored, A<ITaskContext>.Ignored)).Throws<ContinuableException>();

            // expect a good run
            taskManager.AddTask(task);
            Assert.DoesNotThrow(taskManager.Run);

            // Bad run
            taskManager.Tasks[0] = taskBad;
            Assert.Throws<Exception>(taskManager.Run);

            // Bad containable run.
            taskManager.Tasks[0] = taskBadContinue;
            Assert.DoesNotThrow(taskManager.Run);

            // Good task, bad task containable, then bad task, then good task.
            taskManager.Tasks[0] = task;
            taskManager.AddTask(taskBadContinue);
            taskManager.AddTask(taskBad);
            taskManager.AddTask(task);
            Assert.Throws<Exception>(taskManager.Run);
            A.CallTo(()=>task.Run(A<object>.Ignored,A<ITaskContext>.Ignored)).MustHaveHappened(Repeated.Exactly.Times(2));
            A.CallTo(()=>taskBadContinue.Run(A<object>.Ignored,A<ITaskContext>.Ignored)).MustHaveHappened(Repeated.Exactly.Times(2));
            A.CallTo(()=>taskBad.Run(A<object>.Ignored,A<ITaskContext>.Ignored)).MustHaveHappened(Repeated.Exactly.Times(2));
        }

        [Test]
        public void Stop()
        {
            var taskManager = new TaskManager();
            var task1 = A.Fake<ITask>();
            var task2 = A.Fake<ITask>();

            A.CallTo(() => task1.Run(A<object>.Ignored, A<ITaskContext>.Ignored)).Invokes(() => Thread.Sleep(1000));

            taskManager.AddTask(task1);
            taskManager.AddTask(task2);

            var finishedCount = 0;

            new Action(taskManager.Run)
                .BeginInvoke(
                ar =>
                    {
                        A.CallTo(() => task1.Run(A<object>.Ignored, A<ITaskContext>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
                        A.CallTo(() => task2.Run(A<object>.Ignored, A<ITaskContext>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
                        finishedCount++;
                    },
                null);
            while(finishedCount == 0)
                Thread.Sleep(10);
            new Action(taskManager.Run)
                .BeginInvoke(
                ar =>
                    {
                        A.CallTo(() => task1.Run(A<object>.Ignored, A<ITaskContext>.Ignored)).MustHaveHappened(Repeated.Exactly.Times(2));
                        A.CallTo(() => task2.Run(A<object>.Ignored, A<ITaskContext>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
                        finishedCount++;
                    },
                null);
            Thread.Sleep(100);
            taskManager.Stop();
            while(finishedCount ==1)
                Thread.Sleep(10);
        }
    }
}
