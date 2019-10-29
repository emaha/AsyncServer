using Common;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using SFML.System;

namespace DotServer
{
    public class ServerManager
    {
        private readonly ConcurrentDictionary<int, Character> _players = new ConcurrentDictionary<int, Character>();

        private bool isRunning = true;
        private Time accum = Time.Zero;
        private Clock clock = new Clock();
        private Time ups = Time.FromSeconds(1.0f / 60.0f);

        public void Run()
        {
            while (isRunning)
            {
                while (accum > ups)
                {
                    accum -= ups;

                    // UpdateLogic();
                }

                accum += clock.Restart();

                //Thread.Sleep(2000);

                //List<Character> list = new List<Character>();
                //foreach (var item in _players)
                //{
                //    list.Add(item.Value);
                //}

                //Packet packet = new Packet()
                //{
                //    Command = Command.ALL_PLAYER_STATES,
                //    PlayersState = list
                //};
                //if (list.Count > 0)
                //{
                //    AsyncSocketListener.SendToAll(packet);
                //}
            }
        }

        public void Stop()
        {
            isRunning = false;
        }

        public void HitTarget(int clientId, Packet packet)
        {
            var player = _players[clientId];
            var target = _players[packet.Target.Id];
            player.Hit(target);
            packet.Target.Health = target.Health;
            AsyncSocketListener.SendToAll(packet);
        }

        public Character GetPlayer(int clientId)
        {
            _players.TryGetValue(clientId, out var character);
            return character;
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

        public void RemovePlayer(int clientId)
        {
            _players.TryRemove(clientId, out _);
        }
    }
}