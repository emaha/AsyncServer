using ProtoBuf;

namespace Common
{
    [ProtoContract]
    public class Vector2Int
    {
        public Vector2Int() { }

        public Vector2Int(int x, int y)
        {
            X = x;
            Y = y;
        }

        [ProtoMember(1)]
        public int X { get; set; }

        [ProtoMember(2)]
        public int Y { get; set; }

    }
}