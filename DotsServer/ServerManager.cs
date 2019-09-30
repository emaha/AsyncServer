using System;
using Common;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace DotsServer
{
    public class ServerManager
    {
        private static ServerManager _instance;
        public static ServerManager Instance => _instance ?? (_instance = new ServerManager());
        private readonly ConcurrentDictionary<int, Character> _players = new ConcurrentDictionary<int, Character>();

        public void Loop()
        {
            while (true)
            {
                Thread.Sleep(2000);

                List<Character> list = new List<Character>();
                foreach (var item in _players)
                {
                    list.Add(item.Value);
                }

                Packet packet = new Packet()
                {
                    Command = Command.ALL_PLAYER_STATES,
                    PlayersState = list
                };
                if (list.Count > 0)
                {
                    AsyncSocketListener.SendToAll(packet);
                }
            }
        }

        public void HitTarget(int clientId, Packet packet)
        {
            var player = _players[clientId];
            var target = _players[packet.Target.Id];
            player.Hit(target);
            packet.Target.Health = target.Health;
            AsyncSocketListener.SendToAll(packet);
        }

        public void UpdatePlayer(int clientId, Packet packet)
        {
            var pos = packet.ClientPosition;
            _players[clientId].Position = new Vector2Int(pos.X, pos.Y);
        }

        public void CreatePlayer(int clientId)
        {
            var character = new Character(clientId);
            character.RecalculateStats();

            _players[clientId] = character;
        }
    }
}