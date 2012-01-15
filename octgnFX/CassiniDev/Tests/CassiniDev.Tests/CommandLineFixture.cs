using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace CassiniDev.Tests
{
    [TestFixture]
    public class CommandLineFixture
    {
        [Test]
        public void Empty()
        {
            CommandLineArguments args = new CommandLineArguments();
            string[] cmdLine = new string[]
            {
                //@"/path:c:\windows"
            };

            if (!CommandLineParser.ParseArgumentsWithUsage(cmdLine, args))
            {
                Assert.Fail();
            }

            
            Assert.AreEqual(false, args.AddHost);
            Assert.AreEqual(null, args.ApplicationPath);
            Assert.AreEqual(null, args.HostName);
            Assert.AreEqual(null, args.IPAddress);
            Assert.AreEqual(IPMode.Loopback, args.IPMode);
            Assert.AreEqual(false, args.IPv6);
            Assert.AreEqual(false, args.Nodirlist);
            Assert.AreEqual(false, args.Ntlm);
            Assert.AreEqual(0, args.Port);
            Assert.AreEqual(PortMode.FirstAvailable, args.PortMode);
            Assert.AreEqual(65535, args.PortRangeEnd);
            Assert.AreEqual(32768, args.PortRangeStart);
            Assert.AreEqual(RunMode.Server, args.RunMode);
            Assert.AreEqual(false, args.Silent);
            Assert.AreEqual(0, args.TimeOut);
            Assert.AreEqual("/", args.VirtualPath);
            Assert.AreEqual(0, args.WaitForPort);

            Assert.AreEqual("/v:\"/\"", args.ToString());
        }

        [Test]
        public void VisualStudioCmdLine()
        {
            CommandLineArguments args = new CommandLineArguments();
            string[] cmdLine = new string[]
            {
                @"/port:32768",
                "/path:c:\\temp",
                "/vpath:/myapp",
                @"/ntlm",
                @"/silent",
                @"/nodirlist"
            };

            if (!CommandLineParser.ParseArgumentsWithUsage(cmdLine, args))
            {
                Assert.Fail();
            }

            
            Assert.AreEqual(false, args.AddHost);
            Assert.AreEqual("c:\\temp", args.ApplicationPath);
            Assert.AreEqual(null, args.HostName);
            Assert.AreEqual(null, args.IPAddress);
            Assert.AreEqual(IPMode.Loopback, args.IPMode);
            Assert.AreEqual(false, args.IPv6);
            Assert.AreEqual(true, args.Nodirlist);
            Assert.AreEqual(true, args.Ntlm);
            Assert.AreEqual(32768, args.Port);
            Assert.AreEqual(PortMode.FirstAvailable, args.PortMode);
            Assert.AreEqual(65535, args.PortRangeEnd);
            Assert.AreEqual(32768, args.PortRangeStart);
            Assert.AreEqual(RunMode.Server, args.RunMode);
            Assert.AreEqual(true, args.Silent);
            Assert.AreEqual(0, args.TimeOut);
            Assert.AreEqual("/myapp", args.VirtualPath);
            Assert.AreEqual(0, args.WaitForPort);

            Assert.AreEqual("/a:\"c:\\temp\" /v:\"/myapp\" /p:32768 /ntlm /silent /nodirlist", args.ToString());
        }

        [Test]
        public void QuotedValuesInToString()
        {
            CommandLineArguments args = new CommandLineArguments();
            string[] cmdLine = new string[]
            {
                @"/port:32768",
                @"/path:c:\temp foo",
                @"/vpath:/myapp with spaces",
                @"/ntlm",
                @"/silent",
                @"/nodirlist"
            };

            if (!CommandLineParser.ParseArgumentsWithUsage(cmdLine, args))
            {
                Assert.Fail();
            }


            Assert.AreEqual(false, args.AddHost);
            Assert.AreEqual(@"c:\temp foo", args.ApplicationPath);
            Assert.AreEqual(null, args.HostName);
            Assert.AreEqual(null, args.IPAddress);
            Assert.AreEqual(IPMode.Loopback, args.IPMode);
            Assert.AreEqual(false, args.IPv6);
            Assert.AreEqual(true, args.Nodirlist);
            Assert.AreEqual(true, args.Ntlm);
            Assert.AreEqual(32768, args.Port);
            Assert.AreEqual(PortMode.FirstAvailable, args.PortMode);
            Assert.AreEqual(65535, args.PortRangeEnd);
            Assert.AreEqual(32768, args.PortRangeStart);
            Assert.AreEqual(RunMode.Server, args.RunMode);
            Assert.AreEqual(true, args.Silent);
            Assert.AreEqual(0, args.TimeOut);
            Assert.AreEqual("/myapp with spaces", args.VirtualPath);
            Assert.AreEqual(0, args.WaitForPort);

            Assert.AreEqual("/a:\"c:\\temp foo\" /v:\"/myapp with spaces\" /p:32768 /ntlm /silent /nodirlist", args.ToString());
            
        }











    }
}
