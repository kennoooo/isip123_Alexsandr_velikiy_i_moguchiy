using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                throw new ArgumentException("Invalid product data");

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

        public override string ToString()
        {
            return $"Code: {Code}, Name: {Name}, Price: {Price:C}, Quantity: {Quantity}, Category: {Category}, In Stock: {InStock}";
        }
    }

    class Program
    {
        static List<Product> products = new List<Product>();

        static void Main()
        {


            while (true)
            {
                Console.WriteLine("1. Add product\n2. Delete product\n3. Restock\n4. Sell product\n5. Search products\n6. Exit");
                Console.Write("Choose action: ");

                switch (Console.ReadLine())
                {
                    case "1": AddProduct(); break;
                    case "2": Modify(p => products.Remove(p), "deleted"); break;
                    case "3":
                        Modify(p => {
                            int qty = InputInt("How much to add? ");
                            p.AddStock(qty);
                        }, "restocked");
                        break;
                    case "4":
                        Modify(p => {
                            int qty = InputInt("How much to sell? ");
                            Console.WriteLine(p.Sell(qty) ? "Sold." : "Not enough stock.");
                        });
                        break;
                    case "5": Search(); break;
                    case "6": return;
                    default: Console.WriteLine("Invalid command."); break;
                }
            }
        }

        static void AddProduct()
        {
            try
            {
                string name = Input("Name: ");
                decimal price = InputDecimal("Price: ");
                int qty = InputInt("Quantity: ");
                Console.WriteLine("Category (0 - Electronics, 1 - Food, 2 - Clothes): ");
                Category cat = (Category)InputInt("Your choice: ", 0, 2);
                products.Add(new Product(name, price, qty, cat));
                Console.WriteLine("Product added.");
            }
            catch (Exception e) { Console.WriteLine("Error: " + e.Message); }
        }

        static void Modify(Action<Product> action, string successMsg = "")
        {
            var p = FindByCode();
            if (p == null) return;
            action(p);
            if (!string.IsNullOrEmpty(successMsg)) Console.WriteLine($"Product {successMsg}.");
        }

        static void Search()
        {
            Console.WriteLine("1. By code\n2. By name\n3. By category");
            Console.Write("Choose search type: ");

            switch (Console.ReadLine())
            {
                case "1":
                    var product = FindByCode();
                    if (product != null)
                    {
                        Console.WriteLine("\nFound product:");
                        Show(product);
                    }
                    break;
                case "2":
                    string name = Input("Enter name to search: ");
                    var foundByName = products.Where(p => p.Name.Contains(name, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (foundByName.Any())
                    {
                        Console.WriteLine($"\nFound products: {foundByName.Count}");
                        Show(foundByName);
                    }
                    else
                    {
                        Console.WriteLine("Products not found.");
                    }
                    break;
                case "3":
                    int cat = InputInt("Category (0-Electronics, 1-Food, 2-Clothes): ", 0, 2);
                    var foundByCategory = products.Where(p => p.Category == (Category)cat).ToList();
                    if (foundByCategory.Any())
                    {
                        Console.WriteLine($"\nFound products in category {(Category)cat}: {foundByCategory.Count}");
                        Show(foundByCategory);
                    }
                    else
                    {
                        Console.WriteLine("No products found in this category.");
                    }
                    break;
                default: Console.WriteLine("Input error."); break;
            }
        }

        static Product FindByCode()
        {
            try
            {
                int code = InputInt("Enter product code: ");
                var p = products.FirstOrDefault(x => x.Code == code);
                if (p == null) Console.WriteLine("Product not found.");
                return p;
            }
            catch (Exception)
            {
                Console.WriteLine("Code input error.");
                return null;
            }
        }

        static void Show(Product p)
        {
            if (p != null)
                Console.WriteLine(p);
        }

        static void Show(IEnumerable<Product> list)
        {
            foreach (var p in list)
                Console.WriteLine(p); 
        }

        static string Input(string msg)
        {
            Console.Write(msg);
            return Console.ReadLine();
        }

        static int InputInt(string msg, int min = int.MinValue, int max = int.MaxValue)
        {
            while (true)
            {
                Console.Write(msg);
                if (int.TryParse(Console.ReadLine(), out int v) && v >= min && v <= max)
                    return v;
                Console.WriteLine($"Error! Enter a number from {min} to {max}");
            }
        }

        static decimal InputDecimal(string msg)
        {
            while (true)
            {
                Console.Write(msg);
                if (decimal.TryParse(Console.ReadLine(), out decimal v) && v > 0)
                    return v;
                Console.WriteLine("Error! Enter a valid price (positive number)");
            }
        }
    }
}
