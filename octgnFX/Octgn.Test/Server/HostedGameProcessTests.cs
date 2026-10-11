using System;
using System.IO.Pipes;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Octgn.Library;

namespace Octgn.Test.Server
{
    [TestFixture, NonParallelizable, Timeout(5000)]
    public class HostedGameProcessTests
    {
        private static Task<int> Read(NamedPipeServerStream pipe, int preferred)
        {
            return (Task<int>)typeof(HostedGameProcess)
                .GetMethod("ReadReadyPort", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, new object[] { pipe, preferred });
        }

        [TestCase(5000, 5000)]
        [TestCase(5000, 5009)]
        [TestCase(65535, 65535)]
        public async Task ReadyPipe_ReturnsSelectedPort(int preferred, int selected)
        {
            var name = Guid.NewGuid().ToString("N");
            using (var pipe = new NamedPipeServerStream(name, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
            {
                var read = Read(pipe, preferred);
                await HostedGameProcess.ReportReadyAsync(name, selected);
                Assert.AreEqual(selected, await read);
            }
        }

        [TestCase(0)]
        [TestCase(4999)]
        [TestCase(5010)]
        [TestCase(65536)]
        public async Task ReadyPipe_RejectsPortOutsideTheChildsWindow(int selected)
        {
            var name = Guid.NewGuid().ToString("N");
            using (var pipe = new NamedPipeServerStream(name, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
            {
                var read = Read(pipe, 5000);
                await HostedGameProcess.ReportReadyAsync(name, selected);
                Assert.ThrowsAsync<InvalidOperationException>(async () => await read);
            }
        }

        [Test]
        public async Task ReadyPipe_RejectsIncompleteReply()
        {
            var name = Guid.NewGuid().ToString("N");
            using (var pipe = new NamedPipeServerStream(name, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
            {
                var read = Read(pipe, 5000);
                using (var client = new NamedPipeClientStream(".", name, PipeDirection.Out, PipeOptions.Asynchronous))
                {
                    await client.ConnectAsync(1000);
                    await client.WriteAsync(new byte[2], 0, 2);
                }
                Assert.ThrowsAsync<InvalidOperationException>(async () => await read);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task ReadyPipe_DisposalEndsPendingWait(bool connected)
        {
            var name = Guid.NewGuid().ToString("N");
            using (var pipe = new NamedPipeServerStream(name, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
            using (var client = new NamedPipeClientStream(".", name, PipeDirection.Out, PipeOptions.Asynchronous))
            {
                var read = Read(pipe, 5000);
                if (connected) await client.ConnectAsync(1000);
                pipe.Dispose();
                Assert.AreSame(read, await Task.WhenAny(read, Task.Delay(1000)));
                Assert.CatchAsync(async () => await read);
            }
        }
    }
}
