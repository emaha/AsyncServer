using Common;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace DotsServer
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

        public static void StartListening()
        {
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
            ClientStates.Add(state);

            // Init player
            Packet packet = new Packet()
            {
                Command = Command.INIT,
                InitData = new PlayerState()
                {
                    Id = state.ClientId,
                    Position = new Vector2Int(0, 0)
                }
            };
            Send(state.WorkSocket, packet);
            
            ServerManager.Instance.CreatePlayer(state.ClientId);


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
                            ServerManager.Instance.UpdatePlayer(state.ClientId, packet);
                            break;
                        case Command.FIRE:
                            ServerManager.Instance.HitTarget(state.ClientId, packet);
                            break;
                    }
                }

                socket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveCallback, state);
            }
            catch (Exception)
            {
                Console.WriteLine("Disconnected...");
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
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
                //Console.WriteLine($"Sent {bytesSent} bytes to client.");
            }
            catch (Exception e)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
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

            for (int i = 0; i < ClientStates.Count; i++)
            {
                try
                {
                    var s = ClientStates[i].WorkSocket;
                    s.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, s);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    ClientStates.RemoveAt(i--);
                }
            }
        }
    }
}