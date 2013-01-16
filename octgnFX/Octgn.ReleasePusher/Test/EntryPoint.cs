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
            var taskManager = Pusher.SetupTaskManager();
            Assert.NotNull(taskManager);

            Assert.AreEqual(3, taskManager.TaskContext.Data.Count);
            Assert.Contains("WorkingDirectory",(ICollection)taskManager.TaskContext.Data.Keys);
            Assert.Contains("CurrentVersionFileRelativePath",(ICollection)taskManager.TaskContext.Data.Keys);
            Assert.Contains("ReplaceVersionIgnoreFile",(ICollection)taskManager.TaskContext.Data.Keys);
            Assert.False(string.IsNullOrWhiteSpace(taskManager.TaskContext.Data["WorkingDirectory"] as string));
            Assert.False(string.IsNullOrWhiteSpace(taskManager.TaskContext.Data["CurrentVersionFileRelativePath"] as string));
            Assert.False(string.IsNullOrWhiteSpace(taskManager.TaskContext.Data["ReplaceVersionIgnoreFile"] as string));

            Assert.AreEqual(3,taskManager.Tasks.Count);
            Assert.True(taskManager.Tasks.OfType<GetVersion>().Any());
            Assert.True(taskManager.Tasks.OfType<IncrementVersionNumberTask>().Any());
            Assert.True(taskManager.Tasks.OfType<IncrementVersionNumbersInFiles>().Any());
        }
    }
}
