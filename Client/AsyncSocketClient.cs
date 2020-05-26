using Common;
using MessagePack;
using System;
using System.IO;
using System.Net.Sockets;

namespace DotClient
{
    public static class AsyncSocketClient
    {
        //private static int ClientId;
        private const int Port = 11000;

        private static Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static byte[] buffer = new byte[1024]; // размера буфера в 1024 байта хватит примерно на 50-60 клиентов
        public static bool IsAlive { get; private set; }

        private static GameManager _gameManager;

        private static void Connect()
        {
            try
            {
                client.BeginConnect("127.0.0.1", Port, ConnectCallback, client);
                //client.BeginConnect("192.168.0.192", Port, ConnectCallback, client);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            IsAlive = true;
            Console.WriteLine("Connected");
        }

        public static void StartClient(GameManager gm)

        {
            _gameManager = gm;
            Connect();
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket socket = (Socket)ar.AsyncState;
                socket.EndConnect(ar);
                Console.WriteLine($"Socket connected to {socket.RemoteEndPoint}");
                socket.BeginReceive(buffer, 0, buffer.Length, 0, ReceiveCallback, socket);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;

            try
            {
                int received = socket.EndReceive(ar);
                if (received == 0) return;

                Packet packet = MessagePackSerializer.Deserialize<Packet>(buffer);

                Invoke(packet);

                socket.BeginReceive(buffer, 0, buffer.Length, 0, ReceiveCallback, socket);
            }
            catch (SocketException e)
            {
                Console.WriteLine("Disconnected2...");
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                IsAlive = false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                IsAlive = false;
            }
        }

        private static void Invoke(Packet packet)
        {
            switch (packet.Type)
            {
                case MessageType.PING:
                    //Send(packet);
                    Console.WriteLine("ping");
                    break;

                case MessageType.INIT:
                    //_gameManager.InitPlayer(packet.InitData.Id);
                    Console.WriteLine($"init: my id = ");
                    break;

                case MessageType.MESSAGE:
                    //string content = packet.Message;
                    //Console.WriteLine($"Message from server: {content}");
                    break;

                case MessageType.FIRE:
                    //_gameManager.ProcessHit(packet);
                    break;

                case MessageType.ALL_PLAYER_STATES:
                    //_gameManager.UpdateStates(packet.PlayersState);
                    Console.WriteLine($"Received  bytes");
                    break;

                case MessageType.PLAYER_DISCONNECTED:
                    //_gameManager.PlayerDisconnect(packet.Target);
                    //Console.WriteLine($"Dsc: {packet.Target.Id}");
                    break;
            }
        }

        public static void Send(Packet packet)
        {
            byte[] data = MessagePackSerializer.Serialize(packet);

            if (IsAlive)
            {
                client.BeginSend(data, 0, data.Length, 0, SendCallback, client);
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                int bytesSent = client.EndSend(ar);
                //Console.WriteLine($"Sent {bytesSent} bytes to server.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}