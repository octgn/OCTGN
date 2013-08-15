namespace Octgn.ReleasePusher.Test
{
    using System.Collections;
    using System.Linq;

    using NUnit.Framework;

    using Octgn.ReleasePusher.Tasking.Tasks;

    [TestFixture]
    public class EntryPoint
    {
        [Test]
        public void SetupTaskManager()
        {
            var taskManager = Pusher.SetupTaskManager("release");
            Assert.NotNull(taskManager);

            Assert.AreEqual(5, taskManager.TaskContext.Data.Count);
            Assert.Contains("WorkingDirectory",(ICollection)taskManager.TaskContext.Data.Keys);
            Assert.Contains("CurrentVersionFileRelativePath",(ICollection)taskManager.TaskContext.Data.Keys);
            Assert.Contains("CurrentReleaseVersionFileRelativePath",(ICollection)taskManager.TaskContext.Data.Keys);
            Assert.Contains("CurrentTestVersionFileRelativePath",(ICollection)taskManager.TaskContext.Data.Keys);
            Assert.Contains("Mode",(ICollection)taskManager.TaskContext.Data.Keys);
            Assert.False(string.IsNullOrWhiteSpace(taskManager.TaskContext.Data["WorkingDirectory"] as string));
            Assert.False(string.IsNullOrWhiteSpace(taskManager.TaskContext.Data["CurrentVersionFileRelativePath"] as string));
            Assert.False(string.IsNullOrWhiteSpace(taskManager.TaskContext.Data["CurrentReleaseVersionFileRelativePath"] as string));
            Assert.False(string.IsNullOrWhiteSpace(taskManager.TaskContext.Data["CurrentTestVersionFileRelativePath"] as string));
            Assert.False(string.IsNullOrWhiteSpace(taskManager.TaskContext.Data["Mode"] as string));

            Assert.AreEqual(5,taskManager.Tasks.Count);
            Assert.True(taskManager.Tasks.OfType<GetVersion>().Any());
            Assert.True(taskManager.Tasks.OfType<IncrementVersionNumberTask>().Any());
            Assert.True(taskManager.Tasks.OfType<IncrementVersionNumbersInFiles>().Any());
            Assert.True(taskManager.Tasks.OfType<CreatePushBatFile>().Any());
            Assert.True(taskManager.Tasks.OfType<AddRecentChanges>().Any());
        }
    }
}
