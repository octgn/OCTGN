namespace Octgn.ReleasePusher.Tasking
{
    using System.Collections.Generic;
    using System.IO.Abstractions;

    using NUnit.Framework;

    using log4net;

    [TestFixture]
    public class TaskContextTest
    {
        [Test]
        public void Constructors()
        {
            var log = LogManager.GetLogger(typeof(TaskContextTest));
            var data = new Dictionary<string, object>();
            var fileSystem = new FileSystem();

            var context = new TaskContext(log);
            Assert.NotNull(context.Log);
            Assert.NotNull(context.FileSystem);
            Assert.NotNull(context.Data);
            Assert.AreEqual(log,context.Log);
            
            context = new TaskContext(log,data);
            Assert.NotNull(context.Log);
            Assert.NotNull(context.FileSystem);
            Assert.NotNull(context.Data);
            Assert.AreEqual(log,context.Log);
            Assert.AreEqual(data,context.Data);

            context = new TaskContext(log,fileSystem);
            Assert.NotNull(context.Log);
            Assert.NotNull(context.FileSystem);
            Assert.NotNull(context.Data);
            Assert.AreEqual(log,context.Log);
            Assert.AreEqual(fileSystem,context.FileSystem);

            context = new TaskContext(log,fileSystem,data);
            Assert.NotNull(context.Log);
            Assert.NotNull(context.FileSystem);
            Assert.NotNull(context.Data);
            Assert.AreEqual(log,context.Log);
            Assert.AreEqual(fileSystem,context.FileSystem);
            Assert.AreEqual(data,context.Data);
        }
    }
}
