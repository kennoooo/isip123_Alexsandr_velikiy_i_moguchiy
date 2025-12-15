using Model;
using System;

namespace Model
{
    public class Enemy
    {
        public string Name { get; set; }
        public EnemyRace Race { get; set; }
        public int HP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public double CritChance { get; set; } = 0.0;
        public double FreezeChance { get; set; } = 0.0;
        public bool IgnorePlayerDefense { get; set; } = false;
        public bool ReduceIncomingDamage { get; set; } = false; 
        public int DamageReduction { get; set; } = 0; 

        public override string ToString()
        {
            return $"{Name} ({Race}) HP:{HP} ATK:{Attack} DEF:{Defense}";
        }
    }
}