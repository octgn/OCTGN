using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NUnit.Framework;

namespace CassiniDev.NUnitFixtures.Tests
{

    /// <summary>
    /// http://cassinidev.codeplex.com/workitem/14359
    /// </summary>
    [TestFixture]
    public class CacheControlFixture : CassiniDevServer
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            const string applicationPath = @"..\..\..\..\CassiniDev.TestSite";
            StartServer(applicationPath);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            StopServer();
        }

        /// <summary>
        /// I am not sure whether I would consider lack of cache headers in a dev server an 'issue'
        /// when I am writing code I certainly don't want to be debugging against a cached file.
        /// But that is just my initial impression. Certainly a full modified-since implemenation would be 
        /// nice but I don't think that the infrastructure for that plumbing is worth implementing.
        /// </summary>
        [Test]
        public void Test()
        {

            // test is null and void 

            Server.RequestComplete += new EventHandler<RequestEventArgs>(Server_RequestComplete);
            string url = NormalizeUrl("static.txt");
            var client = new WebClient();
            var content = client.DownloadString(url);

        }

        void Server_RequestComplete(object sender, RequestEventArgs e)
        {
            
        }
    }
}
