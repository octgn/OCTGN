using System;
using FakeItEasy;
using NUnit.Framework;
using Octgn.Library;
using Octgn.Scripting;

namespace Octgn.Test.OctgnApp.Scripting
{
    [TestFixture]
    public class VersionRegistrationTests
    {
        private IX previousContext;

        [SetUp]
        public void SetUp()
        {
            previousContext = X.Instance;
            X.SingletonContext = A.Fake<IX>();
            Versioned.Setup(false);
        }

        [TearDown]
        public void TearDown()
        {
            Versioned.Setup(false);
            X.SingletonContext = previousContext;
        }

        [Test]
        public void RegisterNewerTestAfterLivePreservesLiveExpirationDates()
        {
            var oldest = new Version(3, 1, 0, 0);
            var previous = new Version(3, 1, 0, 1);
            var latest = new Version(3, 1, 0, 2);
            var test = new Version(3, 1, 0, 3);
            Versioned.RegisterVersion(oldest, new DateTime(2010, 1, 1), ReleaseMode.Live);
            Versioned.RegisterVersion(previous, new DateTime(2011, 1, 1), ReleaseMode.Live);
            Versioned.RegisterVersion(latest, new DateTime(2012, 1, 1), ReleaseMode.Live);

            Versioned.RegisterVersion(test, new DateTime(2013, 1, 1), ReleaseMode.Test);

            Assert.That(Versioned.GetVersion(test).DeleteDate, Is.EqualTo(DateTime.MaxValue));
            Assert.That(Versioned.GetVersion(latest).DeleteDate, Is.EqualTo(DateTime.MaxValue));
            Assert.That(Versioned.GetVersion(previous).DeleteDate, Is.EqualTo(DateTime.MaxValue));
            Assert.That(Versioned.GetVersion(oldest).DeleteDate, Is.EqualTo(new DateTime(2011, 7, 1)));
            Assert.That(Versioned.LatestVersion, Is.EqualTo(latest));
            Assert.That(Versioned.LowestVersion, Is.EqualTo(previous));
            Assert.That(Versioned.ValidVersion(test), Is.False);
        }

        [Test]
        public void RegisterTestBeforeFirstLiveDoesNotRequireLiveMetadata()
        {
            var test = new Version(3, 1, 0, 1);
            var live = new Version(3, 1, 0, 0);

            Versioned.RegisterVersion(test, new DateTime(2011, 1, 1), ReleaseMode.Test);

            Assert.That(Versioned.GetVersion(test).DeleteDate, Is.EqualTo(DateTime.MaxValue));
            Assert.That(Versioned.LatestVersion, Is.Null);
            Assert.That(Versioned.LowestVersion, Is.Null);

            Versioned.RegisterVersion(live, new DateTime(2010, 1, 1), ReleaseMode.Live);

            Assert.That(Versioned.GetVersion(live).DeleteDate, Is.EqualTo(DateTime.MaxValue));
            Assert.That(Versioned.LatestVersion, Is.EqualTo(live));
            Assert.That(Versioned.LowestVersion, Is.EqualTo(live));
        }

