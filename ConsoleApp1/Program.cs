using System;
using System.Collections.Generic;
using System.Linq;

namespace DailyExpenses
{
    public class Expense
    {
        public string Name { get; set; }
        public double Amount { get; set; }

        public Expense(string name, double amount)
        {
            Name = name;
            Amount = amount;
        }

        public override string ToString()
        {
            return $"{Name}; {Amount} рубле";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<Expense> expenses = new List<Expense>();
            Console.WriteLine("Введите количество операций (от 2 до 40):");
            int n;
            while (!int.TryParse(Console.ReadLine(), out n) || n < 2 || n > 40)
            {
                Console.WriteLine("Неверное значение. Введите число от 2 до 40:");
            }

            Console.WriteLine("Введите траты по шаблону: (Название; Сумма)");
            for (int i = 0; i < n; i++)
            {
                string line;
                while (true)
                {
                    line = Console.ReadLine().Trim();

                    {

                        string[] parts = line.Split(';');
                        if (parts.Length == 2)
                        {
                            string name = parts[0].Trim();
                            if (double.TryParse(parts[1].Trim(), out double amount) && amount > 0)
                            {
                                expenses.Add(new Expense(name, amount));
                                break;
                            }
                        }
                    }
                    Console.WriteLine("Неверный формат. Введите по шаблону: (Название; Сумма)");
                }
            }

            while (true)
            {
                Console.WriteLine("\nМеню:");
                Console.WriteLine("1. Вывод данных");
                Console.WriteLine("2. Статистика (среднее, максимальное, минимальное, сумма)");
                Console.WriteLine("3. Сортировка по цене (пузырьковая сортировка)");
                Console.WriteLine("4. Конвертация валюты");
                Console.WriteLine("5. Поиск по названию");
                Console.WriteLine("0. Выход");
                Console.Write("Выберите пункт: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.WriteLine("\nДанные:");
                        foreach (var exp in expenses)
                        {
                            Console.WriteLine(exp);
                        }
                        break;
                    case "2":
                        if (expenses.Count > 0)
                        {
                            double sum = expenses.Sum(e => e.Amount);
                            double avg = sum / expenses.Count;
                            double max = expenses.Max(e => e.Amount);
                            double min = expenses.Min(e => e.Amount);
                            Console.WriteLine($"\nСтатистика:");
                            Console.WriteLine($"Сумма: {sum} рубле");
                            Console.WriteLine($"Среднее: {avg:F2} рубле");
                            Console.WriteLine($"Максимум: {max} рубле");
                            Console.WriteLine($"Минимум: {min} рубле");
                        }
                        else
                        {
                            Console.WriteLine("Нет данных.");
                        }
                        break;
                    case "3":
                        BubbleSort(expenses);
                        Console.WriteLine("\nДанные отсортированы по цене (по возрастанию):");
                        foreach (var exp in expenses)
                        {
                            Console.WriteLine(exp);
                        }
                        break;
                    case "4":
                        Console.WriteLine("\nВыберите валюту для конвертации:");
                        Console.WriteLine("1. доллар (курс: 90 рубле за 1 доллар)");
                        Console.WriteLine("2. евро (курс: 100 рубле за 1 евро)");
                        Console.WriteLine("3. Ввести свой курс");
                        Console.Write("Выбор: ");
                        string currChoice = Console.ReadLine();
                        double rate = 1.0;
                        string newCurrency = "";
                        switch (currChoice)
                        {
                            case "1":
                                rate = 90.0;
                                newCurrency = "доллар";
                                break;
                            case "2":
                                rate = 100.0;
                                newCurrency = "евро";
                                break;
                            case "3":
                                Console.Write("Введите курс: ");
                                if (double.TryParse(Console.ReadLine(), out rate) && rate > 0)
                                {
                                    Console.Write("Введите название валюты: ");
                                    newCurrency = Console.ReadLine().Trim();
                                }
                                else
                                {
                                    Console.WriteLine("Неверный курс.");
                                    continue;
                                }
                                break;
                            default:
                                Console.WriteLine("Неверный выбор.");
                                continue;
                        }
                        Console.WriteLine($"\nКонвертированные данные в {newCurrency}:");
                        foreach (var exp in expenses)
                        {
                            double converted = exp.Amount / rate;
                            Console.WriteLine($"{exp.Name}; {converted:F2} {newCurrency}");
                        }
                        break;
                    case "5":
                        Console.Write("Введите название для поиска: ");
                        string keyword = Console.ReadLine().Trim();
                        var found = expenses.Where(e => e.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
                        if (found.Count > 0)
                        {
                            Console.WriteLine("\nНайденные траты:");
                            foreach (var exp in found)
                            {
                                Console.WriteLine(exp);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Ничего не найдено.");
                        }
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Неверный выбор.");
                        break;
                }
            }
        }

        static void BubbleSort(List<Expense> list)
        {
            int n = list.Count;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (list[j].Amount > list[j + 1].Amount)
                    {
                        Expense temp = list[j];
                        list[j] = list[j + 1];
                        list[j + 1] = temp;
                    }
                }
            }
        }
    }
}
