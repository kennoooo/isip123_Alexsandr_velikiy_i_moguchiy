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

        public static void ProcessEnemyAttack(Player player, Enemy enemy)
        {
            int incoming = enemy.Attack;
            int playerArmorValue = enemy.IgnorePlayerDefense ? 0 : (player.Armor?.Defense ?? 0);

            if (player.Defending)
            {

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
    }
}