using ProtoBuf;

namespace Common
{
    [ProtoContract]
    public class PlayerState
    {
        [ProtoMember(1)]
        public int Id { get; set; }

        [ProtoMember(2)]
        public Vector2Int Position { get; set; }
    }
}