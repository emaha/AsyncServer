using Common;
using System.Collections.Generic;

namespace DotServer
{
    public class ObjectManager
    {
        private List<Character> Characters = new List<Character>();
        private List<Item> Items = new List<Item>();

        public void Update()
        {
            foreach (var character in Characters)
            {
                character.Update();
            }
        }
    }
}