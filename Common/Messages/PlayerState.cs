using MessagePack;

namespace Common
{
    [MessagePackObject]
    public class PlayerInfo
    {
        [Key(1)]
        public int Id { get; set; }

        [Key(2)]
        public Vector2Int Position { get; set; }
    }
}