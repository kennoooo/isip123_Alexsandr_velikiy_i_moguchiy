using Model;
using System;

namespace Model
{
    public class Weapon
    {
        public string Name;
        public int Attack;

        public override string ToString()
        {
            return $"{Name} (+{Attack} ATK)";
        }
    }
}