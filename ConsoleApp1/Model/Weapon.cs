using Model;
using System;

namespace Model
{
    public class Weapon
    {
        public string Name { get; set; }
        public int Attack { get; set; }

        public override string ToString()
        {
            return $"{Name} (+{Attack} ATK)";
        }
    }
}