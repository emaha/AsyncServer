using Common;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
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
                Command = Command.INIT,
                InitData = new PlayerState
                {
                    Id = state.ClientId,
                    Position = new Vector2Int(0, 0)
                }
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
                    MemoryStream ms = new MemoryStream(state.Buffer, 0, received);
                    Packet packet = Serializer.Deserialize<Packet>(ms);

                    switch (packet.Command)
                    {
                        case Command.PING:
                            Console.WriteLine($"PING");
                            break;

                        case Command.MESSAGE:
                            Console.WriteLine($"Message from client: {packet.Message}");
                            break;

                        case Command.MOVE:
                            _serverManager.UpdatePlayer(state.ClientId, packet);
                            break;

                        case Command.FIRE:
                            _serverManager.HitTarget(state.ClientId, packet);
                            break;

                        case Command.DISCONNECT:
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
                Command = Command.PLAYER_DISCONNECTED,
                Target = _serverManager.GetPlayer(state.ClientId)
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

        private static void Send(Socket socket, Packet data)
        {
            MemoryStream ms = new MemoryStream();
            Serializer.Serialize(ms, data);
            byte[] byteData = ms.ToArray();
            socket.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, socket);
        }

        public static void SendToAll(Packet data)
        {
            MemoryStream ms = new MemoryStream();
            Serializer.Serialize(ms, data);
            byte[] byteData = ms.ToArray();

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