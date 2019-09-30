namespace Common
{
    public abstract class Item
    {
        public int Armor { get; set; }
        public int Dmg { get; set; }
    }

    public class Gear : Item
    {
        public Gear(int armor = 1)
        {
            Armor = armor;
        }
    }

    public class Weapon : Item
    {
        public Weapon(int dmg = 1)
        {
            Dmg = dmg;
        }
    }
}