using Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace DotClient
{
    public class GameManager
    {
        private readonly ConcurrentDictionary<int, Character> _players = new ConcurrentDictionary<int, Character>();
        private static readonly object mutex = new object();
        private int ClientId { get; set; }

        public void InitPlayer(int clientId)
        {
            ClientId = clientId;
        }

        public void UpdateStates(List<Character> states)
        {
            lock (mutex)
            {
                foreach (var item in states)
                {
                    if (_players.TryGetValue(item.Id, out var player))
                    {
                        player.Position = item.Position;
                        player.Health = item.Health;
                    }
                    else
                    {
                        _players[item.Id] = item;
                    }
                }
            }
        }

        private void RemovePlayer(int clientId)
        {
            lock (mutex)
            {
                _players.TryRemove(clientId, out _);
            }
        }

        public void PlayerDisconnect(Character target)
        {
            RemovePlayer(target.Id);
        }

        public void Run()
        {
            while (AsyncSocketClient.IsAlive)
            {
                Thread.Sleep(1000);

                Packet packet = new Packet
                {
                    Command = Command.MOVE,
                    ClientPosition = new Vector2Int(0, 0)
                };

                AsyncSocketClient.Send(packet);

                Draw();
            }
            Console.WriteLine("Server unreachable. Exit");
        }

        public void ProcessHit(Packet packet)
        {
        }

        public void Draw()
        {
        }
    }
}