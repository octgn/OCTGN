using System;
using System.Threading;
using System.Threading.Tasks;

namespace Octgn.LoadTester
{
    class Program
    {
        static async Task Main(string[] args) {
            using (var cts = new CancellationTokenSource()) {
                try {
                    Console.CancelKeyPress += (_, __) => {
                        try {
                            cts.Cancel();
                        } catch (ObjectDisposedException) { }
                    };

                    var tester = new Tester();

                    await tester.Run(cts.Token);
                } catch (Exception ex) {
                    Console.Error.WriteLine(ex);
                }
            }

            Console.WriteLine();
            Console.WriteLine("== DONE ==");
        }
    }
}
