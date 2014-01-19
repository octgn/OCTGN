namespace Octgn.ReleasePusher.Test.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using FakeItEasy;

    using NUnit.Framework;

    using Octgn.ReleasePusher.Tasking;
    using Octgn.ReleasePusher.Tasking.Tasks;

    using log4net;
    using System.IO.Abstractions;

    [TestFixture]
    public class IncrementVersionNumbersInFilesTest
    {
        [Test]
        public void Run()
        {
            var fileSystem = A.Fake<IFileSystem>();
            var task = A.Fake<IncrementVersionNumbersInFiles>(x=>x.Wrapping(new IncrementVersionNumbersInFiles()));
            var goodData = new Dictionary<string, object>
                               {
                                   { "WorkingDirectory",""},
                                   {"CurrentVersion",new Version("3.0.0.0")},
                                   {"CurrentReleaseVersion",new Version("3.0.0.0")},
                                   {"CurrentTestVersion",new Version("3.0.0.0")},
                                   {"NewVersion", new Version("3.0.0.1")},
                                   {"Mode", "test"}
                               };

            var badData1 = new Dictionary<string, object>
                               {
                                   {"CurrentVersion",new Version("3.0.0.0")},
                                   {"NewVersion", new Version("3.0.0.1")},
                                   {"Mode", "test"}
                               };
 
            var badData2 = new Dictionary<string, object>
                               {
                                   {"CurrentVersion",new Version("3.0.0.0")},
                                   {"CurrentReleaseVersion",new Version("3.0.0.0")},
                                   {"NewVersion", new Version("3.0.0.1")},
                                   {"Mode", "test"}
                               };
            var badData3 = new Dictionary<string, object>
                               {
                                   { "WorkingDirectory",""},
                                   {"CurrentReleaseVersion",new Version("3.0.0.0")},
                                   {"NewVersion", new Version("3.0.0.1")},
                                   {"Mode", "test"}
                               };
            var badData4 = new Dictionary<string, object>
                               {
                                   { "WorkingDirectory",""},
                                   {"CurrentReleaseVersion",new Version("3.0.0.0")},
                                   {"CurrentVersion",new Version("3.0.0.0")},
                                   {"Mode","test"}
                               };
            var badData5 = new Dictionary<string, object>
                               {
                                   { "WorkingDirectory",""},
                                   {"CurrentVersion",new Version("3.0.0.0")},
                                   {"CurrentReleaseVersion",new Version("3.0.0.0")},
                                   {"NewVersion", new Version("3.0.0.1")},
                                   {"Mode", "test"}
                               };
            var goodContext = A.Fake<ITaskContext>(x=>x.Wrapping(new TaskContext(A.Fake<ILog>(),fileSystem,goodData)));
            var badContext2 = A.Fake<ITaskContext>(x=>x.Wrapping(new TaskContext(A.Fake<ILog>(),fileSystem,badData2)));
            var badContext3 = A.Fake<ITaskContext>(x=>x.Wrapping(new TaskContext(A.Fake<ILog>(),fileSystem,badData3)));
            var badContext4 = A.Fake<ITaskContext>(x=>x.Wrapping(new TaskContext(A.Fake<ILog>(),fileSystem,badData4)));
            var badContext5 = A.Fake<ITaskContext>(x=>x.Wrapping(new TaskContext(A.Fake<ILog>(),fileSystem,badData5)));

            var goodFiles = new[] { "c:\\asdf.txt", "c:\\face\\asdf.txt", "c:\\face\\brains\\asdf.txt" };
            var badFiles = new[] { "c:\\face.txt", "c:\\face2\\asdf.txt", "c:\\face","c:\\asdf" };

            A.CallTo(() => task.GetUpdateFiles(A<ITaskContext>.Ignored)).Returns(goodFiles);
            A.CallTo(
                () =>
                task.ProcessFile(A<ITaskContext>.Ignored, A<FileInfoBase>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).DoesNothing();

            // Good data, grabs all files
            A.CallTo(
                () => fileSystem.Directory.GetFiles(A<string>.Ignored, A<string>.Ignored, SearchOption.AllDirectories))
             .Returns(goodFiles);
            Assert.DoesNotThrow(() => task.Run(this, goodContext));
            A.CallTo(
                () =>
                task.ProcessFile(A<ITaskContext>.Ignored, A<FileInfoBase>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Times(3));

            // Inject bad files
            A.CallTo(
                () => fileSystem.Directory.GetFiles(A<string>.Ignored, A<string>.Ignored, SearchOption.AllDirectories))
             .Returns(goodFiles.Union(badFiles).ToArray());
            Assert.DoesNotThrow(() => task.Run(this, goodContext));
            A.CallTo(
                () =>
                task.ProcessFile(A<ITaskContext>.Ignored, A<FileInfoBase>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Times(6));


            // bad data 1
            A.CallTo(
                () => fileSystem.Directory.GetFiles(A<string>.Ignored, A<string>.Ignored, SearchOption.AllDirectories))
             .Returns(goodFiles);
            Assert.Throws<KeyNotFoundException>(() => task.Run(this, badContext2));
            A.CallTo(
                () =>
                task.ProcessFile(A<ITaskContext>.Ignored, A<FileInfoBase>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Times(6));

            // bad data 2
            A.CallTo(
                () => fileSystem.Directory.GetFiles(A<string>.Ignored, A<string>.Ignored, SearchOption.AllDirectories))
             .Returns(goodFiles);
            Assert.Throws<KeyNotFoundException>(() => task.Run(this, badContext2));
            A.CallTo(
                () =>
                task.ProcessFile(A<ITaskContext>.Ignored, A<FileInfoBase>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Times(6));

            // bad data 3
            A.CallTo(
                () => fileSystem.Directory.GetFiles(A<string>.Ignored, A<string>.Ignored, SearchOption.AllDirectories))
             .Returns(goodFiles);
            Assert.Throws<KeyNotFoundException>(() => task.Run(this, badContext3));
            A.CallTo(
                () =>
                task.ProcessFile(A<ITaskContext>.Ignored, A<FileInfoBase>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Times(6));

            // bad data 4
            A.CallTo(
                () => fileSystem.Directory.GetFiles(A<string>.Ignored, A<string>.Ignored, SearchOption.AllDirectories))
             .Returns(goodFiles);
            Assert.Throws<KeyNotFoundException>(() => task.Run(this, badContext4));
            A.CallTo(
                () =>
                task.ProcessFile(A<ITaskContext>.Ignored, A<FileInfoBase>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Times(6));

            // bad data 4
            A.CallTo(
                () => fileSystem.Directory.GetFiles(A<string>.Ignored, A<string>.Ignored, SearchOption.AllDirectories))
             .Returns(goodFiles);
            Assert.Throws<KeyNotFoundException>(() => task.Run(this, badContext5));
            A.CallTo(
                () =>
                task.ProcessFile(A<ITaskContext>.Ignored, A<FileInfoBase>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Times(6));


        }

        [Test]
        public void ProcessFile()
        {
            var fileSystem = A.Fake<IFileSystem>();
            A.CallTo(()=>fileSystem.Path).Returns(A.Fake<PathBase>(x=>x.Wrapping(new PathWrapper())));
            var context = A.Fake<ITaskContext>(x=>x.Wrapping(new TaskContext(A.Fake<ILog>(),fileSystem)));
            var task = new IncrementVersionNumbersInFiles();

            const string CurrentVersion = "3.0.0.0";
            const string CurrentReleaseVersion = "3.0.0.0";
            const string CurrentTestVersion = "3.0.0.0";
            const string NewVersion = "3.0.0.1";

            const string NoVersion = "asldkfjaw faowkjef awoeijf a;sodkfjaw oeifjaw\nfawo\teifj\tawoef";
            const string NoVersionResult = NoVersion;
            const string HasVersion = "falskdjfawoeka wef\n\r\n\tlaskdjfaoweifjaw awoiefjaw" + CurrentVersion + "fjowiejf\n";
            var HasVersionResult = HasVersion.Replace(CurrentVersion,NewVersion);

            var passFile = A.Fake<FileInfoBase>();

            context.Data["WorkingDirectory"] = "c:\\asdf\\";
            context.Data["Mode"] = "test";

            A.CallTo(() => passFile.FullName).Returns(task.GetUpdateFiles(context).First());
            var a = task.GetUpdateFiles(context).First();
            Debug.WriteLine(a);
            A.CallTo(() => passFile.Name).Returns(new FileInfo(task.GetUpdateFiles(context).First()).Name);

            var fileContents = "";
            A.CallTo(() => fileSystem.File.WriteAllText(A<string>.Ignored, A<string>.Ignored))
             .Invokes(new Action<string, string>((s, s1) => fileContents = s1));

            // has version
            fileContents = "";
            A.CallTo(() => fileSystem.File.ReadAllText(A<string>.Ignored)).Returns(HasVersion);
            Assert.DoesNotThrow(() => task.ProcessFile(context, passFile, CurrentVersion, CurrentReleaseVersion, CurrentTestVersion,NewVersion));
            A.CallTo(()=>fileSystem.File.WriteAllText(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(()=>fileSystem.File.ReadAllText(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            Assert.AreEqual(HasVersionResult,fileContents);
            
            // no version
            fileContents = "";
            A.CallTo(() => fileSystem.File.ReadAllText(A<string>.Ignored)).Returns(NoVersion);
            Assert.DoesNotThrow(() => task.ProcessFile(context, passFile, CurrentVersion, CurrentReleaseVersion, CurrentTestVersion, NewVersion));
            A.CallTo(()=>fileSystem.File.WriteAllText(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(()=>fileSystem.File.ReadAllText(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Times(2));
            Assert.AreEqual("",fileContents);
        }

        [Test]
        public void GetUpdateFiles()
        {
            var fileSystem = A.Fake<IFileSystem>();
            A.CallTo(() => fileSystem.Path).Returns(A.Fake<PathBase>(x => x.Wrapping(new PathWrapper())));
            var context = A.Fake<ITaskContext>(x=>x.Wrapping(new TaskContext(A.Fake<ILog>(),fileSystem)));
            var task = new IncrementVersionNumbersInFiles();

            //A.CallTo(() => task.CreateUpdateString(A<ITaskContext>.Ignored, A<string>.Ignored)).Returns("");
            context.Data["WorkingDirectory"] = "";
            context.Data["Mode"] = "release";

            // Test modes
            context.Data["Mode"] = "release";
            Assert.DoesNotThrow(()=>task.GetUpdateFiles(context));
            Assert.AreEqual(22,task.GetUpdateFiles(context).Length);
            context.Data["Mode"] = "test";
            Assert.DoesNotThrow(() => task.GetUpdateFiles(context));
            Assert.AreEqual(22,task.GetUpdateFiles(context).Length);
            context.Data["Mode"] = "a";
            Assert.Throws<InvalidOperationException>(() => task.GetUpdateFiles(context));

            string[] result = null;

            // Release Mode Specific Files
            context.Data["Mode"] = "release";
            result = task.GetUpdateFiles(context);
            Assert.True(result.Any(x => x.Contains("deploy\\currentversion.txt")));
            Assert.True(result.Any(x => x.Contains("installer\\Install.nsi")));
            Assert.True(result.Any(x => x.Contains("octgnFX\\Octgn\\CurrentReleaseVersion.txt")));
            Assert.False(result.Any(x => x.Contains("deploy\\currentversiontest.txt")));
            Assert.False(result.Any(x => x.Contains("installer\\InstallTest.nsi")));
            Assert.False(result.Any(x => x.Contains("octgnFX\\Octgn\\CurrentTestVersion.txt")));

            // Release Mode Specific Files
            context.Data["Mode"] = "test";
            result = task.GetUpdateFiles(context);
            Assert.True(result.Any(x => x.Contains("deploy\\currentversiontest.txt")));
            Assert.True(result.Any(x => x.Contains("installer\\InstallTest.nsi")));
            Assert.True(result.Any(x => x.Contains("octgnFX\\Octgn\\CurrentTestVersion.txt")));
            Assert.False(result.Any(x => x.Contains("deploy\\currentversion.txt")));
            Assert.False(result.Any(x => x.Contains("installer\\Install.nsi")));
            Assert.False(result.Any(x => x.Contains("octgnFX\\Octgn\\CurrentReleaseVersion.txt")));
        }
    }
}
