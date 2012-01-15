using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NUnit.Framework;

namespace CassiniDev.NUnitFixtures.Tests
{
    /// <summary>
    /// Note: this fixture leverages the in-process CassiniDev server. If you have restrictions or experience
    /// issues with the in-process server, simply reference CassiniDev-console and use CassiniDevServerOP. 
    /// API and usage is same.
    /// </summary>
    [TestFixture]
    public class CassiniDevNUnitFixture : CassiniDevServer
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            // just need to back out of the bin dir
            const string applicationPath = @"..\";

            // Will start specified application as "localhost" on loopback and first available port in the range 8000-10000 with vpath "/"
            StartServer(applicationPath);

            // if you would like to exercise more control simply use any of the available overloads
        }

        //// this is left over from out-of-process fixture. I really don't think it is
        //// necessary in-proc, but a zombie thread might cause trouble. salt to taste
        //[TestFixtureTearDown]
        //public void TestFixtureTearDown()
        //{
        //    StopServer();
        //}

        [Test]
        public void Test()
        {

            // normalize URL provides a fully qualified URL rooted on the currently running server's hostname and port
            string url = NormalizeUrl("Default.aspx");

            WebClient wc = new WebClient();
            string actual = wc.DownloadString(url);

            Assert.IsTrue(actual.Contains("this is default.aspx"));

            // for a web testing utility library that simplifies endpoint testing see the Salient.Web namespace in http://salient.codeplex.com
        }
    }

}
