using Model;
using System;
using System.Numerics;


namespace Model
{

    class Program
    {


        static void Main()
        {
            Player player = new Player();

            Console.WriteLine("Управление: вводите A для атаки, D для защиты. При подборе предметов — Y/N.");
            Console.WriteLine("Нажмите Enter, чтобы начать...");
            Console.ReadLine();

            int turn = 0;
            while (player.HP > 0)
            {
                turn++;
                Console.WriteLine($"Ход {turn}");

                if (turn % 10 == 0)
                {
                    Console.WriteLine("Вы наткнулись на босса!");
                    int bossIndex = RandomChoice.Next(4); 
                    Enemy boss = EnemyFactory.CreateBoss(bossIndex);
                    bool alive = CombatSystem.Combat(player, boss);
                    if (!alive) break;
                    continue;
                }

                
                if (RandomChoice.Next(2) == 0)
                {
                    Console.WriteLine("Вы нашли сундук!");
                    object item = ItemGenerator.GenerateRandomItem();
                    ItemGenerator.ProcessItem(item, player);
                }
                else
                {
                    Enemy enemy = EnemyFactory.GenerateRandomEnemy();
                    bool alive = CombatSystem.Combat(player, enemy);
                    if (!alive) break;
                }

                Console.WriteLine();
            }
        }
    }
}
