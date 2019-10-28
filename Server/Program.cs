using System;

namespace DotServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ServerManager serverManager = new ServerManager();
            AsyncSocketListener.StartListening(serverManager);

            serverManager.Run();

            Console.WriteLine("\nPress ENTER to continue...");
            Console.ReadLine();
        }
    }
}