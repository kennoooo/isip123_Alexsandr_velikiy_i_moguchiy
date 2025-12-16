using Model;
using System;

namespace Model
{
    public class Player
    {
        public int MaxHP = 100;
        public int HP = 100;
        public Weapon Weapon = new Weapon { Name = "Кулаки", Attack = 3 };
        public Armor Armor = new Armor { Name = "Обычная одежда", Defense = 3 };
        public bool Frozen = false;
        public bool Defending = false;

        public void HealFull()
        {
            HP = MaxHP;
        }
    }
}