using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Common
{
    [ProtoContract]
    public class Character
    {
        [ProtoMember(1)]
        public int Id { get; set; }

        [ProtoMember(2)]
        public int Health { get; set; }
        [ProtoMember(3)]
        public int Energy { get; set; }

        [ProtoMember(4)]
        public Vector2Int Position { get; set; }

        [ProtoIgnore]
        public int Armor { get; set; }
        [ProtoIgnore]
        public Stats BaseStats { get; set; }
        [ProtoIgnore]
        public Stats AdditionalStats { get; set; }
        [ProtoIgnore]
        public int Damage { get; set; }
        [ProtoIgnore]
        public int CritRate { get; set; }
        [ProtoIgnore]
        public int CarryWeight { get; set; }
        [ProtoIgnore]
        public int Sight { get; set; }
        [ProtoIgnore]
        public List<Item> Inventory;
        [ProtoIgnore]
        public List<Effect> Effects;

        public Character() { }

        public Character(int clientId)
        {
            Id = clientId;
            BaseStats = new Stats()
            {
                Strength = 5,
                Perception = 5,
                Endurance = 5,
                Agility = 5,
                Charisma = 5,
                Intelligence = 5,
                Luck = 5
            };
            Inventory = new List<Item>();
            Effects = new List<Effect>();
            Health = BaseStats.Endurance * 25;
            Position = new Vector2Int(0,0);
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
                Console.WriteLine("Crit!!!");
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
            else
            {
                res = 0;
            }
            Console.WriteLine($"Player {Id} hitted, {res}dmg, hp:{Health}");
            if (Health <= 0) Console.WriteLine($"i'm dead");
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