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

        public static bool Combat(Player player, Enemy enemy)
        {
            Console.WriteLine($"Начинается бой: {enemy}");

            while (player.HP > 0 && enemy.HP > 0)
            {
                if (player.Frozen)
                {
                    Console.WriteLine("Вы заморожены и пропускаете ход!");
                    player.Frozen = false;
                }
                else
                {
                    Console.WriteLine($"Ваш HP: {player.HP}/{player.MaxHP}  Оружие: {player.Weapon}  Доспех: {player.Armor}");
                    Console.Write("Выберите действие (A - Атака, D - Защита): ");
                    string cmd = Console.ReadLine().Trim().ToLower();
                    player.Defending = false;

                    if (cmd == "d")
                    {
                        player.Defending = true;
                        Console.WriteLine("Вы встали в защиту: шанс уклониться 40% или блок уменьшает урон.");
                    }
                    else
                    {
                        int playerBase = 8;
                        int damage = Math.Max(1, playerBase + (player.Weapon?.Attack ?? 0) - enemy.Defense);

                        if (enemy.ReduceIncomingDamage)
                        {
                            damage = Math.Max(1, damage - enemy.DamageReduction);
                            Console.WriteLine($"Слизень уменьшает полученный урон на {enemy.DamageReduction} единиц!");
                        }

                        enemy.HP -= damage;
                        Console.WriteLine($"Вы атакуете и наносите {damage} урона (HP врага: {Math.Max(0, enemy.HP)})");
                    }
                }

                if (enemy.HP > 0)
                {
                    Enemy.ProcessEnemyAttack(player, enemy);
                }
            }

            if (player.HP <= 0)
            {
                Console.WriteLine("Вы погибли. Игра окончена.");
                return false;
            }

            Console.WriteLine("Враг повержен!");
            return true;
        }
    }
}