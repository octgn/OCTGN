namespace Octgn.Test.DataNew.FileDB
{
    using System.Linq;
    using System.Reflection;

    using NUnit.Framework;
    using Octgn.DataNew.FileDB;
    using Octgn.DataNew.Entities;
    using Octgn.Library;

    public class CollectionDefinition
    {
        [Test]
        public void Test()
        {
            Paths.WorkingDirectory = System.IO.Directory.GetCurrentDirectory();
            var config = new FileDbConfiguration();
            config
                .SetDirectory(Paths.DataDirectory)
                .DefineCollection<Game>("Games")
                .SetPart(x=>x.Property(y=>y.Id))
                .SetPart(x=>x.File("definition.xml"))
                .Conf()
                .DefineCollection<Set>("Sets")
                .OverrideRoot(x=>x.Directory("Games"))
                .SetPart(x=>x.Property(y=>y.GameId))
                .SetPart(x=>x.Directory("Sets"))
                .SetPart(x=>x.Property(y=>y.Id))
                .SetPart(x=>x.File("set.xml"));

            Assert.AreEqual(2,config.Configurations.Count);
            var conf = config.Configurations[0];
            Assert.AreEqual(2,conf.Parts.Count());

            Assert.AreEqual("{Id}",conf.Parts.First().PartString());
            Assert.AreEqual("definition.xml",conf.Parts.Skip(1).Take(1).First().PartString());
            Assert.AreEqual("Games",conf.Root.PartString());
            foreach(var c in config.Configurations)
                System.Console.WriteLine(c.Path);
//            Assert.Fail();
        }
    }
}