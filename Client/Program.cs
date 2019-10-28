using System;
using System.Threading;

namespace DotClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Thread.Sleep(2000);
            var manager = new GameManager();
            AsyncSocketClient.StartClient(manager);

            manager.Run();

            Console.ReadLine();
        }
    }
}