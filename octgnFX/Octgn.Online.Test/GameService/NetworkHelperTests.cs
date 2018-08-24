using NUnit.Framework;
using Octgn.Online.GameService;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Octgn.Online.Test.GameService
{
    [TestFixture]
    public class NetworkHelperTests
    {
        [Test]
        public void Increments() {
            var nh = new NetworkHelper(32000, 32002, "NetworkHelperTest2");

            var port1 = nh.NextPort;

            var expected = port1 + 1;
            if (expected >= 32002)
                expected = 32000;

            nh = new NetworkHelper(32000, 32002, "NetworkHelperTest2");

            var port2 = nh.NextPort;

            Assert.AreEqual(expected, port2);

            expected = port2 + 1;
            if (expected >= 32002)
                expected = 32000;

            nh = new NetworkHelper(32000, 32002, "NetworkHelperTest2");

            var port3 = nh.NextPort;

            Assert.AreEqual(expected, port3);
        }

        [Test]
        public void NoConflicts() {
            var startEvent = new ManualResetEvent(false);

            var allPorts = new ConcurrentBag<int>();

            void runSingleThread() {
                // Wait for all the threads to be started before beginning work
                startEvent.WaitOne();

                var nh = new NetworkHelper(21000, 31000, "NetworkHelperTest");
                var ports = new List<int>();
                for(var i = 0; i < 1000; i++) {
                    var port = nh.NextPort;

                    ports.Add(port);
                }

                // Update the shared list of ports.
                // We do this afterwards on purpose. This makes the test more valid.
                foreach (var port in ports)
                    allPorts.Add(port);
            }

            // Create all the threads
            var threads = new List<Thread>();
            for(var i = 0; i < 10; i++) {
                var thread = new Thread(runSingleThread);
                threads.Add(thread);
            }

            // Start all the threads
            threads.ForEach(t => t.Start());

            // Signal all the threads to work at the same time
            startEvent.Set();

            //wait until all the threads to finish
            threads.ForEach(t => t.Join());

            // Check results
            var uniqueNumbers = new HashSet<int>();
            foreach (var port in allPorts) {
                if (uniqueNumbers.Contains(port)) {
                    Assert.Fail("Duplicate port detected");
                    return;
                }
                uniqueNumbers.Add(port);
            }
        }
    }
}
