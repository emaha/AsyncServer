using MessagePack;

namespace Common
{
    [MessagePackObject]
    public class Vector2Int
    {
        public Vector2Int() { }

        public Vector2Int(int x, int y)
        {
            X = x;
            Y = y;
        }

        [Key(1)]
        public int X { get; set; }

        [Key(2)]
        public int Y { get; set; }

    }
}