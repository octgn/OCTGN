using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace CassiniDev.Tests
{
    [TestFixture]
    public class IssueResolutionFixture
    {
        [Test]
        public void ServerCanBeRestarted()
        {
            var server = new CassiniDev.Server(Environment.CurrentDirectory);
            server .Start();
            new AutoResetEvent(false).WaitOne(1000);
            server.ShutDown();
            new AutoResetEvent(false).WaitOne(1000);
            server.Start();
            new AutoResetEvent(false).WaitOne(1000);
            server.ShutDown();
            
        }
    }
}
