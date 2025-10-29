using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace AutoServiceSimple
{
    class Part
    {
        public int Id;
        public string Name;
        public decimal Price;
        public int Quantity;

        public Part(int id, string name, decimal price, int qty)
        {
            Id = id;
            Name = name;
            Price = price > 0 ? price : 1;
            Quantity = qty >= 0 ? qty : 0;
        }
    }

    class PendingOrder
    {
        public int Id;
        public int PartId;
        public int Qty;
        public int Remain;
    }

    class Game
    {

        readonly string connectionString = ("Server=bd-kip.fa.ru;Database=!!pr7Guts;User Id=sa;Password=1qaz!QAZ;");

        decimal balance = 200;
        int carsSinceOrder = 0;
        Random rnd = new Random();

        List<Part> stock = new List<Part>();
        List<PendingOrder> pending = new List<PendingOrder>();

        public void Run()
        {
            Console.WriteLine("Добро пожаловать в автосервис!");

            LoadFromDb();

            while (true)
            {
                Console.WriteLine($"\nБаланс: {balance:0.00}");
                Console.WriteLine($"Склад: {string.Join(", ", stock.Select(p => $"{p.Name}:{p.Quantity}"))}");
                Console.WriteLine("\n1) Новый клиент");
                Console.WriteLine("2) Закупка запчастей");
                Console.WriteLine("3) Ожидаемые поставки");
                Console.WriteLine("0) Выход");
                Console.Write("Выбор: ");

                string cmd = Console.ReadLine();
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

        void LoadFromDb()
        {
            stock.Clear();
            pending.Clear();

            var conn = new SqlConnection(connectionString);
            conn.Open();

            using (var cmd = new SqlCommand("SELECT Id, Name, Price, Quantity FROM Parts", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    stock.Add(new Part(
                        reader.GetInt32(0),
                        reader.GetString(1),
                        reader.GetDecimal(2),
                        reader.GetInt32(3)
                    ));
                }
            }

            using (var cmd = new SqlCommand("SELECT Id, PartId, Qty, Remain FROM PendingOrders", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    pending.Add(new PendingOrder
                    {
                        Id = reader.GetInt32(0),
                        PartId = reader.GetInt32(1),
                        Qty = reader.GetInt32(2),
                        Remain = reader.GetInt32(3)
                    });
                }
            }

            using (var cmd = new SqlCommand("SELECT Balance, CarsSinceOrder FROM GameState WHERE Id = 1", conn))
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    balance = reader.GetDecimal(0);
                    carsSinceOrder = reader.GetInt32(1);
                }
            }
        }

        void PersistState()

{
            var conn = new SqlConnection(connectionString);
        conn.Open();
            var tran = conn.BeginTransaction();

            foreach (var p in stock)
            {
                var cmd = conn.CreateCommand();
        cmd.Transaction = tran;
                cmd.CommandText = "UPDATE Parts SET Quantity = @qty, Price = @price, Name = @name WHERE Id = @id";
                cmd.Parameters.AddWithValue("@qty", p.Quantity);
                cmd.Parameters.AddWithValue("@price", p.Price);
                cmd.Parameters.AddWithValue("@name", p.Name);
                cmd.Parameters.AddWithValue("@id", p.Id);
                cmd.ExecuteNonQuery();
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "DELETE FROM PendingOrders";
                cmd.ExecuteNonQuery();
            }

foreach (var po in pending)
{
    var cmd = conn.CreateCommand();
    cmd.Transaction = tran;
    cmd.CommandText = "INSERT INTO PendingOrders (PartId, Qty, Remain) VALUES (@partId, @qty, @remain)";
    cmd.Parameters.AddWithValue("@partId", po.PartId);
    cmd.Parameters.AddWithValue("@qty", po.Qty);
    cmd.Parameters.AddWithValue("@remain", po.Remain);
    cmd.ExecuteNonQuery();
}


using (var cmd = conn.CreateCommand())
{
    cmd.Transaction = tran;
    cmd.CommandText = "UPDATE GameState SET Balance = @bal, CarsSinceOrder = @cars WHERE Id = 1";
    cmd.Parameters.AddWithValue("@bal", balance);
    cmd.Parameters.AddWithValue("@cars", carsSinceOrder);
    cmd.ExecuteNonQuery();
}

tran.Commit();
        }

        void ServeClient()
{
    carsSinceOrder++;
    DeliverOrders();

    var availablePart = stock[rnd.Next(stock.Count)];
    var jobPrice = availablePart.Price + rnd.Next(10, 40);

    Console.WriteLine($"Клиент: нужна замена детали \"{availablePart.Name}\"");
    Console.WriteLine($"Клиент заплатит: {jobPrice:0.00}");
    Console.WriteLine("1) Починить  2) Отказать");
    Console.Write("Действие: ");

    string choice = Console.ReadLine();
    Console.WriteLine();

    switch (choice)
    {
        case "1":
            if (availablePart.Quantity > 0)
            {
                availablePart.Quantity--;
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

        default:
            decimal pen = jobPrice * 0.5m;
            balance -= pen;
            Console.WriteLine($"Отказ — штраф {pen:0.00}");
            break;
    }

    PersistState();

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

    Console.
                Write("Введите номер детали: ");
            if (!int.TryParse(Console.ReadLine(), out int num) || num < 1 || num > stock.Count)
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
            pending.Add(new PendingOrder { PartId = part.Id, Qty = qty, Remain = 2 });
            Console.WriteLine($"Заказано {qty}x {part.Name}. Поставка через 2 клиента.");

            PersistState();
        }

        void DeliverOrders()
        {
            if (!pending.Any()) return;

            for (int i = pending.Count - 1; i >= 0; i--)
            {
                pending[i].Remain--;

                if (pending[i].Remain <= 0)
                {
                    var po = pending[i];
                    var part = stock.FirstOrDefault(p => p.Id == po.PartId);
                    if (part != null)
                    {
                        part.Quantity += po.Qty;
                        Console.WriteLine($"Поставка: {po.Qty}x {part.Name} добавлено на склад!");
                    }
                    pending.RemoveAt(i);
                }
            }

            PersistState();
        }

        void ShowPending()
        {
            if (!pending.Any())
            {
                Console.WriteLine("Нет ожидаемых поставок.");
                return;
            }

            Console.WriteLine("Ожидаемые поставки:");
            foreach (var po in pending)
            {
                var part = stock.FirstOrDefault(p => p.Id == po.PartId);
                string name = part?.Name ?? ("PartId=" + po.PartId);
                Console.WriteLine($"- {po.Qty}x {name}, прибудет через {po.Remain} клиентов");
            }
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
