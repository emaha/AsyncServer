using Common;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace DotServer
{
    public class StateObject
    {
        public int ClientId;
        public Socket WorkSocket;
        public byte[] Buffer = new byte[1024];
    }

    public static class AsyncSocketListener
    {
        private static readonly Socket Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly List<StateObject> ClientStates = new List<StateObject>();
        private static int _clientCounter;

        private static readonly object mutex = new object();
        private static ServerManager _serverManager;

        public static void StartListening(ServerManager sm)
        {
            _serverManager = sm;
            try
            {
                Listener.Bind(new IPEndPoint(IPAddress.Any, 11000));
                Listener.Listen(0);

                Console.WriteLine("Waiting for a connection...");

                Listener.BeginAccept(AcceptCallback, Listener);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;
            Socket handler = socket.EndAccept(ar);

            StateObject state = new StateObject
            {
                ClientId = _clientCounter++,
                WorkSocket = handler
            };
            lock (mutex)
            {
                ClientStates.Add(state);
            }

            // Init player
            Packet packet = new Packet
            {
                Type = MessageType.INIT,
                Position = new Vector2Int(0, 0)
            };
            Send(state.WorkSocket, packet);

            _serverManager.CreatePlayer(state.ClientId);

            Console.WriteLine("Accepted from {0}", handler.RemoteEndPoint);

            handler.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveCallback, state);
            Listener.BeginAccept(AcceptCallback, socket);
        }

        public static void ReceiveCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket socket = state.WorkSocket;

            try
            {
                int received = socket.EndReceive(ar);

                if (received > 0)
                {
                    Packet packet = MessagePackSerializer.Deserialize<Packet>(state.Buffer);
                    

                    //byte[] data = new byte[received - MessageConfig.MESSAGE_LEN];
                    //Array.Copy(state.Buffer, 1, data, 0, data.Length);

                    switch (packet.Type)
                    {
                        case MessageType.PING:
                            Console.WriteLine($"PING");
                            break;

                        case MessageType.MESSAGE:
                            Console.WriteLine($"Message from client: ");
                            break;

                        case MessageType.MOVE:
                            var data = packet.Position;
                            _serverManager.UpdatePlayer(state.ClientId, data);
                            break;

                        case MessageType.FIRE:
                            var hitData = new byte[0];
                            _serverManager.HitTarget(state.ClientId, hitData);
                            break;

                        case MessageType.DISCONNECT:
                            DisconnectClient(state, socket);
                            break;
                    }
                }

                socket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveCallback, state);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Disconnected...{ex.Message}");
                DisconnectClient(state, socket);
            }
        }

        private static void DisconnectClient(StateObject state, Socket socket)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            Packet packet = new Packet
            {
                Type = MessageType.PLAYER_DISCONNECTED,
                Character = _serverManager.GetPlayer(state.ClientId)
            };
            SendToAll(packet);

            lock (mutex)
            {
                Console.WriteLine($"Remove {state.ClientId}");
                _serverManager.RemovePlayer(state.ClientId);
                ClientStates.Remove(state);
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            Socket socket = null;
            try
            {
                socket = (Socket)ar.AsyncState;
                int bytesSent = socket.EndSend(ar);
            }
            catch (Exception e)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                Console.WriteLine("Sendcallback error");
                Console.WriteLine(e.ToString());
            }
        }

        private static void Send(Socket socket, Packet packet)
        {
            byte[] byteData = MessagePackSerializer.Serialize(packet);
            socket.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, socket);
        }

        public static void SendToAll(Packet packet)
        {
            byte[] byteData = MessagePackSerializer.Serialize(packet);

            lock (mutex)
            {
                foreach (var client in ClientStates)
                {
                    try
                    {
                        var s = client.WorkSocket;
                        s.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, s);
                    }
                    catch (Exception e)
                    {
                        //Console.WriteLine(e.Message);
                    }
                }
            }
        }
    }
}