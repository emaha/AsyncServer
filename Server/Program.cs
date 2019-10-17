﻿using System;

namespace DotServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            AsyncSocketListener.StartListening();

            ServerManager.Instance.Loop();

            Console.WriteLine("\nPress ENTER to continue...");
            Console.ReadLine();
        }
    }
}