using System;
using System.Collections.Generic;

/*
 Простая консольная пошаговая мини-игра-роуглайк на C#.
 - Игрок имеет здоровье и экипировку (оружие + доспехи).
 - На каждом ходу 50/50: сундук или враг. Каждые 10 ходов — босс.
 - В бою игрок ходит первым (Атака или Защита). Враг всегда отвечает.
 - Особенности врагов: гоблин (шанс крита), скелет (игнор защиты игрока), маг (шанс заморозки — игрок пропускает следующий бой-ход).
 - Предметы из сундука: зелье (полное лечение), оружие или доспех (можно подобрать/отбросить).

 Код построен компактно и снабжён подробными комментариями по основным частям.
*/

class Program
{
    static Random rng = new Random();

    // --- Простые структуры данных для предметов и существ ---
    class Weapon { public string Name; public int Attack; public override string ToString() => $"{Name} (+{Attack} ATK)"; }
    class Armor { public string Name; public int Defense; public override string ToString() => $"{Name} (+{Defense} DEF)"; }

    class Player
    {
        public int MaxHP = 100;
        public int HP = 100;
        public Weapon Weapon = new Weapon { Name = "Кулаки", Attack = 0 };
        public Armor Armor = new Armor { Name = "Обычная одежда", Defense = 0 };
        public bool Frozen = false; // если true — игрок пропускает следующий боевой ход
        public bool Defending = false; // режим защиты на текущую атаку
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

    // --- Базовые значения для обычных противников ---
    static Enemy MakeBaseGoblin()
    {
        return new Enemy { Name = "Гоблин", Race = EnemyRace.Goblin, HP = 30, Attack = 8, Defense = 3, CritChance = 0.12 };
    }
    static Enemy MakeBaseSkeleton()
    {
        return new Enemy { Name = "Скелет", Race = EnemyRace.Skeleton, HP = 28, Attack = 9, Defense = 4, IgnorePlayerDefense = true };
    }
    static Enemy MakeBaseMage()
    {
        return new Enemy { Name = "Маг", Race = EnemyRace.Mage, HP = 24, Attack = 7, Defense = 2, FreezeChance = 0.18 };
    }

    // --- Боссы (наследуют особенности расы и модифицируют характеристики) ---
    static Enemy MakeBoss(int bossIndex)
    {
        // bossIndex 0..3 — четыре разных босса
        Enemy e = bossIndex switch
        {
            0 => MakeBaseGoblin(), // VVG (гоблин-босс)
            1 => MakeBaseSkeleton(), // Ковальский (скелет-босс)
            2 => MakeBaseMage(), // Архимаг C++ (маг-босс)
            3 => MakeBaseSkeleton(), // Пестов S-- (скелет с особенностями)
            _ => MakeBaseGoblin()
        };
        switch (bossIndex)
        {
            case 0: // ВВГ (гоблин)
                e.Name = "ВВГ";
                e.HP = (int)(e.HP * 2.0);
                e.Attack = (int)Math.Ceiling(e.Attack * 1.5);
                e.Defense = (int)Math.Ceiling(e.Defense * 1.2);
                e.CritChance += 0.10; // +10% к шансу крита
                break;
            case 1: // Ковальский (скелет)
                e.Name = "Ковальский";
                e.HP = (int)(e.HP * 2.5);
                e.Attack = (int)Math.Ceiling(e.Attack * 1.3);
                e.Defense = (int)Math.Ceiling(e.Defense * 1.4);
                e.IgnorePlayerDefense = true; // уже true, но повторим для ясности
                break;
            case 2: // Архимаг C++ (маг)
                e.Name = "Архимаг C++";
                e.HP = (int)(e.HP * 1.8);
                e.Attack = (int)Math.Ceiling(e.Attack * 1.6);
                e.Defense = (int)Math.Ceiling(e.Defense * 1.1);
                e.FreezeChance += 0.10; // +10% к заморозке
                break;
            case 3: // Пестов S-- (скелет с шансом заморозки)
                e.Name = "Пестов S--";
                e.HP = (int)(e.HP * 1.3);
                e.Attack = (int)Math.Ceiling(e.Attack * 1.8);
                e.Defense = Math.Max(0, (int)Math.Floor(e.Defense * 0.6));
                e.IgnorePlayerDefense = true;
                e.FreezeChance = 0.18 + 0.15; // добавляет шанс заморозки +15% к обычному магу
                break;
        }
        return e;
    }

