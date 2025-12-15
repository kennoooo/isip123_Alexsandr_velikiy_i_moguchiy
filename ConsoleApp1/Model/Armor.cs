using Model;
using System;

namespace Model
{
    public class Armor
    {
        public string Name { get; set; }
        public int Defense { get; set; }

        public override string ToString()
        {
            return $"{Name} (+{Defense} DEF)";
        }
    }
}