        [TestCase(0, 1, 2)]
        [TestCase(0, 2, 1)]
        [TestCase(1, 0, 2)]
        [TestCase(1, 2, 0)]
        [TestCase(2, 0, 1)]
        [TestCase(2, 1, 0)]
        public void NewestTwoLiveVersionsNeverExpireRegardlessOfRegistrationOrder(int first, int second, int third)
        {
            var versions = new[] { new Version(3, 1, 0, 0), new Version(3, 1, 0, 1), new Version(3, 1, 0, 2) };
            var dates = new[] { new DateTime(2010, 1, 1), new DateTime(2011, 1, 1), new DateTime(2012, 1, 1) };
            foreach (var index in new[] { first, second, third })
                Versioned.RegisterVersion(versions[index], dates[index], ReleaseMode.Live);

            Assert.That(Versioned.GetVersion(versions[2]).DeleteDate, Is.EqualTo(DateTime.MaxValue));
            Assert.That(Versioned.GetVersion(versions[1]).DeleteDate, Is.EqualTo(DateTime.MaxValue));
            Assert.That(Versioned.GetVersion(versions[0]).DeleteDate, Is.EqualTo(dates[1].AddMonths(6)));
            Assert.That(Versioned.LatestVersion, Is.EqualTo(versions[2]));
            Assert.That(Versioned.LowestVersion, Is.EqualTo(versions[1]));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void TestVersionBetweenLiveVersionsDoesNotChangeExpirationChain(int testRegistrationIndex)
        {
            var versions = new[] { new Version(3, 1, 0, 0), new Version(3, 1, 0, 2), new Version(3, 1, 0, 4) };
            var dates = new[] { new DateTime(2010, 1, 1), new DateTime(2011, 1, 1), new DateTime(2012, 1, 1) };
            var test = new Version(3, 1, 0, 3);
            for (var index = 0; index <= versions.Length; index++)
            {
                if (index == testRegistrationIndex)
                    Versioned.RegisterVersion(test, new DateTime(2011, 6, 1), ReleaseMode.Test);
                if (index < versions.Length)
                    Versioned.RegisterVersion(versions[index], dates[index], ReleaseMode.Live);
            }

            Assert.That(Versioned.GetVersion(test).DeleteDate, Is.EqualTo(DateTime.MaxValue));
            Assert.That(Versioned.GetVersion(versions[2]).DeleteDate, Is.EqualTo(DateTime.MaxValue));
            Assert.That(Versioned.GetVersion(versions[1]).DeleteDate, Is.EqualTo(DateTime.MaxValue));
            Assert.That(Versioned.GetVersion(versions[0]).DeleteDate, Is.EqualTo(dates[1].AddMonths(6)));
        }

        [Test]
        public void OlderLiveVersionsExpireSixMonthsAfterTheirNextLiveRelease()
        {
            var versions = new[] { new Version(3, 1, 0, 0), new Version(3, 1, 0, 1), new Version(3, 1, 0, 2), new Version(3, 1, 0, 3) };
            var dates = new[] { new DateTime(2010, 1, 1), new DateTime(2011, 1, 31), new DateTime(2012, 8, 31), new DateTime(2014, 1, 1) };
            for (var index = versions.Length - 1; index >= 0; index--)
                Versioned.RegisterVersion(versions[index], dates[index], ReleaseMode.Live);

            Assert.That(Versioned.GetVersion(versions[0]).DeleteDate, Is.EqualTo(dates[1].AddMonths(6)));
            Assert.That(Versioned.GetVersion(versions[1]).DeleteDate, Is.EqualTo(dates[2].AddMonths(6)));
            Assert.That(Versioned.GetVersion(versions[2]).DeleteDate, Is.EqualTo(DateTime.MaxValue));
            Assert.That(Versioned.GetVersion(versions[3]).DeleteDate, Is.EqualTo(DateTime.MaxValue));
        }

        [Test]
        public void SecondTestVersionIsRejectedWithoutChangingRegisteredVersions()
        {
            var live = new Version(3, 1, 0, 0);
            var test = new Version(3, 1, 0, 1);
            var duplicateTest = new Version(3, 1, 0, 2);
            Versioned.RegisterVersion(live, new DateTime(2010, 1, 1), ReleaseMode.Live);
            Versioned.RegisterVersion(test, new DateTime(2011, 1, 1), ReleaseMode.Test);

            var exception = Assert.Throws<ArgumentException>(() =>
                Versioned.RegisterVersion(duplicateTest, new DateTime(2012, 1, 1), ReleaseMode.Test));

            Assert.That(exception.ParamName, Is.EqualTo("mode"));
            Assert.That(Versioned.GetVersion(duplicateTest), Is.Null);
            Assert.That(Versioned.GetVersion(live).DeleteDate, Is.EqualTo(DateTime.MaxValue));
            Assert.That(Versioned.GetVersion(test).DeleteDate, Is.EqualTo(DateTime.MaxValue));
        }
    }
}
