using System;
using System.Collections.Generic;
using System.Linq;

namespace StoreApp
{
    enum Category { Electronics, Food, Clothes }

    class Product
    {
        private static int nextCode = 1000;
        public int Code { get; }
        public string Name { get; }
        public decimal Price { get; private set; }
        public int Quantity { get; private set; }
        public bool InStock => Quantity > 0;
        public Category Category { get; }

        public Product(string name, decimal price, int qty, Category cat)
        {
            if (string.IsNullOrWhiteSpace(name) || price <= 0 || qty < 0)
                throw new ArgumentException("Некорректные данные для товара");

            Code = ++nextCode;
            Name = name;
            Price = price;
            Quantity = qty;
            Category = cat;
        }

        public void AddStock(int qty) => Quantity += qty;
        public bool Sell(int qty)
        {
            if (qty <= 0 || qty > Quantity) return false;
            Quantity -= qty; return true;
        }


    }

    class Program
    {
        static List<Product> products = new List<Product>
        {

        };

        static void Main()
        {
            while (true)
            {
                Console.WriteLine("\n1. Добавить  \n2. Удалить  \n3. Поставка  \n4. Продажа  \n5. Поиск  \n6. Выход");
                switch (Console.ReadLine())
                {
                    case "1": AddProduct(); break;
                    case "2": Modify(p => products.Remove(p), "удалён"); break;
                    case "3": Modify(p => { p.AddStock(InputInt("Сколько добавить? ")); }, "пополнен"); break;
                    case "4":
                        Modify(p => {
                            int qty = InputInt("Сколько продать? ");
                            Console.WriteLine(p.Sell(qty) ? "Продано." : "Недостаточно товара.");
                        }); break;
                    case "5": Search(); break;
                    case "6": return;
                    default: Console.WriteLine("Неверная команда."); break;
                }
            }
        }

        static void AddProduct()
        {
            try
            {
                string name = Input("Название: ");
                decimal price = InputDecimal("Цена: ");
                int qty = InputInt("Количество: ");
                Console.WriteLine("Категория (0 - Electronics, 1 - Food, 2 - Clothes): ");
                Category cat = (Category)InputInt("Ваш выбор: ", 0, 2);
                products.Add(new Product(name, price, qty, cat));
                Console.WriteLine("Товар добавлен.");
            }
            catch (Exception e) { Console.WriteLine("Ошибка: " + e.Message); }
        }

        static void Modify(Action<Product> action, string successMsg = "")
        {
            var p = FindByCode();
            if (p == null) return;
            action(p);
            if (!string.IsNullOrEmpty(successMsg)) Console.WriteLine($"Товар {successMsg}.");
        }

        static void Search()
        {
            Console.WriteLine("1.Код  2.Название  3.Категория");
            switch (Console.ReadLine())
            {
                case "1": Show(FindByCode()); break;
                case "2":
                    string name = Input("Название: ");
                    Show(products.Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase)));
                    break;
                case "3":
                    int cat = InputInt("Категория (0-2): ", 0, 2);
                    Show(products.Where(p => p.Category == (Category)cat));
                    break;
                default: Console.WriteLine("Ошибка ввода."); break;
            }
        }

        static Product FindByCode()
        {
            int code = InputInt("Введите код товара: ");
            var p = products.FirstOrDefault(x => x.Code == code);
            if (p == null) Console.WriteLine("Товар не найден.");
            return p;
        }

        static void Show(Product p) { if (p != null) Console.WriteLine(p); }
        static void Show(IEnumerable<Product> list) { foreach (var p in list) Console.WriteLine(p); }

        static string Input(string msg) { Console.Write(msg); return Console.ReadLine(); }
        static int InputInt(string msg, int min = int.MinValue, int max = int.MaxValue)
        {
            Console.Write(msg);
            return int.TryParse(Console.ReadLine(), out int v) && v >= min && v <= max ? v : throw new Exception("Некорректное число");
        }
        static decimal InputDecimal(string msg)
        {
            Console.Write(msg);
            return decimal.TryParse(Console.ReadLine(), out decimal v) && v > 0 ? v : throw new Exception("Некорректная цена");
        }
    }
}
