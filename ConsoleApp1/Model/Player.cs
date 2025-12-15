using Model;
using System;

namespace Model
{
    public class Player
    {
        public int MaxHP { get; set; } = 100;
        public int HP { get; set; } = 100;
        public Weapon Weapon { get; set; } = new Weapon { Name = "Кулаки", Attack = 3 };
        public Armor Armor { get; set; } = new Armor { Name = "Обычная одежда", Defense = 3 };
        public bool Frozen { get; set; } = false;
        public bool Defending { get; set; } = false;

        public void HealFull()
        {
            HP = MaxHP;
        }
    }
}