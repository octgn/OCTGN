namespace Octgn.Test.OctgnApp.Play.State
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using NUnit.Framework;

    using Octgn.DataNew.Entities;
    using Octgn.Play.State;

    public class StateSaveTests
    {
        [Test]
        public void IsSerializable()
        {
            var list = typeof(StateSave<>).GetCustomAttributes(typeof(System.SerializableAttribute), true);
            Assert.AreEqual(1,list.Length);
        }
        
        [Test]
        public void CreateWorks()
        {
            var ss = StateSave<Octgn.DataNew.Entities.Game>.Create(new Octgn.DataNew.Entities.Game());
            Assert.NotNull(ss);
        }

        [Test]
        public void CreateSetsInstance()
        {
            var testList = new Octgn.DataNew.Entities.Game();

            var ss = StateSave<List<string>>.Create(testList);
            Assert.AreEqual(testList, ss.GetInstance());
        }

        [Test]
        public void GetSetInstanceWorks()
        {
            var testList = new Octgn.DataNew.Entities.Game()
                               {
                                   Name = "Chicken"
                               };
            var ss = StateSave<Octgn.DataNew.Entities.Game>.Create(new Octgn.DataNew.Entities.Game());
            Assert.AreNotEqual(testList.Name, (ss.GetInstance() as Octgn.DataNew.Entities.Game).Name);
            ss.SetInstance(testList);
            Assert.AreEqual(testList,ss.GetInstance());
        }

        [Test]
        public void SaveLoadStateWork()
        {
            var ss = (TestStateSave)StateSave<Octgn.DataNew.Entities.Game>.Create(new Octgn.DataNew.Entities.Game());
            ss.SetData();
            ss.SaveState();
            ss.SetInstance(null);
            ss.SetInstance(new Game());
            ss.LoadState();
            ss.VerifyLoadedData();
        }

        [Test]
        public void CreateFailsIfNoStateSaveImplementationFound()
        {
            Assert.Throws<InvalidOperationException>(() => StateSave<Exception>.Create(new Exception()));
        }

        [Test]
        public void TestCreateTimes()
        {
            long firstTime = 0;
            long firstTimeMs = 0;
            long secondTime = 0;
            long secondTimeMs = 0;
            var sw = new Stopwatch();
            sw.Start();
            StateSave<Octgn.DataNew.Entities.Game>.Create(new Octgn.DataNew.Entities.Game());
            sw.Stop();
            firstTime = sw.ElapsedTicks;
            firstTimeMs = sw.ElapsedMilliseconds;
            sw.Reset();
            sw.Start();
            StateSave<Octgn.DataNew.Entities.Game>.Create(new Octgn.DataNew.Entities.Game());
            sw.Stop();
            secondTime = sw.ElapsedTicks;
            secondTimeMs = sw.ElapsedMilliseconds;
            Console.WriteLine("FirstTime: " + firstTime);
            Console.WriteLine("SecondTime: " + secondTime);
            Console.WriteLine("FirstTimeMs: " + firstTimeMs);
            Console.WriteLine("SecondTimeMs: " + secondTimeMs);
            Assert.True(firstTime > secondTime);
            //Assert.LessOrEqual(firstTimeMs,1000);
            //Assert.LessOrEqual(secondTimeMs, 100);
        }
    }

    [Serializable]
    public class TestStateSave : StateSave<Octgn.DataNew.Entities.Game>
    {
        private Guid gameId = Guid.NewGuid();
        public TestStateSave(Octgn.DataNew.Entities.Game instance)
            : base(instance)
        {
        }

        public override void SaveState()
        {
            Save(x=>x.Table);
            Save(x=>x.Name);
            Save(x=>x.Sounds);
            Save(x=>x.Id,gameId);
            Save(x=>x.OctgnVersion,new Version(1,2,3,4).ToString());
            Save(x=>x.InstallPath,"c:\\");
            Save(x=>x.IconUrl,new Uri("http://www.google.com"));
        }

        public override void LoadState()
        {
            Load(x=>x.Table);
            Load(x => x.Name);
            Load(x => x.Sounds);
            Load(x=>x.Id);
            Load<Version,string>(x => x.OctgnVersion, (instance, value) => Version.Parse(value) );
            var ipath = LoadAndReturn(x => x.InstallPath);
            Instance.InstallPath = ipath;

            var iconUrl = LoadAndReturn<string, Uri>(x => x.IconUrl);
            Instance.IconUrl = iconUrl.ToString();

        }

        public void SetData()
        {
            Instance.Name = "asdf1";
            Instance.Sounds = new Dictionary<string, GameSound>{{"asdf",new GameSound(){Name = "asdf"}}};
        }

        public void VerifyLoadedData()
        {
            Assert.AreEqual("asdf1",Instance.Name);
            Assert.AreEqual(1,Instance.Sounds.Count);
            Assert.AreEqual("asdf",Instance.Sounds["asdf"].Name);
            Assert.AreEqual(gameId,Instance.Id);
            Assert.AreEqual(new Version(1,2,3,4),Instance.OctgnVersion);
            Assert.AreEqual("c:\\",Instance.InstallPath);
            Assert.AreEqual(new Uri("http://www.google.com/").ToString(),Instance.IconUrl);
        }
    }
}