    // --- Генерация случайных обычных врагов ---
    static Enemy GenerateRandomEnemy()
    {
        int t = rng.Next(3);
        return t switch
        {
            0 => MakeBaseGoblin(),
            1 => MakeBaseSkeleton(),
            2 => MakeBaseMage(),
            _ => MakeBaseGoblin()
        };
    }

    // --- Генерация случайного предмета из сундука ---
    static object GenerateRandomItem()
    {
        int t = rng.Next(3);
        if (t == 0) return "potion"; // полное исцеление
        if (t == 1) // оружие
            return new Weapon { Name = $"Меч_{rng.Next(1,1000)}", Attack = rng.Next(1, 6) };
        else // доспех
            return new Armor { Name = $"Доспех_{rng.Next(1,1000)}", Defense = rng.Next(1, 6) };
    }

    // --- Боевой цикл между игроком и врагом ---
    // Возвращает true если игрок выжил, false если погиб.
    static bool Combat(Player p, Enemy enemy)
    {
        Console.WriteLine($"Начинается бой: {enemy}");

        while (p.HP > 0 && enemy.HP > 0)
        {
            // Если игрок заморожен — он пропускает свой ход (только в бою).
            if (p.Frozen)
            {
                Console.WriteLine("Вы заморожены и пропускаете ход!");
                p.Frozen = false; // заморозка срабатывает один раз
            }
            else
            {
                // Ход игрока: выбор Атаки или Защиты
                Console.WriteLine($"Ваш HP: {p.HP}/{p.MaxHP}  Оружие: {p.Weapon}  Доспех: {p.Armor}");
                Console.Write("Выберите действие (A - Атака, D - Защита): ");
                string cmd = Console.ReadLine().Trim().ToLower();
                p.Defending = false;
                if (cmd == "d" || cmd == "защита" || cmd == "def")
                {
                    p.Defending = true; // включаем режим защиты — влияет на следующую атаку врага
                    Console.WriteLine("Вы встали в защиту: шанс уклониться 40% или блок уменьшает урон.");
                }
                else
                {
                    // Атакуем
                    int playerBase = 8 // базовый урон игрока без оружия
                    int damage = Math.Max(1, playerBase + (p.Weapon?.Attack ?? 0) - enemy.Defense);
                    enemy.HP -= damage;
                    Console.WriteLine($"Вы атакуете и наносите {damage} урона (HP врага: {Math.Max(0, enemy.HP)})");
                }
            }

            // Если враг жив — он отвечает
            if (enemy.HP > 0)
            {
                // Враг атакует
                int incoming = enemy.Attack;

                // Если враг игнорирует защиту игрока, то защита брони не учитывается
                int playerArmorValue = enemy.IgnorePlayerDefense ? 0 : (p.Armor?.Defense ?? 0);

                // Если игрок защищается — сначала шанс уклониться 40%
                if (p.Defending)
                {
                    double roll = rng.NextDouble();
                    if (roll < 0.40) // уклонение — урон 0
                    {
                        Console.WriteLine("Вы успешно уклонились от атаки!");
                    }
                    else
                    {
                        // Не уклонились — блок уменьшает получаемый урон на 70–100% от характеристики защиты
                        double factor = 0.7 + rng.NextDouble() * 0.3; // 0.7..1.0
                        double reduction = playerArmorValue * factor;
                        int damage = Math.Max(0, (int)Math.Round(incoming - reduction));
                        // Учитываем крит у гоблина и возможность заморозки ниже
                        bool wasCrit = rng.NextDouble() < enemy.CritChance;
                        if (wasCrit) { damage = damage * 2; Console.WriteLine("Критический удар от врага!"); }
                        if (enemy.FreezeChance > 0 && rng.NextDouble() < enemy.FreezeChance) { p.Frozen = true; Console.WriteLine("Враг наложил заморозку — вы пропустите следующий боевой ход!"); }
                        p.HP -= damage;
                        Console.WriteLine($"Враг атакует и вы получаете {damage} урона (HP: {Math.Max(0, p.HP)})");
                    }
                }
                else
                {
                    // Игрок не защищается — обычный удар
                    int effectiveArmor = playerArmorValue;
                    int damage = Math.Max(0, incoming - effectiveArmor);
                    bool wasCrit = rng.NextDouble() < enemy.CritChance;
                    if (wasCrit) { damage = damage * 2; Console.WriteLine("Критический удар от врага!"); }
                    if (enemy.FreezeChance > 0 && rng.NextDouble() < enemy.FreezeChance) { p.Frozen = true; Console.WriteLine("Враг наложил заморозку — вы пропустите следующий боевой ход!"); }
                    p.HP -= damage;
                    Console.WriteLine($"Враг атакует и вы получаете {damage} урона (HP: {Math.Max(0, p.HP)})");
                }
            }

            // Сбрасываем режим защиты после одного применения
            p.Defending = false;
            Console.WriteLine();
        }

        if (p.HP <= 0) Console.WriteLine("Вы погибли. Игра окончена.");
        else Console.WriteLine("Враг повержен!");

        return p.HP > 0;
    }

