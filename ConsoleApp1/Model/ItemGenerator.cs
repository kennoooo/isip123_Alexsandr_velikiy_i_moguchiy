using Model;
using System;

namespace Model
{
    public static class ItemGenerator
    {

        public static object GenerateRandomItem()
        {
            int t = RandomChoice.Next(3);

            if (t == 0)
                return "potion";

            if (t == 1)
                return new Weapon
                {
                    Name = $"Меч_{RandomChoice.Next(1, 1000)}",
                    Attack = RandomChoice.Next(2, 8)
                };
            else
                return new Armor
                {
                    Name = $"Доспех_{RandomChoice.Next(1, 1000)}",
                    Defense = RandomChoice.Next(1, 6)
                };
        }

        public static void ProcessItem(object item, Player player)
        {
            if (item is string s && s == "potion")
            {
                player.HealFull();
                Console.WriteLine("В сундуке — лечебное зелье. Вы полностью исцелены!");
            }
            else if (item is Weapon weapon)
            {
                Console.WriteLine($"В сундуке — новое оружие: {weapon}");
                Console.WriteLine($"Текущее оружие: {player.Weapon}");
                Console.Write("Взять новое оружие? (y/n): ");
                string ans = Console.ReadLine().Trim().ToLower();

                if (ans == "y")
                {
                    player.Weapon = weapon;
                    Console.WriteLine("Оружие заменено.");
                }
                else
                {
                    Console.WriteLine("Оружие выброшено.");
                }
            }
            else if (item is Armor armor)
            {
                Console.WriteLine($"В сундуке — новый доспех: {armor}");
                Console.WriteLine($"Текущий доспех: {player.Armor}");
                Console.Write("Взять новый доспех? (y/n): ");
                string ans = Console.ReadLine().Trim().ToLower();

                if (ans == "y")
                {
                    player.Armor = armor;
                    Console.WriteLine("Доспех заменён.");
                }
                else
                {
                    Console.WriteLine("Доспех выброшен.");
                }
            }
        }
    }
}