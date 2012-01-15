using System;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CassiniDev.MSTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod, DeploymentItem("test-page.htm")]
        public void TestMethod1()
        {
            var server = new CassiniDevServer();
            server.StartServer(Environment.CurrentDirectory);
            var url = server.NormalizeUrl("test-page.htm");
            WebClient client = new WebClient();
            var response = client.DownloadString(url);
            Assert.IsTrue(response.Contains("test page"));
            server.StopServer();
        }
    }
}
