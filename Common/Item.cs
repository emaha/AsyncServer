namespace Common
{
    public enum Slot { HEAD, BODY, LEGS, WEAPON }

    public abstract class Item
    {
        protected Item()
        {
            Stats = new Stats();
        }

        public Slot Slot { get; set; }
        public int Armor { get; set; }
        public int Dmg { get; set; }
        public Stats Stats { get; set; }
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
            Slot = Slot.WEAPON;
            Dmg = dmg;
        }
    }
}