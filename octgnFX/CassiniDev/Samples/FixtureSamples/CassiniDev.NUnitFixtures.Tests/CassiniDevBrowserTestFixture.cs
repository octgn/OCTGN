using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using CassiniDev.ServerLog;
using Microsoft.Win32;
using NUnit.Framework;

namespace CassiniDev.NUnitFixtures.Tests
{
    [TestFixture]
    public class CassiniDevBrowserTestFixture
    {


        /// <summary>
        /// Want to let a javascript unit test page post it's results to a non-existent handler
        /// so we can catch the post data in RequestComplete
        /// </summary>
        [Test]
        public void TestIE()
        {


            const string applicationPath = @"..\..\..\..\CassiniDev.TestSite";
            var test = new CassiniDevBrowserTest();
            test.StartServer(Path.GetFullPath(applicationPath));

            var uri = test.NormalizeUrl("qunit-callback.htm");

            RequestEventArgs result = test.RunTest(uri, WebBrowser.InternetExplorer);
            var body = Encoding.UTF8.GetString(result.RequestLog.Body);

            test.StopServer();

        }
       

        /// <summary>
        /// Want to let a javascript unit test page post it's results to a non-existent handler
        /// so we can catch the post data in RequestComplete
        /// </summary>
        [Test]
        public void TestFF()
        {


            const string applicationPath = @"..\..\..\..\CassiniDev.TestSite";
            var test = new CassiniDevBrowserTest();
            test.StartServer(Path.GetFullPath(applicationPath));

            var uri = test.NormalizeUrl("qunit-callback.htm");

            RequestEventArgs result = test.RunTest(uri, WebBrowser.Firefox);
            var body = Encoding.UTF8.GetString(result.RequestLog.Body);

            test.StopServer();

        }

        /// <summary>
        /// Want to let a javascript unit test page post it's results to a non-existent handler
        /// so we can catch the post data in RequestComplete
        /// </summary>
        [Test]
        public void TestChrome()
        {


            const string applicationPath = @"..\..\..\..\CassiniDev.TestSite";
            var test = new CassiniDevBrowserTest();
            test.StartServer(Path.GetFullPath(applicationPath));

            var uri = test.NormalizeUrl("qunit-callback.htm");

            RequestEventArgs result = test.RunTest(uri, WebBrowser.Chrome);
            var body = Encoding.UTF8.GetString(result.RequestLog.Body);

            test.StopServer();

        }
    }


}
