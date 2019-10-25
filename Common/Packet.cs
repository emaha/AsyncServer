using ProtoBuf;
using System.Collections.Generic;

namespace Common
{
    [ProtoContract]
    public class Packet
    {
        /// <summary>
        /// Команда
        /// </summary>
        [ProtoMember(1)]
        public Command Command { get; set; }

        // Класс упаковывается в массив байт
        // Свойства сериализуются если != null

        #region Полезная нагрузка пакета (Payload)

        /// <summary>
        /// Данные о клиенте. Клиент -> Сервер
        /// </summary>
        [ProtoMember(2)]
        public Vector2Int ClientPosition { get; set; }

        /// <summary>
        /// Данные всех клиентов. Сервер -> Клиент
        /// </summary>
        [ProtoMember(3)]
        public List<Character> PlayersState { get; set; }

        /// <summary>
        /// Данные инициализации клиента. Сервер -> Клиент
        /// </summary>
        [ProtoMember(4)]
        public PlayerState InitData { get; set; }

        /// <summary>
        /// Текстовое сообщение. Оба направления
        /// </summary>
        [ProtoMember(5)]
        public string Message { get; set; }

        /// <summary>
        /// Цель любых действий
        /// </summary>
        [ProtoMember(6)]
        public Character Target { get; set; }

        #endregion Полезная нагрузка пакета (Payload)
    }
}