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
    public class GetVersionTest
    {
        [Test]
        public void Run()
        {
            var data = new Dictionary<string, object>
                           {
                               { "WorkingDirectory", "c:\\face" },
                               {
                                   "CurrentVersionFileRelativePath",
                                   "\\brains\\neck\\CurrentVersion.txt"
                               },
                               {
                                   "CurrentReleaseVersionFileRelativePath",
                                   "\\brains\\neck\\CurrentReleaseVersion.txt"
                               },
                               {
                                   "CurrentTestVersionFileRelativePath",
                                   "\\brains\\neck\\CurrentTestVersion.txt"
                               }
                           };
            var fileSystem = A.Fake<IFileSystem>();
            var log = LogManager.GetLogger(typeof(GetVersion));

            // Do ideal pass currentversion
            
            A.CallTo(()=>fileSystem.File.ReadAllText(A<string>.Ignored)).Returns("3.0.0.0");
            var getVersion = A.Fake<GetVersion>(x=>x.Wrapping(new GetVersion()));
            var context = new TaskContext(log, fileSystem, data);

            A.CallTo(() => getVersion.ParseVersion(A<string>.Ignored)).Returns(new Version(3, 0, 0, 0));
            Assert.DoesNotThrow(()=>getVersion.Run(this,context));
            A.CallTo(() => fileSystem.Path.Combine(context.Data["WorkingDirectory"] as string, context.Data["CurrentVersionFileRelativePath"] as string)).MustHaveHappened(Repeated.Exactly.Once);

            Assert.Contains("CurrentVersion",(ICollection)context.Data.Keys);
            Assert.NotNull(context.Data["CurrentVersion"] as Version);
            Assert.AreEqual(new Version("3.0.0.0"), context.Data["CurrentVersion"] as Version);

            // Make sure it throws an exception if the file doesn't exist.
            A.CallTo(() => fileSystem.File.ReadAllText(A<string>.Ignored)).Throws<System.IO.FileNotFoundException>();

            Assert.Throws<System.IO.FileNotFoundException>(() => getVersion.Run(this, context));

            // Make sure a bad version throws an exception
            A.CallTo(() => fileSystem.File.ReadAllText(A<string>.Ignored)).Returns("OMG LOL NOT A VERSION");
            A.CallTo(() => getVersion.ParseVersion(A<string>.Ignored)).CallsBaseMethod();
            Assert.Throws<ArgumentException>(() => getVersion.Run(this, context));

            // Do ideal pass currentreleaseversion

            fileSystem = A.Fake<IFileSystem>();
            log = LogManager.GetLogger(typeof(GetVersion));
            context = new TaskContext(log, fileSystem, data);
            getVersion = A.Fake<GetVersion>(x => x.Wrapping(new GetVersion()));
            A.CallTo(()=>fileSystem.File.ReadAllText(A<string>.Ignored)).Returns("3.0.0.0");

            A.CallTo(() => getVersion.ParseVersion(A<string>.Ignored)).Returns(new Version(3, 0, 0, 0));
            Assert.DoesNotThrow(()=>getVersion.Run(this,context));
            A.CallTo(() => fileSystem.Path.Combine(context.Data["WorkingDirectory"] as string, context.Data["CurrentReleaseVersionFileRelativePath"] as string)).MustHaveHappened(Repeated.Exactly.Once);

            Assert.Contains("CurrentReleaseVersion",(ICollection)context.Data.Keys);
            Assert.NotNull(context.Data["CurrentReleaseVersion"] as Version);
            Assert.AreEqual(new Version("3.0.0.0"), context.Data["CurrentReleaseVersion"] as Version);

            // Make sure it throws an exception if the file doesn't exist.
            A.CallTo(() => fileSystem.File.ReadAllText(A<string>.Ignored)).Throws<System.IO.FileNotFoundException>();

            Assert.Throws<System.IO.FileNotFoundException>(() => getVersion.Run(this, context));

            // Make sure a bad version throws an exception
            A.CallTo(() => fileSystem.File.ReadAllText(A<string>.Ignored)).Returns("OMG LOL NOT A VERSION");
            A.CallTo(() => getVersion.ParseVersion(A<string>.Ignored)).CallsBaseMethod();
            Assert.Throws<ArgumentException>(() => getVersion.Run(this, context));

            // Do ideal pass currenttestversion
            fileSystem = A.Fake<IFileSystem>();
            log = LogManager.GetLogger(typeof(GetVersion));
            context = new TaskContext(log, fileSystem, data);
            getVersion = A.Fake<GetVersion>(x => x.Wrapping(new GetVersion()));
            A.CallTo(()=>fileSystem.File.ReadAllText(A<string>.Ignored)).Returns("3.0.0.0");

            A.CallTo(() => getVersion.ParseVersion(A<string>.Ignored)).Returns(new Version(3, 0, 0, 0));
            Assert.DoesNotThrow(()=>getVersion.Run(this,context));
            A.CallTo(() => fileSystem.Path.Combine(context.Data["WorkingDirectory"] as string, context.Data["CurrentTestVersionFileRelativePath"] as string)).MustHaveHappened(Repeated.Exactly.Once);

            Assert.Contains("CurrentTestVersion",(ICollection)context.Data.Keys);
            Assert.NotNull(context.Data["CurrentTestVersion"] as Version);
            Assert.AreEqual(new Version("3.0.0.0"), context.Data["CurrentTestVersion"] as Version);

            // Make sure it throws an exception if the file doesn't exist.
            A.CallTo(() => fileSystem.File.ReadAllText(A<string>.Ignored)).Throws<System.IO.FileNotFoundException>();

            Assert.Throws<System.IO.FileNotFoundException>(() => getVersion.Run(this, context));

            // Make sure a bad version throws an exception
            A.CallTo(() => fileSystem.File.ReadAllText(A<string>.Ignored)).Returns("OMG LOL NOT A VERSION");
            A.CallTo(() => getVersion.ParseVersion(A<string>.Ignored)).CallsBaseMethod();
            Assert.Throws<ArgumentException>(() => getVersion.Run(this, context));
        }

        [Test]
        public void ParseVersion()
        {
            const string GoodVersionString = "3.0.0.0";
            var goodVersion = new Version("3.0.0.0");

            const string BadVersionString = "asdf";

            Version result = null;
            Assert.DoesNotThrow(()=>result = new GetVersion().ParseVersion(GoodVersionString));
            Assert.AreEqual(goodVersion,result);

            Assert.Throws<ArgumentException>(() => result = new GetVersion().ParseVersion(BadVersionString));

        }
    }
}
