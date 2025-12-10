using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game();
            game.Start();
        }
    }

    public class Game
    {
        private List<PendingPurchase> pendingPurchases = new List<PendingPurchase>();

        public class PendingPurchase
        {
            public int PartID;
            public int Count;
            public int ClientsToServe;
        }
        private decimal Balance => Core.Context.WareHouse.First().Balance;

        public void Start()
        {
            Console.Clear();
            Console.WriteLine("=== АВТОСЕРВИС — НАЧАЛО ИГРЫ ===");
            Console.WriteLine();

            if (!Core.Context.Parts.Any())
            {
                Console.WriteLine("Ошибка: таблица Parts пустая.");
                return;
            }

            if (!Core.Context.WareHouse.Any())
            {
                Console.WriteLine("Ошибка: отсутствует запись WareHouse.");
                return;
            }

            MainLoop();
        }

        private void MainLoop()
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine($"Ваш баланс: {Balance} руб.");
                Console.WriteLine("1 — принять клиента");
                Console.WriteLine("2 — закупить детали");
                Console.WriteLine("3 — склад");
                Console.WriteLine("0 — выход");

                Console.Write("Выбор: ");
                string input = Console.ReadLine();

                switch (input)
                {
                    case "1": HandleClient(); break;
                    case "2": PurchaseMenu(); break;
                    case "3": PrintWarehouse(); break;
                    case "0": return;
                    default: Console.WriteLine("Неверный ввод."); break;
                }
            }
        }

        private void HandleClient()
        {
            Random rnd = new Random();

            var parts = Core.Context.Parts.ToList();
            if (parts.Count == 0)
            {
                Console.WriteLine("Нет деталей в базе.");
                return;
            }

            var brokenPart = parts[rnd.Next(parts.Count)];

            decimal laborCost = 200; 
            decimal totalPrice = brokenPart.Price + laborCost;

            Console.WriteLine($"\nКлиент: сломалась деталь — {brokenPart.Name}");
            Console.WriteLine($"Стоимость ремонта: {totalPrice} руб.");
            Console.Write("Принять заказ? (y/n): ");
            string input = Console.ReadLine()?.ToLower();

            if (input != "y")
            {
                ApplyFine(totalPrice * 0.1m);
                Console.WriteLine("Вы отказались от ремонта, штраф списан.");
                return;
            }


            var whPart = Core.Context.WarehouseParts
                .FirstOrDefault(x => x.PartsID == brokenPart.ID && x.Count > 0);

            if (whPart != null)
            {
                whPart.Count--;
                Core.Context.SaveChanges();

                var warehouse = Core.Context.WareHouse.First();
                warehouse.Balance += totalPrice;
                Core.Context.SaveChanges();

                Console.WriteLine($"Ремонт выполнен! +{totalPrice} руб.");
            }
            else
            {
                Console.WriteLine("Детали на складе нет. Клиент недоволен, списан штраф.");
                ApplyFine(totalPrice * 1.5m);
            }
            UpdatePendingPurchases();
        }

        private void RepairCar(Parts part, WarehouseParts whPart)
        {
            whPart.Count--;
            Core.Context.WarehouseParts.Attach(whPart);
            Core.Context.SaveChanges();

            var warehouse = Core.Context.WareHouse.First();
            warehouse.Balance += part.Price + 200;
            Core.Context.SaveChanges();

            Console.WriteLine($"Ремонт выполнен! +{part.Price + 200} руб.");
        }

        private void PayFine()
        {
            var warehouse = Core.Context.WareHouse.First();
            warehouse.Balance -= 100;
            Core.Context.SaveChanges();

            Console.WriteLine("-100 руб. штраф");
        }

        private void PurchaseMenu()
        {
            Console.WriteLine();
            Console.WriteLine("МЕНЮ");

            var parts = Core.Context.Parts.ToList();

            for (int i = 0; i < parts.Count; i++)
                Console.WriteLine($"{i + 1}. {parts[i].Name} — {parts[i].Price} руб.");

            Console.Write("Выберите деталь (0 = выход): ");
            if (!int.TryParse(Console.ReadLine(), out int choice) ||
                choice < 1 || choice > parts.Count)
                return;

            var part = parts[choice - 1];

            Console.Write("Количество: ");
            if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0)
            {
                Console.WriteLine("Неверно.");
                return;
            }

            decimal total = count * part.Price;
            var warehouse = Core.Context.WareHouse.First();

            if (warehouse.Balance < total)
            {
                Console.WriteLine("Недостаточно средств.");
                return;
            }

            warehouse.Balance -= total;
            Core.Context.SaveChanges();

            AddToStock(part.ID, count);

            Console.WriteLine($"Закуплено {count} шт. деталь {part.Name} (-{total} руб.)");
        }

        private void AddToStock(int partId, int count)
        {
            pendingPurchases.Add(new PendingPurchase
            {
                PartID = partId,
                Count = count,
                ClientsToServe = 2
            });

            Console.WriteLine($"Деталь будет добавлена на складе через 2 дня.");
        }

        private void PrintWarehouse()
        {
            Console.WriteLine();
            Console.WriteLine("СКЛАД");

            var items = Core.Context.WarehouseParts.ToList();

            foreach (var i in items)
            {
                var p = Core.Context.Parts.First(x => x.ID == i.PartsID);
                Console.WriteLine($"{p.Name} — {i.Count} шт.");
            }
        }

        private void ApplyFine(decimal fineAmount)
        {

            var warehouse = Core.Context.WareHouse.First();

            warehouse.Balance -= fineAmount;
            Core.Context.SaveChanges();

            Console.WriteLine($"Списано штрафа: {fineAmount} руб.");

            if (warehouse.Balance < 0)
            {
                Console.WriteLine("Игра окончена.");
                Environment.Exit(0);
            }
        }
        private void UpdatePendingPurchases()
        {
            foreach (var p in pendingPurchases.ToList())
            {
                p.ClientsToServe--;

                if (p.ClientsToServe <= 0)
                {
                    var row = Core.Context.WarehouseParts.FirstOrDefault(x => x.PartsID == p.PartID);

                    if (row == null)
                    {
                        row = new WarehouseParts()
                        {
                            WarehouseID = Core.Context.WareHouse.First().Id,
                            PartsID = p.PartID,
                            Count = p.Count
                        };
                        Core.Context.WarehouseParts.Add(row);
                    }
                    else
                    {
                        row.Count += p.Count;
                    }

                    Core.Context.SaveChanges();
                    pendingPurchases.Remove(p);

                    Console.WriteLine($"Деталь добавлена на склад ({p.Count} шт.)");
                }
            }
        }

    }
}
