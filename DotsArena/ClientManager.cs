using Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace DotsArena
{
    public class ClientManager
    {
        private static ClientManager _instance;
        public static ClientManager Instance => _instance ?? (_instance = new ClientManager());
        private readonly ConcurrentDictionary<int, Character> _players = new ConcurrentDictionary<int, Character>();

        private int ClientId { get; set; }
        private int x, y;

        public void InitPlayer(int clientId)
        {
            ClientId = clientId;
        }

        public void Run()
        {
            while (AsyncSocketClient.IsAlive)
            {
                x++;
                y++;
                Thread.Sleep(1000);

                Packet packet = new Packet
                {
                    Command = Command.MOVE,
                    ClientPosition = new Vector2Int(x, y)
                };

                if (x % 10 == 0)
                {
                    TryToKillSomebody();
                }

                AsyncSocketClient.Send(packet);

                Draw();
            }
            Console.WriteLine("Server unreachable. Exit");
        }

        public void ProcessHit(Packet packet)
        {
            if (packet.Target.Id == ClientId)
            {
                _players[ClientId].Health = packet.Target.Health;
                Console.WriteLine($"I'm hitted. Health:{_players[ClientId].Health}");
            }
            else
            {
                Console.WriteLine($"Somebody hit the Player {packet.Target.Id}");
            }
        }

        public void TryToKillSomebody()
        {
            Character target = null;
            foreach (var item in _players)
            {
                if (item.Key != ClientId && item.Value.Health > 0)
                {
                    target = item.Value;
                    break;
                }
            }

            if (target != null)
            {
                Packet p = new Packet()
                {
                    Command = Command.FIRE,
                    Target = target
                };
                Console.WriteLine($"I'm hitting Player {target.Id}");
                AsyncSocketClient.Send(p);
            }
        }

        public void UpdateStates(List<Character> states)
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

        public void Draw()
        {
            
        }
    }
}