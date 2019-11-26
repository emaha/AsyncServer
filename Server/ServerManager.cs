using System;
using Common;
using SFML.System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DotServer
{
    public class ServerManager
    {
        private readonly ConcurrentDictionary<int, Character> _players = new ConcurrentDictionary<int, Character>();

        private readonly GameManager _gameManager = new GameManager();

        private bool _isRunning = true;
        private Time _accum = Time.Zero;
        private readonly Clock _clock = new Clock();
        private readonly Time _ups = Time.FromSeconds(1.0f / 60.0f);

        private int _sendAllCounter = 0;

        /// <summary>
        /// Основной цикл
        /// </summary>
        public void Run()
        {
            while (_isRunning)
            {
                while (_accum >= _ups)
                {
                    UpdateLogic();

                    // SendDataToAll();

                    _accum -= _ups;
                }

                _accum += _clock.Restart();
            }
        }

        private void SendDataToAll()
        {
            // 3 раза в секунду
            if (_sendAllCounter++ != 20) return;

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

            Console.WriteLine("Send");
            _sendAllCounter = 0;
        }

        private void UpdateLogic()
        {
            _gameManager.Update();
        }

        public void Stop()
        {
            _isRunning = false;
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