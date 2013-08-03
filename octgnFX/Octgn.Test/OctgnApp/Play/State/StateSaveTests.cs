namespace Octgn.Test.OctgnApp.Play.State
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using NUnit.Framework;

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
            var ss = StateSave<List<string>>.Create(new List<string>());
            Assert.NotNull(ss);
        }

        [Test]
        public void CreateSetsInstance()
        {
            var testList = new List<string>();
            testList.Add("asdf");

            var ss = StateSave<List<string>>.Create(testList);
            Assert.AreEqual(testList, ss.GetInstance());
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
            StateSave<List<String>>.Create(new List<String>());
            sw.Stop();
            firstTime = sw.ElapsedTicks;
            firstTimeMs = sw.ElapsedMilliseconds;
            sw.Reset();
            sw.Start();
            StateSave<List<String>>.Create(new List<String>());
            sw.Stop();
            secondTime = sw.ElapsedTicks;
            secondTimeMs = sw.ElapsedMilliseconds;
            Console.WriteLine("FirstTime: " + firstTime);
            Console.WriteLine("SecondTime: " + secondTime);
            Console.WriteLine("FirstTimeMs: " + firstTimeMs);
            Console.WriteLine("SecondTimeMs: " + secondTimeMs);
            Assert.True(firstTime > secondTime);
            Assert.LessOrEqual(firstTimeMs,1000);
            Assert.LessOrEqual(secondTimeMs, 100);
        }
    }

    public class TestStateSave : StateSave<List<string>>
    {
        public TestStateSave(List<string> instance)
            : base(instance)
        {
        }

        public override void SaveState()
        {
            
        }

        public override void LoadState()
        {
            
        }
    }
}