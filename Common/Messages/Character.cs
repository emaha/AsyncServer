using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    [MessagePackObject]
    public class Character
    {
        [Key(1)]
        public int Id { get; set; }

        [Key(2)]
        public int Health { get; set; }

        [Key(3)]
        public int Energy { get; set; }

        [Key(4)]
        public Vector2Int Position { get; set; }

        [IgnoreMember]
        public int Armor { get; set; }

        [IgnoreMember]
        public Stats BaseStats { get; set; }

        [IgnoreMember]
        public Stats AdditionalStats { get; set; }

        [IgnoreMember]
        public int Damage { get; set; }

        [IgnoreMember]
        public int CritRate { get; set; }

        [IgnoreMember]
        public int CarryWeight { get; set; }

        [IgnoreMember]
        public int Sight { get; set; }

        [IgnoreMember]
        public List<Item> Inventory;

        [IgnoreMember]
        public List<Item> EquippedItems;

        [IgnoreMember]
        public List<Effect> Effects;

        [IgnoreMember]
        public bool IsAlive => Health > 0;

        public Character()
        {
        }

        public Character(int clientId)
        {
            Id = clientId;
            BaseStats = new Stats()
            {
                Strength = 5,
                Perception = 5,
                Endurance = 5,
                Charisma = 5,
                Intelligence = 5,
                Agility = 5,
                Luck = 5
            };
            Inventory = new List<Item>();
            Effects = new List<Effect>();
            Health = BaseStats.Endurance * 25;
            Position = new Vector2Int(0, 0);
        }

        public void Update()
        {
        }

        public void TickEffects()
        {
            foreach (var effect in Effects)
            {
                effect.Tick(this);
            }
        }

        public void Hit(Character target)
        {
            var rand = new Random().Next(0, 100);
            int dmg;
            if (rand <= CritRate)
            {
                dmg = Damage * 2;
            }
            else
            {
                dmg = Damage;
            }

            target.TakeDmg(dmg);
        }

        public void TakeDmg(int dmg)
        {
            var res = dmg - Armor;
            if (res > 0)
            {
                Health -= res;
            }
        }

        public void Equip(Item item)
        {
            Inventory.Remove(item);

            var equippedItem = EquippedItems.First(x => x.Slot == item.Slot);
            if (equippedItem != null)
            {
                UnEquip(equippedItem);
            }
            EquippedItems.Add(item);

            RecalculateStats();
        }

        public void UnEquip(Item item)
        {
            EquippedItems.Remove(item);
            Inventory.Add(item);

            RecalculateStats();
        }

        public void RecalculateStats()
        {
            Damage = BaseStats.Strength * 5;
            Armor = BaseStats.Endurance * 2;
            CritRate = (int)Math.Pow(BaseStats.Luck, 2) / 4;
            CarryWeight = BaseStats.Strength * 20;
            Sight = BaseStats.Perception * 2;

            foreach (var item in Inventory)
            {
                Armor += item.Armor;
                Damage += item.Dmg;
            }
        }
    }
}