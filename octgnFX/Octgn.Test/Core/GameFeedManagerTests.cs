namespace Octgn.Test.Core
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using Octgn.Core;

    public class GameFeedManagerTests
    {
        [NUnit.Framework.Test]
        public void GetPackages_GetsOneRepo()
        {
            var ret = GameFeedManager.Get().GetPackages();
            Assert.AreEqual(1, ret.Count());
            Console.WriteLine(ret.First().Version.Version);
        }
    }
}