using Model;
using System;

namespace Model
{
    public class Armor
    {
        public string Name;
        public int Defense;

        public override string ToString()
        {
            return $"{Name} (+{Defense} DEF)";
        }
    }
}