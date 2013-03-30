namespace Octgn.Test.DataNew.FileDB
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using NUnit.Framework;

    using Octgn.DataNew;
    using Octgn.DataNew.FileDB;
    using Octgn.DataNew.Entities;
    using Octgn.Library;
    using Octgn.Library.ExtensionMethods;
    using Octgn.ProxyGenerator;

    public class CollectionDefinition
    {
        [SetUp]
        public void Setup()
        {
            Paths.Get().WorkingDirectory = System.IO.Directory.GetCurrentDirectory();
        }
        [Test]
        public void Test()
        {
            var config = new FileDbConfiguration();
            config
                .SetDirectory(Paths.Get().DataDirectory)
                .DefineCollection<Game>("GameDatabase")
                .SetPart(x => x.Property(y => y.Id))
                .SetPart(x => x.File("definition.xml"))
                .Conf()
                .DefineCollection<Set>("Sets")
                .OverrideRoot(x => x.Directory("GameDatabase"))
                .SetPart(x => x.Property(y => y.GameId))
                .SetPart(x => x.Directory("Sets"))
                .SetPart(x => x.Property(y => y.Id))
                .SetPart(x => x.File("set.xml"));

            Assert.AreEqual(2, config.Configurations.Count);
            var conf = config.Configurations[0];
            Assert.AreEqual(2, conf.Parts.Count());

            Assert.AreEqual("{Id}", conf.Parts.First().PartString());
            Assert.AreEqual("definition.xml", conf.Parts.Skip(1).Take(1).First().PartString());
            Assert.AreEqual("GameDatabase", conf.Root.PartString());
            foreach (var c in config.Configurations)
            {
                System.Console.WriteLine(c.Path);
                foreach (var d in new DirectoryInfo(c.Path).Split())
                {
                    Console.WriteLine(d);
                }
            }

            //config.Query<Game>().By(x => x.Id,Op.Eq, );
        }
        [Test]
        public void Enumerate()
        {
            var dbconfig = DbContext.Get().Db;


            foreach (var config in dbconfig.Config.Configurations)
            {
                var root = new DirectoryInfo(Path.Combine(dbconfig.Config.Directory, config.Root.PartString()));
                foreach (var r in root.SplitFull()) if (!Directory.Exists(r.FullName)) Directory.CreateDirectory(r.FullName);

                var searchList = new List<DirectoryInfo>();

                searchList.Add(root);
                var done = false;
                foreach (var part in config.Parts)
                {
                    if (done) break;
                    switch (part.PartType)
                    {
                        case PartType.Directory:
                            for (var i = 0; i < searchList.Count; i++)
                            {
                                searchList[i] =
                                    new DirectoryInfo(Path.Combine(searchList[i].FullName, part.PartString()));
                                if (!Directory.Exists(searchList[i].FullName)) Directory.CreateDirectory(searchList[i].FullName);
                            }
                            break;
                        case PartType.Property:
                            //if next not file
                            var newList = new List<DirectoryInfo>();
                            foreach (var i in searchList)
                            {
                                newList.AddRange(i.GetDirectories());
                            }
                            searchList = newList;
                            break;
                        case PartType.File:
                            var remList = new List<DirectoryInfo>();
                            foreach (var i in searchList)
                            {
                                if (i.GetFiles().Any(x => x.Name == part.PartString()) == false) remList.Add(i);
                            }
                            foreach (var i in remList) searchList.Remove(i);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                //Console.WriteLine(timer.ElapsedMilliseconds);
                //foreach (var f in searchList) Console.WriteLine(f.FullName);
            }
            var timer = new Stopwatch();
            timer.Start();
            foreach (var g in dbconfig.Query<Game>())
            {
                
            }
            timer.Stop();
            Console.WriteLine("GameLoadTime: {0}",timer.ElapsedMilliseconds);
            timer.Reset();
            timer.Start();
            var cardCount = 0;
            var markerCount = 0;
            foreach (var s in dbconfig.Query<Set>())
            {
                cardCount += s.Cards.Count();
                markerCount += s.Markers.Count();
            }
            foreach (var s in dbconfig.Query<GameScript>())
            {
                Console.WriteLine(s.Path);
            }
            foreach (var p in dbconfig.Query<ProxyDefinition>())
            {
                Console.WriteLine(p.Key);
            }
            timer.Stop();
            Console.WriteLine("TotalCards: {0}",cardCount);
            Console.WriteLine("TotalMarkers: {0}",markerCount);
            Console.WriteLine("TotalLoadTime: {0}",timer.ElapsedMilliseconds);

            //Assert.Fail();
        }

    }
}