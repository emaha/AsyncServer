using Common;
using ProtoBuf;
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

                if (received > 0)
                {
                    MemoryStream ms = new MemoryStream(buffer, 0, received);
                    Packet packet = Serializer.Deserialize<Packet>(ms);

                    switch (packet.Command)
                    {
                        case Command.PING:
                            Send(packet);
                            Console.WriteLine("ping");
                            break;

                        case Command.INIT:
                            _gameManager.InitPlayer(packet.InitData.Id);
                            Console.WriteLine($"init: my id = {packet.InitData.Id}");
                            break;

                        case Command.MESSAGE:
                            string content = packet.Message;
                            Console.WriteLine($"Message from server: {content}");
                            break;

                        case Command.FIRE:
                            _gameManager.ProcessHit(packet);
                            break;

                        case Command.ALL_PLAYER_STATES:
                            _gameManager.UpdateStates(packet.PlayersState);
                            break;

                        case Command.PLAYER_DISCONNECTED:
                            _gameManager.PlayerDisconnect(packet.Target);
                            Console.WriteLine($"Dsc: {packet.Target.Id}");
                            break;
                    }

                    socket.BeginReceive(buffer, 0, buffer.Length, 0, ReceiveCallback, socket);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("Disconnected2...");
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                IsAlive = false;
                throw new Exception("SocketException", e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void Send(Packet data)
        {
            MemoryStream ms = new MemoryStream();
            Serializer.Serialize(ms, data);
            byte[] byteData = ms.ToArray();

            if (IsAlive)
            {
                client.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, client);
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