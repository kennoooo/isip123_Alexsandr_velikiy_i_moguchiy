using Model;
using System;

namespace Model
{
    public static class CombatSystem
    {


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
                    ProcessEnemyAttack(player, enemy);
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

        private static void ProcessEnemyAttack(Player player, Enemy enemy)
        {
            int incoming = enemy.Attack;
            int playerArmorValue = enemy.IgnorePlayerDefense ? 0 : (player.Armor?.Defense ?? 0);

            if (player.Defending)
            {
                // Используем RandomChoice вместо Random
                if (RandomChoice.Chance(0.40))
                {
                    Console.WriteLine("Вы успешно уклонились от атаки!");
                }
                else
                {
                    double factor = 0.7 + RandomChoice.NextDouble() * 0.3;
                    double reduction = playerArmorValue * factor;
                    int damage = Math.Max(0, (int)Math.Round(incoming - reduction));
                    ApplySpecialEffects(player, enemy, damage);
                }
            }
            else
            {
                int damage = Math.Max(0, incoming - playerArmorValue);
                ApplySpecialEffects(player, enemy, damage);
            }
        }

        private static void ApplySpecialEffects(Player player, Enemy enemy, int baseDamage)
        {
            bool wasCrit = RandomChoice.Chance(enemy.CritChance);
            int finalDamage = baseDamage;

            if (wasCrit)
            {
                finalDamage *= 2;
                Console.WriteLine("Критический удар от врага!");
            }

            if (enemy.FreezeChance > 0 && RandomChoice.Chance(enemy.FreezeChance))
            {
                player.Frozen = true;
                Console.WriteLine("Враг наложил заморозку — вы пропустите следующий боевой ход!");
            }

            player.HP -= finalDamage;
            Console.WriteLine($"Враг атакует и вы получаете {finalDamage} урона (HP: {Math.Max(0, player.HP)})");
        }
    }
}