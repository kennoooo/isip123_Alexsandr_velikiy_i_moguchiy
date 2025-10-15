using System;
using System.Collections.Generic;

/*
 Упрощённая и ускоренная версия консольной пошаговой мини-игры-роуглайк на C#.
 Модификации по просьбе: игра стала "быстрее" (меньше ожиданий ввода, быстрый ввод действий через клавиши)
 и немного повышен урон у всех сущностей.

 Основное:
 - Быстрый ввод: для выбора действий и ответов на сундуки используется Console.ReadKey(true) — не нужно нажимать Enter.
 - Увеличен базовый урон игрока и немного повышены показатели атаки врагов и оружия.
 - Сохранил все особенности: шанс крита у гоблина, игнор брони у скелета, заморозка у мага, боссы с модификаторами.
 - Код по-прежнему компактный и снабжён комментариями по ключевым местам.
*/

class Program
{
    static Random rng = new Random();

    // --- Простые структуры для предметов и существ ---
    class Weapon { public string Name; public int Attack; public override string ToString() => $"{Name} (+{Attack} ATK)"; }
    class Armor { public string Name; public int Defense; public override string ToString() => $"{Name} (+{Defense} DEF)"; }

    class Player
    {
        public int MaxHP = 100;
        public int HP = 100;
        public Weapon Weapon = new Weapon { Name = "Кулаки", Attack = 0 };
        public Armor Armor = new Armor { Name = "Обычная одежда", Defense = 0 };
        public bool Frozen = false; // пропуск хода
        public bool Defending = false; // режим защиты
        public void HealFull() => HP = MaxHP;
    }

    enum EnemyRace { Goblin, Skeleton, Mage }

    class Enemy
    {
        public string Name;
        public EnemyRace Race;
        public int HP;
        public int Attack;
        public int Defense;
        public double CritChance = 0.0;   // для гоблина
        public double FreezeChance = 0.0; // для мага
        public bool IgnorePlayerDefense = false; // для скелета
        public override string ToString() => $"{Name} ({Race}) HP:{HP} ATK:{Attack} DEF:{Defense}";
    }

    // --- Базовые враги (с чуть большим уроном) ---
    static Enemy MakeBaseGoblin()
    {
        // Увеличил атаку с 8 -> 10
        return new Enemy { Name = "Гоблин", Race = EnemyRace.Goblin, HP = 30, Attack = 10, Defense = 3, CritChance = 0.12 };
    }
    static Enemy MakeBaseSkeleton()
    {
        // Увеличил атаку с 9 -> 11
        return new Enemy { Name = "Скелет", Race = EnemyRace.Skeleton, HP = 28, Attack = 11, Defense = 4, IgnorePlayerDefense = true };
    }
    static Enemy MakeBaseMage()
    {
        // Увеличил атаку с 7 -> 9
        return new Enemy { Name = "Маг", Race = EnemyRace.Mage, HP = 24, Attack = 9, Defense = 2, FreezeChance = 0.18 };
    }

    // --- Боссы (модифицируют базовые параметры) ---
    static Enemy MakeBoss(int bossIndex)
    {
        Enemy e = bossIndex switch
        {
            0 => MakeBaseGoblin(),
            1 => MakeBaseSkeleton(),
            2 => MakeBaseMage(),
            3 => MakeBaseSkeleton(),
            _ => MakeBaseGoblin()
        };
        switch (bossIndex)
        {
            case 0: // ВВГ (гоблин-босс)
                e.Name = "ВВГ";
                e.HP = (int)(e.HP * 2.0);
                e.Attack = (int)Math.Ceiling(e.Attack * 1.5);
                e.Defense = (int)Math.Ceiling(e.Defense * 1.2);
                e.CritChance += 0.10;
                break;
            case 1: // Ковальский (скелет-босс)
                e.Name = "Ковальский";
                e.HP = (int)(e.HP * 2.5);
                e.Attack = (int)Math.Ceiling(e.Attack * 1.3);
                e.Defense = (int)Math.Ceiling(e.Defense * 1.4);
                e.IgnorePlayerDefense = true;
                break;
            case 2: // Архимаг C++ (маг-босс)
                e.Name = "Архимаг C++";
                e.HP = (int)(e.HP * 1.8);
                e.Attack = (int)Math.Ceiling(e.Attack * 1.6);
                e.Defense = (int)Math.Ceiling(e.Defense * 1.1);
                e.FreezeChance += 0.10;
                break;
            case 3: // Пестов S-- (скелет с шансом заморозки)
                e.Name = "Пестов S--";
                e.HP = (int)(e.HP * 1.3);
                e.Attack = (int)Math.Ceiling(e.Attack * 1.8);
                e.Defense = Math.Max(0, (int)Math.Floor(e.Defense * 0.6));
                e.IgnorePlayerDefense = true;
                e.FreezeChance = 0.18 + 0.15; // комбинируем значение
                break;
        }
        return e;
    }

    // --- Случайный обычный враг ---
    static Enemy GenerateRandomEnemy() => rng.Next(3) switch
    {
        0 => MakeBaseGoblin(),
        1 => MakeBaseSkeleton(),
        2 => MakeBaseMage(),
        _ => MakeBaseGoblin()
    };

    // --- Случайный предмет из сундука ---
    static object GenerateRandomItem()
    {
        int t = rng.Next(3);
        if (t == 0) return "potion"; // зелье
        if (t == 1) // оружие — немного сильнее (ранг 2..8)
            return new Weapon { Name = $"Меч_{rng.Next(1,1000)}", Attack = rng.Next(2, 9) };
        else // доспех
            return new Armor { Name = $"Доспех_{rng.Next(1,1000)}", Defense = rng.Next(1, 6) };
    }

