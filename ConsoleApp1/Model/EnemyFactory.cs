using Model;
using System;

namespace Model
{
    public static class EnemyFactory
    {


        public static Enemy CreateBaseGoblin()
        {
            return new Enemy
            {
                Name = "Гоблин",
                Race = EnemyRace.Goblin,
                HP = 30,
                Attack = 8,
                Defense = 3,
                CritChance = 0.12
            };
        }

        public static Enemy CreateBaseSkeleton()
        {
            return new Enemy
            {
                Name = "Скелет",
                Race = EnemyRace.Skeleton,
                HP = 28,
                Attack = 9,
                Defense = 4,
                IgnorePlayerDefense = true
            };
        }

        public static Enemy CreateBaseMage()
        {
            return new Enemy
            {
                Name = "Маг",
                Race = EnemyRace.Mage,
                HP = 24,
                Attack = 7,
                Defense = 2,
                FreezeChance = 0.18
            };
        }

        public static Enemy CreateBaseSlime()
        {
            return new Enemy
            {
                Name = "Слизень",
                Race = EnemyRace.Slime,
                HP = 35,
                Attack = 6,
                Defense = 1,
                ReduceIncomingDamage = true,
                DamageReduction = 2
            };
        }

        public static Enemy CreateBoss(int bossIndex)
        {
            Enemy e = bossIndex switch
            {
                0 => CreateBaseGoblin(),
                1 => CreateBaseSkeleton(),
                2 => CreateBaseMage(),
                3 => CreateBaseSkeleton(),
                _ => CreateBaseGoblin()
            };

            switch (bossIndex)
            {
                case 0:
                    e.Name = "ВВГ";
                    e.HP = (int)(e.HP * 2.0);
                    e.Attack = (int)System.Math.Ceiling(e.Attack * 1.5);
                    e.Defense = (int)System.Math.Ceiling(e.Defense * 1.2);
                    e.CritChance += 0.10;
                    break;
                case 1:
                    e.Name = "Ковальский";
                    e.HP = (int)(e.HP * 2.5);
                    e.Attack = (int)System.Math.Ceiling(e.Attack * 1.3);
                    e.Defense = (int)System.Math.Ceiling(e.Defense * 1.4);
                    e.IgnorePlayerDefense = true;
                    break;
                case 2:
                    e.Name = "Архимаг C++";
                    e.HP = (int)(e.HP * 1.8);
                    e.Attack = (int)System.Math.Ceiling(e.Attack * 1.6);
                    e.Defense = (int)System.Math.Ceiling(e.Defense * 1.1);
                    e.FreezeChance += 0.10;
                    break;
                case 3:
                    e.Name = "Пестов S--";
                    e.HP = (int)(e.HP * 1.3);
                    e.Attack = (int)System.Math.Ceiling(e.Attack * 1.8);
                    e.Defense = System.Math.Max(0, (int)System.Math.Floor(e.Defense * 0.6));
                    e.IgnorePlayerDefense = true;
                    e.FreezeChance = 0.18 + 0.15;
                    break;
            }
            return e;
        }

        public static Enemy GenerateRandomEnemy()
        {
            int t = RandomChoice.Next(4);
            return t switch
            {
                0 => CreateBaseGoblin(),
                1 => CreateBaseSkeleton(),
                2 => CreateBaseMage(),
                3 => CreateBaseSlime(),
                _ => CreateBaseGoblin()
            };
        }
    }
}