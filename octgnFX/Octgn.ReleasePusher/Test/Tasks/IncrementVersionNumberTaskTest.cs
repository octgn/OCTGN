namespace Octgn.ReleasePusher.Test.Tasks
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO.Abstractions;

    using FakeItEasy;

    using NUnit.Framework;

    using Octgn.ReleasePusher.Tasking;
    using Octgn.ReleasePusher.Tasking.Tasks;

    using log4net;

    [TestFixture]
    public class IncrementVersionNumberTaskTest
    {
        [Test]
        public void Run()
        {
            var data = new Dictionary<string, object>
                           {
                               { "CurrentVersion", new Version("3.0.0.0") },
                               {"Mode","release"}
                           };
            var badData = new Dictionary<string, object>
                              {
                                  { "CurrentVersion", new Exception("Lol") },
                                   {"Mode","release"}
                              };
            var log = LogManager.GetLogger(typeof(IncrementVersionNumberTask));

            var context = new TaskContext(log, data);

            var incTask = new IncrementVersionNumberTask();

            // -- RELEASE --
            // Good run
            
            Assert.DoesNotThrow(()=>incTask.Run(this,context));
            Assert.Contains("CurrentVersion",(ICollection)context.Data.Keys);
            Assert.Contains("NewVersion",(ICollection)context.Data.Keys);
            Assert.AreEqual(new Version(3,0,0,0),context.Data["CurrentVersion"]);
            Assert.AreEqual(new Version(3,0,1,0),context.Data["NewVersion"]);

            // Try no input version
            context = new TaskContext(log,new Dictionary<string, object>());
            Assert.Throws<KeyNotFoundException>(() => incTask.Run(this, context));

            // Try bad version number
            context = new TaskContext(log,badData);
            Assert.Throws<NullReferenceException>(() => incTask.Run(this, context));


            // -- TEST --
            // Good run
            context = new TaskContext(log,data);
            context.Data["Mode"] = "test";

            Assert.DoesNotThrow(()=>incTask.Run(this,context));
            Assert.Contains("CurrentVersion",(ICollection)context.Data.Keys);
            Assert.Contains("NewVersion",(ICollection)context.Data.Keys);
            Assert.AreEqual(new Version(3,0,0,0),context.Data["CurrentVersion"]);
            Assert.AreEqual(new Version(3,0,0,1),context.Data["NewVersion"]);

            // Try no input version
            context = new TaskContext(log,new Dictionary<string, object>());
            Assert.Throws<KeyNotFoundException>(() => incTask.Run(this, context));

            // Try bad version number
            context = new TaskContext(log,badData);
            context.Data["Mode"] = "test";
            Assert.Throws<NullReferenceException>(() => incTask.Run(this, context));
        }
    }
}
