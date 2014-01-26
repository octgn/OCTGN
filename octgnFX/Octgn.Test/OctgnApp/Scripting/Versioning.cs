using System;
using System.Collections.Generic;
using FakeItEasy;
using NUnit.Framework;
using Octgn.Library;
using Octgn.Scripting;

namespace Octgn.Test.OctgnApp.Scripting
{
    public class VersionedTests
    {
		[Test]
		public void ValidVersion_ReturnsFalseIfVersionNotRegistered()
		{
            SetDebug(false);
            Versioned.Setup(false);
            Versioned.RegisterVersion(new Version(3, 1, 0, 0), DateTime.MinValue, ReleaseMode.Live);

            Versioned.Register<TestTypeBase>();

            var val = Versioned.ValidVersion(new Version(3, 1, 0, 1));

            Assert.False(val);
		}

		[Test]
		public void ValidVersion_WorksProperlyForDebugMode()
		{
            SetDebug(true);
            Versioned.Setup(false);
            Versioned.RegisterVersion(new Version(3, 1, 0, 0), DateTime.MinValue, ReleaseMode.Live);
            Versioned.RegisterVersion(new Version(3, 1, 0, 1), DateTime.MinValue, ReleaseMode.Test);

            Versioned.Register<TestTypeBase>();

		    var val = Versioned.ValidVersion(new Version(3, 1, 0, 1));

			Assert.True(val);
		}

        [Test]
        public void ValidVersion_WorksProperlyForDeveloperMode()
        {
            SetDebug(true);
            Versioned.Setup(false);
            Versioned.RegisterVersion(new Version(3, 1, 0, 0), DateTime.MinValue, ReleaseMode.Live);
            Versioned.RegisterVersion(new Version(3, 1, 0, 1), DateTime.MinValue, ReleaseMode.Test);

            Versioned.Register<TestTypeBase>();

            var val = Versioned.ValidVersion(new Version(3, 1, 0, 1));

            Assert.True(val);
        }

        [Test]
        public void ValidVersion_WorksProperlyForLiveMode()
        {
            SetDebug(false);
            Versioned.Setup(false);
            Versioned.RegisterVersion(new Version(3, 1, 0, 0), DateTime.MinValue, ReleaseMode.Live);
            Versioned.RegisterVersion(new Version(3, 1, 0, 1), DateTime.MinValue, ReleaseMode.Test);

            Versioned.Register<TestTypeBase>();

            var val = Versioned.ValidVersion(new Version(3, 1, 0, 1));

            Assert.False(val);

            val = Versioned.ValidVersion(new Version(3, 1, 0, 0));

			Assert.True(val);
        }

		[Test]
		public void Get_Throws_IfTypeRegisteredType()
		{
            SetDebug(false);
			Versioned.Setup(false);
			Versioned.RegisterVersion(new Version(3,1,0,0),DateTime.MinValue,ReleaseMode.Live );
		    Assert.Throws<InvalidOperationException>(() =>
		        {
                    var g = Versioned.Get<TestTypeBase>(new Version(3, 1, 0, 0));
		        });
		}

		[Test]
		public void Get_Throws_IfVersionNotRegistered()
		{
            SetDebug(false);
            Versioned.Setup(false);
            Assert.Throws<InvalidOperationException>(() =>
            {
                var g = Versioned.Get<TestTypeBase>(new Version(3, 1, 0, 0));
            });		    
		}

		[Test]
		public void Get_Throws_IfNoTypeForVersionRegistered()
		{
            SetDebug(false);
            Versioned.Setup(false);
			Versioned.RegisterVersion(new Version(3,1,0,0),DateTime.MinValue,ReleaseMode.Live );
			Versioned.RegisterVersion(new Version(3,1,0,1),DateTime.MinValue,ReleaseMode.Live );
            Versioned.Register<TestTypeBase>();
            Assert.Throws<InvalidOperationException>(() =>
            {
                var g = Versioned.Get<TestTypeBase>(new Version(3, 1, 0, 2));
            });	
		}

		[Test]
		public void Get_Throws_IfRequestedVersionIsTestAndWeAreInLive()
		{
            Versioned.Setup(false);
			SetDebug(false);
		    var curx = X.Instance;
		    var testx = A.Fake<IX>();
		    A.CallTo(() => testx.Debug).Returns(false);
		    X.SingletonContext = testx;

            Versioned.RegisterVersion(new Version(3, 1, 0, 0), DateTime.MinValue, ReleaseMode.Live);
            Versioned.RegisterVersion(new Version(3, 1, 0, 1), DateTime.MinValue, ReleaseMode.Test);
            Versioned.Register<TestTypeBase>();
            Assert.Throws<InvalidOperationException>(() =>
            {
                var g = Versioned.Get<TestTypeBase>(new Version(3, 1, 0, 1));
            });	

		    X.SingletonContext = curx;
		}

		[Test]
		public void Get_ReturnsProperVersionInTest()
		{
            SetDebug(true);
            Versioned.Setup(false);
            Versioned.RegisterVersion(new Version(3, 1, 0, 0), DateTime.MinValue, ReleaseMode.Live);
            Versioned.RegisterVersion(new Version(3, 1, 0, 1), DateTime.MinValue, ReleaseMode.Test);

			Versioned.Register<TestTypeBase>();

		    var val = Versioned.Get<TestTypeBase>(new Version(3, 1, 0, 1));

            Assert.AreEqual(typeof(TestType_3_1_0_1),val.GetType());
		}

        [Test]
        public void Get_ReturnsProperVersionInDevMode()
        {
            SetDebug(false);
            Versioned.Setup(true);
            Versioned.RegisterVersion(new Version(3, 1, 0, 0), DateTime.MinValue, ReleaseMode.Live);
            Versioned.RegisterVersion(new Version(3, 1, 0, 1), DateTime.MinValue, ReleaseMode.Test);

            Versioned.Register<TestTypeBase>();

            var val = Versioned.Get<TestTypeBase>(new Version(3, 1, 0, 1));

            Assert.AreEqual(typeof(TestType_3_1_0_1), val.GetType());
        }

		[Test]
		public void Get_ReturnsProperVersionInLive()
		{
            SetDebug(false);
            Versioned.Setup(false);
            Versioned.RegisterVersion(new Version(3, 1, 0, 0), DateTime.MinValue, ReleaseMode.Live);
            Versioned.RegisterVersion(new Version(3, 1, 0, 1), DateTime.MinValue, ReleaseMode.Test);

			Versioned.Register<TestTypeBase>();

		    var val = Versioned.Get<TestTypeBase>(new Version(3, 1, 0, 0));

            Assert.AreEqual(typeof(TestType_3_1_0_0),val.GetType());
		}

		[Test]
		public void Syntax()
		{
            Versioned.Setup(false);
            var gep = Versioned.Get<TestTypeBase>(new Version(3, 1, 0, 0));
		}

		private void SetDebug(bool d)
		{
            var testx = A.Fake<IX>();
            A.CallTo(() => testx.Debug).Returns(d);
            X.SingletonContext = testx;
		}
    }

	internal class TestTypeBase
	{
	    
	}
	[Versioned("3.1.0.0")]
	internal class TestType_3_1_0_0 : TestTypeBase
	{
	    
	}
    [Versioned("3.1.0.1")]
    internal class TestType_3_1_0_1 : TestTypeBase
    {

    }

    internal class TestType_3_1_0_2 : TestTypeBase
    {

    }
}