namespace Octgn.ReleasePusher.Test.Tasks
{
    using System;
    using System.Collections.Generic;
    using System.IO;

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
                                   { "ReplaceVersionIgnoreFile", "" },
                                   { "WorkingDirectory",""},
                                   {"CurrentVersion",new Version("3.0.0.0")},
                                   {"NewVersion", new Version("3.0.0.1")}
                               };
            var badData1 = new Dictionary<string, object>
                               {
                                   { "WorkingDirectory",""},
                                   {"CurrentVersion",new Version("3.0.0.0")},
                                   {"NewVersion", new Version("3.0.0.1")}
                               };

            var badData2 = new Dictionary<string, object>
                               {
                                   { "ReplaceVersionIgnoreFile", "" },
                                   {"CurrentVersion",new Version("3.0.0.0")},
                                   {"NewVersion", new Version("3.0.0.1")}
                               };
            var badData3 = new Dictionary<string, object>
                               {
                                   { "ReplaceVersionIgnoreFile", "" },
                                   { "WorkingDirectory",""},
                                   {"NewVersion", new Version("3.0.0.1")}
                               };
            var badData4 = new Dictionary<string, object>
                               {
                                   { "ReplaceVersionIgnoreFile", "" },
                                   { "WorkingDirectory",""},
                                   {"CurrentVersion",new Version("3.0.0.0")},
                               };
            var goodContext = A.Fake<ITaskContext>(x=>x.Wrapping(new TaskContext(A.Fake<ILog>(),fileSystem,goodData)));
            var badContext1 = A.Fake<ITaskContext>(x=>x.Wrapping(new TaskContext(A.Fake<ILog>(),fileSystem,badData1)));
            var badContext2 = A.Fake<ITaskContext>(x=>x.Wrapping(new TaskContext(A.Fake<ILog>(),fileSystem,badData2)));
            var badContext3 = A.Fake<ITaskContext>(x=>x.Wrapping(new TaskContext(A.Fake<ILog>(),fileSystem,badData3)));
            var badContext4 = A.Fake<ITaskContext>(x=>x.Wrapping(new TaskContext(A.Fake<ILog>(),fileSystem,badData4)));

            var goodFiles = new[] { "asdf.txt", "face/asdf.txt", "face/brains/asdf.txt" };

            A.CallTo(
                () =>
                task.ProcessFile(A<ITaskContext>.Ignored, A<FileInfoBase>.Ignored, A<string[]>.Ignored,A<string>.Ignored, A<string>.Ignored)).DoesNothing();

            // Good data, grabs all files
            A.CallTo(
                () => fileSystem.Directory.GetFiles(A<string>.Ignored, A<string>.Ignored, SearchOption.AllDirectories))
             .Returns(goodFiles);
            Assert.DoesNotThrow(() => task.Run(this, goodContext));
            A.CallTo(
                () =>
                task.ProcessFile(A<ITaskContext>.Ignored, A<FileInfoBase>.Ignored, A<string[]>.Ignored,A<string>.Ignored, A<string>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Times(3));

            // bad data 1
            A.CallTo(
                () => fileSystem.Directory.GetFiles(A<string>.Ignored, A<string>.Ignored, SearchOption.AllDirectories))
             .Returns(goodFiles);
            Assert.Throws<KeyNotFoundException>(() => task.Run(this, badContext1));
            A.CallTo(
                () =>
                task.ProcessFile(A<ITaskContext>.Ignored, A<FileInfoBase>.Ignored, A<string[]>.Ignored,A<string>.Ignored, A<string>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Times(3));

            // bad data 2
            A.CallTo(
                () => fileSystem.Directory.GetFiles(A<string>.Ignored, A<string>.Ignored, SearchOption.AllDirectories))
             .Returns(goodFiles);
            Assert.Throws<KeyNotFoundException>(() => task.Run(this, badContext2));
            A.CallTo(
                () =>
                task.ProcessFile(A<ITaskContext>.Ignored, A<FileInfoBase>.Ignored, A<string[]>.Ignored,A<string>.Ignored, A<string>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Times(3));

            // bad data 3
            A.CallTo(
                () => fileSystem.Directory.GetFiles(A<string>.Ignored, A<string>.Ignored, SearchOption.AllDirectories))
             .Returns(goodFiles);
            Assert.Throws<KeyNotFoundException>(() => task.Run(this, badContext3));
            A.CallTo(
                () =>
                task.ProcessFile(A<ITaskContext>.Ignored, A<FileInfoBase>.Ignored, A<string[]>.Ignored,A<string>.Ignored, A<string>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Times(3));

            // bad data 4
            A.CallTo(
                () => fileSystem.Directory.GetFiles(A<string>.Ignored, A<string>.Ignored, SearchOption.AllDirectories))
             .Returns(goodFiles);
            Assert.Throws<KeyNotFoundException>(() => task.Run(this, badContext4));
            A.CallTo(
                () =>
                task.ProcessFile(A<ITaskContext>.Ignored, A<FileInfoBase>.Ignored, A<string[]>.Ignored,A<string>.Ignored, A<string>.Ignored))
                .MustHaveHappened(Repeated.Exactly.Times(3));



        }

        [Test]
        public void ProcessFile()
        {
            var fileSystem = A.Fake<IFileSystem>();
            var context = A.Fake<ITaskContext>(x=>x.Wrapping(new TaskContext(A.Fake<ILog>(),fileSystem)));
            var task = new IncrementVersionNumbersInFiles();

            const string CurrentVersion = "3.0.0.0";
            const string NewVersion = "3.0.0.1";

            const string NoVersion = "asldkfjaw faowkjef awoeijf a;sodkfjaw oeifjaw\nfawo\teifj\tawoef";
            const string NoVersionResult = NoVersion;
            const string HasVersion = "falskdjfawoeka wef\n\r\n\tlaskdjfaoweifjaw awoiefjaw" + CurrentVersion + "fjowiejf\n";
            var HasVersionResult = HasVersion.Replace(CurrentVersion,NewVersion);

            var ignoreFiles = new[] { "ignore1.txt", "ignore2.txt" };
            var passFile = A.Fake<FileInfoBase>();
            var nonpassFile = A.Fake<FileInfoBase>();

            context.Data["WorkingDirectory"] = "c:\\asdf\\";

            A.CallTo(() => passFile.FullName).Returns(@"c:\face\brains\tits.txt");
            A.CallTo(() => nonpassFile.FullName).Returns(@"c:\face\brains\ignore2.txt");
            A.CallTo(() => passFile.Name).Returns(@"tits.txt");
            A.CallTo(() => nonpassFile.Name).Returns(@"ignore2.txt");

            var fileContents = "";
            A.CallTo(() => fileSystem.File.WriteAllText(A<string>.Ignored, A<string>.Ignored))
             .Invokes(new Action<string, string>((s, s1) => fileContents = s1));

            // file allowed, has version
            fileContents = "";
            A.CallTo(() => fileSystem.File.ReadAllText(A<string>.Ignored)).Returns(HasVersion);
            Assert.DoesNotThrow(()=>task.ProcessFile(context,passFile,ignoreFiles,CurrentVersion,NewVersion));
            A.CallTo(()=>fileSystem.File.WriteAllText(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(()=>fileSystem.File.ReadAllText(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            Assert.AreEqual(HasVersionResult,fileContents);
            
            // file allowed, no version
            fileContents = "";
            A.CallTo(() => fileSystem.File.ReadAllText(A<string>.Ignored)).Returns(NoVersion);
            Assert.DoesNotThrow(() => task.ProcessFile(context, passFile, ignoreFiles, CurrentVersion, NewVersion));
            A.CallTo(()=>fileSystem.File.WriteAllText(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(()=>fileSystem.File.ReadAllText(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Times(2));
            Assert.AreEqual("",fileContents);

            // file not allowed, has version
            fileContents = "";
            A.CallTo(() => fileSystem.File.ReadAllText(A<string>.Ignored)).Returns(HasVersion);
            Assert.DoesNotThrow(() => task.ProcessFile(context, nonpassFile, ignoreFiles, CurrentVersion, NewVersion));
            A.CallTo(()=>fileSystem.File.WriteAllText(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(()=>fileSystem.File.ReadAllText(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Times(2));
            Assert.AreEqual("",fileContents);

            // file not allowed, has no version
            fileContents = "";
            A.CallTo(() => fileSystem.File.ReadAllText(A<string>.Ignored)).Returns(NoVersion);
            Assert.DoesNotThrow(() => task.ProcessFile(context, nonpassFile, ignoreFiles, CurrentVersion, NewVersion));
            A.CallTo(()=>fileSystem.File.WriteAllText(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Once);
            A.CallTo(()=>fileSystem.File.ReadAllText(A<string>.Ignored)).MustHaveHappened(Repeated.Exactly.Times(2));
            Assert.AreEqual("",fileContents);
        }

        [Test]
        public void GetIgnoreFiles()
        {
            var context = A.Fake<ITaskContext>();
            var task = new IncrementVersionNumbersInFiles();

            const string Good = "asdf.txt";
            const string Good2 = "asdf.txt,abcd.txt";
            const string Good3 = "asdf.txt,abcd.txt,   ,face.jim";

            var goodres = new []{ "asdf.txt" };
            var goodres2 = new []{ "asdf.txt","abcd.txt" };
            var goodres3 = new []{ "asdf.txt","abcd.txt","face.jim" };

            string bad = null;
            const string Bad2 = "";

            string[] res = null;
            Assert.DoesNotThrow(() =>res = task.GetIgnoreFiles(context,Good));
            Assert.AreEqual(goodres,res);

            Assert.DoesNotThrow(()=>res = task.GetIgnoreFiles(context,Good2));
            Assert.AreEqual(goodres2,res);

            Assert.DoesNotThrow(()=>res = task.GetIgnoreFiles(context,Good3));
            Assert.AreEqual(goodres3,res);

            Assert.Throws<NullReferenceException>(() => res = task.GetIgnoreFiles(context, bad));

            Assert.DoesNotThrow(()=> res = task.GetIgnoreFiles(context,Bad2));
            Assert.AreEqual(new string[0],res);
        }
    }
}
