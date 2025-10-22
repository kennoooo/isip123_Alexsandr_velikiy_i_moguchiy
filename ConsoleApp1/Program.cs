using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoServiceSimple
{
    class Part
    {
        public string Name;
        public decimal Price;
        public int Quantity;

        public Part(string name, decimal price, int qty)
        {
            Name = name;
            Price = price > 0 ? price : 1;
            Quantity = qty >= 0 ? qty : 0;
        }
    }

    class Game
    {
        decimal balance = 200;
        int carsSinceOrder = 0;
        Random rnd = new();

        List<Part> stock = new()
        {
            new("Тормозной диск", 50, 2),
            new("Свеча зажигания", 15, 4),
            new("Аккумулятор", 80, 1)
        };

        List<(Part part, int qty, int remain)> pending = new();

        public void Run()
        {
            Console.WriteLine("Добро пожаловать в автосервис!");

            while (true)
            {
                Console.WriteLine($"\nБаланс: {balance:0.00}");
                Console.WriteLine($"Склад: {string.Join(", ", stock.Select(p => $"{p.Name}:{p.Quantity}"))}");
                Console.WriteLine("\n1) Новый клиент");
                Console.WriteLine("2) Закупка запчастей");
                Console.WriteLine("3) Ожидаемые поставки");
                Console.WriteLine("0) Выход");
                Console.Write("Выбор: ");

                string? cmd = Console.ReadLine();
                Console.WriteLine();

                switch (cmd)
                {
                    case "1":
                        ServeClient();
                        break;
                    case "2":
                        BuyParts();
                        break;
                    case "3":
                        ShowPending();
                        break;
                    case "0":
                        Console.WriteLine("До свидания!");
                        return;
                    default:
                        Console.WriteLine("Неверная команда!");
                        break;
                }
            }
        }

        void ServeClient()
        {
            carsSinceOrder++;
            DeliverOrders();

            var part = stock[rnd.Next(stock.Count)];
            var jobPrice = part.Price + rnd.Next(10, 40);

            Console.WriteLine($"Клиент: нужна замена детали \"{part.Name}\"");
            Console.WriteLine($"Клиент заплатит: {jobPrice:0.00}");
            Console.WriteLine("1) Починить  2) Отказать");
            Console.Write("Действие: ");

            string? choice = Console.ReadLine();
            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    if (part.Quantity > 0)
                    {
                        part.Quantity--;
                        balance += jobPrice;
                        Console.WriteLine("Ремонт успешен!");
                    }
                    else
                    {
                        decimal fine = jobPrice * 1.5m;
                        balance -= fine;
                        Console.WriteLine($"Нет нужной детали! Штраф {fine:0.00}");
                    }
                    break;

                case "2":
                    decimal penalty = jobPrice * 0.5m;
                    balance -= penalty;
                    Console.WriteLine($"Отказ — штраф {penalty:0.00}");
                    break;

            }

            if (balance < 0)
            {
                Console.WriteLine("\nВы разорились! Игра окончена.");
                Environment.Exit(0);
            }
        }

        void BuyParts()
        {
            Console.WriteLine("Доступные запчасти:");
            for (int i = 0; i < stock.Count; i++)
                Console.WriteLine($"{i + 1}) {stock[i].Name} — {stock[i].Price:0.00}");

            Console.Write("Введите номер детали: ");
            if (!int.TryParse(Console.ReadLine(), out int num)  && num < 1 &&  num > stock.Count)
            {
                Console.WriteLine("Неверный выбор.");
                return;
            }

            var part = stock[num - 1];
            Console.Write("Введите количество: ");
            if (!int.TryParse(Console.ReadLine(), out int qty) || qty <= 0)
            {
                Console.WriteLine("Неверное количество.");
                return;
            }

            decimal totalCost = part.Price * qty;
            if (balance < totalCost)
            {
                Console.WriteLine($"Недостаточно средств! Нужно {totalCost:0.00}, а у вас {balance:0.00}");
                return;
            }

            balance -= totalCost;
            pending.Add((part, qty, 2)); // придёт через 2 клиента
            Console.WriteLine($"Заказано {qty}x {part.Name}. Поставка через 2 клиента.");
        }

        void DeliverOrders()
        {
            if (!pending.Any()) return;

            for (int i = pending.Count - 1; i >= 0; i--)
            {
                var (part, qty, remain) = pending[i];
                remain--;

                if (remain <= 0)
                {
                    part.Quantity += qty;
                    Console.WriteLine($"Поставка: {qty}x {part.Name} добавлено на склад!");
                    pending.RemoveAt(i);
                }
                else
                {
                    pending[i] = (part, qty, remain);
                }
            }
        }

        void ShowPending()
        {
            if (!pending.Any())
            {
                Console.WriteLine("Нет ожидаемых поставок.");
                return;
            }

            Console.WriteLine("Ожидаемые поставки:");
            foreach (var (part, qty, remain) in pending)
                Console.WriteLine($"- {qty}x {part.Name}, прибудет через {remain} клиентов");
        }
    }

    class Program
    {
        static void Main()
        {
            new Game().Run();
        }
    }
}
