using Model;
using System;

namespace Model
{
    public class Enemy
    {
        public string Name;
        public EnemyRace Race;
        public int HP;
        public int Attack;
        public int Defense;
        public double CritChance = 0.0;
        public double FreezeChance = 0.0;
        public bool IgnorePlayerDefense = false;
        public bool ReduceIncomingDamage = false; 
        public int DamageReduction = 0; 

        public override string ToString()
        {
            return $"{Name} ({Race}) HP:{HP} ATK:{Attack} DEF:{Defense}";
        }
    }
}