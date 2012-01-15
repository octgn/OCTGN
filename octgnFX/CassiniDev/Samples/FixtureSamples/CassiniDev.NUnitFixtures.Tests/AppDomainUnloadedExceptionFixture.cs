using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NUnit.Framework;

namespace CassiniDev.NUnitFixtures.Tests
{
    /// <summary>
    /// Some runners like NUnit register an appdomainunloaded exception being thrown out of cassinidev/webdev.webserver/cassini
    /// when shutting down the test that fails otherwise successful tests. The goal is to provide yet another advantage over the
    /// other implementations by eliminating this exception
    /// </summary>
    [TestFixture]
    public class AppDomainUnloadedExceptionFixture : CassiniDevNUnitFixture
    {
        // we are just going to run the base test repeatedly
        [Test]
        public void Test2()
        {

            Test();
            Test();
            Test();
        }

        [Test]
        public void Test3()
        {

            Test2();
            Test2();
            Test2();
        }
    }
}
