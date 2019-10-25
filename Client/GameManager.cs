using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace DotClient
{
    public class GameManager
    {
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
                lock (mutex)
                {
                    _players[ClientId].Health = packet.Target.Health;

                    Console.WriteLine($"Received. Amount:{_players[ClientId].Health}");
                }
            }
            else
            {
                Console.WriteLine($"Somebody send text to Client {packet.Target.Id}");
            }
        }

        public void TryToKillSomebody()
        {
            Character target = null;
            lock (mutex)
            {
                foreach (var item in _players)
                {
                    if (item.Key == ClientId || item.Value.Health <= 0) continue;
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
                Console.WriteLine($"send to client {target.Id}");
                AsyncSocketClient.Send(p);
            }
        }

        public void Draw()
        {
        }
    }
}