    // --- Боевой цикл ---
    // Сделан компактным и быстро реагирующим (используется Console.ReadKey для выбора).
    static bool Combat(Player p, Enemy enemy)
    {
        Console.WriteLine($"Бой: {enemy}");

        // Базовый урон игрока повышен (с 5 -> 8) для более быстрого завершения боёв
        int playerBase = 8;

        while (p.HP > 0 && enemy.HP > 0)
        {
            if (p.Frozen)
            {
                Console.WriteLine("Вы заморожены — пропускаете ход.");
                p.Frozen = false;
            }
            else
            {
                // Быстрый ввод: A — атака, D — защита (не нужно нажимать Enter)
                Console.WriteLine($"Ваш HP:{p.HP}/{p.MaxHP}  Оружие:{p.Weapon}  Доспех:{p.Armor}");
                Console.Write("Нажмите A (атака) или D (защита): ");
                var key = Console.ReadKey(true);
                p.Defending = false;
                char ch = char.ToLower(key.KeyChar);
                if (ch == 'd' || ch == 'д')
                {
                    p.Defending = true;
                    Console.WriteLine("-> Вы в защите (40% уклонение или блок).");
                }
                else
                {
                    // Атака
                    int damage = Math.Max(1, playerBase + (p.Weapon?.Attack ?? 0) - enemy.Defense);
                    enemy.HP -= damage;
                    Console.WriteLine($"-> Вы наносите {damage} урона. (HP врага: {Math.Max(0, enemy.HP)})");
                }
            }

            // Ответ врага, если жив
            if (enemy.HP > 0)
            {
                int incoming = enemy.Attack;
                int playerArmorValue = enemy.IgnorePlayerDefense ? 0 : (p.Armor?.Defense ?? 0);

                if (p.Defending)
                {
                    double roll = rng.NextDouble();
                    if (roll < 0.40)
                    {
                        Console.WriteLine("-> Вы уклонились!");
                    }
                    else
                    {
                        double factor = 0.7 + rng.NextDouble() * 0.3; // 70..100%
                        double reduction = playerArmorValue * factor;
                        int damage = Math.Max(0, (int)Math.Round(incoming - reduction));
                        if (rng.NextDouble() < enemy.CritChance) { damage *= 2; Console.WriteLine("-> Критический удар!"); }
                        if (enemy.FreezeChance > 0 && rng.NextDouble() < enemy.FreezeChance) { p.Frozen = true; Console.WriteLine("-> Вас заморозили!"); }
                        p.HP -= damage;
                        Console.WriteLine($"-> Враг наносит {damage} урона. (Ваш HP: {Math.Max(0, p.HP)})");
                    }
                }
                else
                {
                    int damage = Math.Max(0, incoming - playerArmorValue);
                    if (rng.NextDouble() < enemy.CritChance) { damage *= 2; Console.WriteLine("-> Критический удар!"); }
                    if (enemy.FreezeChance > 0 && rng.NextDouble() < enemy.FreezeChance) { p.Frozen = true; Console.WriteLine("-> Вас заморозили!"); }
                    p.HP -= damage;
                    Console.WriteLine($"-> Враг наносит {damage} урона. (Ваш HP: {Math.Max(0, p.HP)})");
                }
            }

            p.Defending = false; // защита действует только на один ответ врага
        }

        if (p.HP <= 0) Console.WriteLine("Вы погибли."); else Console.WriteLine("Враг повержен!");
        return p.HP > 0;
    }

    static void Main()
    {
        Player player = new Player();
        Console.WriteLine("--- Быстрая версия: нажимайте клавиши без Enter. Урон немного увеличен. ---");

        int turn = 0;
        while (player.HP > 0)
        {
            turn++;
            Console.WriteLine($"-- Ход {turn} --");

            // Каждые 10 ходов — босс
            if (turn % 10 == 0)
            {
                Console.WriteLine("Наткнулись на босса!");
                Enemy boss = MakeBoss(rng.Next(4));
                if (!Combat(player, boss)) break;
                continue;
            }

            // 50/50 сундук или враг
            if (rng.Next(2) == 0)
            {
                Console.WriteLine("Найден сундук!");
                object item = GenerateRandomItem();
                if (item is string s && s == "potion")
                {
                    player.HealFull();
                    Console.WriteLine("Зелье: вы полностью исцелены.");
                }
                else if (item is Weapon w)
                {
                    Console.WriteLine($"Оружие: {w}. Текущее: {player.Weapon}");
                    Console.Write("Взять? (Y/N): ");
                    var k = Console.ReadKey(true);
                    char c = char.ToLower(k.KeyChar);
                    if (c == 'y' || c == 'д') { player.Weapon = w; Console.WriteLine("-> Оружие надето."); }
                    else Console.WriteLine("-> Оружие выброшено.");
                }
                else if (item is Armor a)
                {
                    Console.WriteLine($"Доспех: {a}. Текущий: {player.Armor}");
                    Console.Write("Взять? (Y/N): ");
                    var k = Console.ReadKey(true);
                    char c = char.ToLower(k.KeyChar);
                    if (c == 'y' || c == 'д') { player.Armor = a; Console.WriteLine("-> Доспех надет."); }
                    else Console.WriteLine("-> Доспех выброшен.");
                }
            }
            else
            {
                Enemy enemy = GenerateRandomEnemy();
                if (!Combat(player, enemy)) break;
            }
        }

        Console.WriteLine("Спасибо за игру!");
    }
}
