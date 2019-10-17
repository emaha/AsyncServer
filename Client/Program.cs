using System;
using System.Threading;

namespace DotClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Thread.Sleep(2000);
            AsyncSocketClient.StartClient();

            ClientManager.Instance.Run();

            Console.ReadLine();
        }
    }
}