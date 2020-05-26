using MessagePack;
using System.Collections.Generic;

namespace Common
{
    [MessagePackObject]
    public class Packet
    {
        /// <summary>
        /// Тип сообщения
        /// </summary>
        [Key(1)]
        public MessageType Type { get; set; }

        /// <summary>
        /// Нагрузка
        /// </summary>
        [Key(2)]
        public Vector2Int Position { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Key(3)]
        public Character Character { get; set; }

        /// <summary>
        /// /
        /// </summary>
        [Key(4)]
        public List<Character> Characters { get; set; }
    }
}