namespace Client
{
    using System;
    using System.Threading;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Client");
            // Give the hub a chance to start up.
            Thread.Sleep(5000);
            var client = new TestClient("http://localhost:12345/signalr");
            client.Start().Wait();
            Console.WriteLine("Client Started");

            Console.WriteLine("Press the q key to quit.");

            var keepRunning = true;
            while (keepRunning)
            {
                if (Console.KeyAvailable) if (Console.ReadKey().Key == ConsoleKey.Q) keepRunning = false;
                Thread.Sleep(10);
            }
        }
    }
}
