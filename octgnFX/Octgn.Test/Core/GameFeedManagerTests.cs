namespace Octgn.Test.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NUnit.Framework;

    using Octgn.Core;

    public class GameFeedManagerTests
    {
        [Test]
        public void GetPackages_NoDuplicates()
        {
            var ret = GameFeedManager.Get().GetPackages();
            var packList = new List<string>();
            foreach (var pack in ret)
            {
                if (packList.Any(x => x == pack.Id)) Assert.Fail("Found duplicate of a package");
                else packList.Add(pack.Id);
            }
            
            Console.WriteLine(ret.First().Version.Version);
        }
    }
}