    static void Main()
    {
        Player player = new Player();
        Console.WriteLine("--- Текстовая консольная пошаговая мини-игра-роуглайк ---");
        Console.WriteLine("Управление: вводите A для атаки, D для защиты. При подборе предметов — Y/N.");
        Console.WriteLine("Нажмите Enter, чтобы начать...");
        Console.ReadLine();

        int turn = 0;
        while (player.HP > 0)
        {
            turn++;
            Console.WriteLine($"--- Ход {turn} ---");

            // Каждые 10 ходов — босс
            if (turn % 10 == 0)
            {
                Console.WriteLine("Вы наткнулись на босса!");
                int bossIndex = rng.Next(4);
                Enemy boss = MakeBoss(bossIndex);
                bool alive = Combat(player, boss);
                if (!alive) break;
                continue;
            }

            // Иначе 50/50 сундук или враг
            if (rng.Next(2) == 0) // сундук
            {
                Console.WriteLine("Вы нашли сундук!");
                object item = GenerateRandomItem();
                if (item is string s && s == "potion")
                {
                    player.HealFull();
                    Console.WriteLine("В сундуке — лечебное зелье. Вы полностью исцелены!");
                }
                else if (item is Weapon w)
                {
                    Console.WriteLine($"В сундуке — новое оружие: {w}");
                    Console.WriteLine($"Текущее оружие: {player.Weapon}");
                    Console.Write("Взять новое оружие? (y/n): ");
                    string ans = Console.ReadLine().Trim().ToLower();
                    if (ans == "y" || ans == "д" ) { player.Weapon = w; Console.WriteLine("Оружие заменено."); }
                    else Console.WriteLine("Оружие выброшено.");
                }
                else if (item is Armor a)
                {
                    Console.WriteLine($"В сундуке — новый доспех: {a}");
                    Console.WriteLine($"Текущий доспех: {player.Armor}");
                    Console.Write("Взять новый доспех? (y/n): ");
                    string ans = Console.ReadLine().Trim().ToLower();
                    if (ans == "y" || ans == "д") { player.Armor = a; Console.WriteLine("Доспех заменён."); }
                    else Console.WriteLine("Доспех выброшен.");
                }
            }
            else // враг
            {
                Enemy enemy = GenerateRandomEnemy();
                bool alive = Combat(player, enemy);
                if (!alive) break;
            }

            Console.WriteLine();
        }

        Console.WriteLine("Спасибо за игру!");
    }
}
