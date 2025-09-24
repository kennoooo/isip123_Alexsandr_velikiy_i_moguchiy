using System;
using System.Collections.Generic;
using System.Linq;

namespace DailyExpenses
{
    public class Expense
    {
        public string Name;
        public double Amount;

        public Expense(string name, double amount)
        {
            Name = name;
            Amount = amount;
        }

        public override string ToString()
        {
            return $"{Name}; {Amount} руб.";
        }
    }

    class Program
    {
        static List<Expense> expenses = new List<Expense>();

        static void Main(string[] args)
        {
            InputExpenses();
            ShowMenu();
        }

        static void InputExpenses()
        {
            Console.WriteLine("Сколько трат записать? (2-40):");
            int count = ReadNumber(2, 40);

            Console.WriteLine("Введите траты в формате: Название; Сумма");
            for (int i = 0; i < count; i++)
            {
                expenses.Add(ReadExpense());
            }
        }

        static int ReadNumber(int min, int max)
        {
            int number;
            while (!int.TryParse(Console.ReadLine(), out number) || number < min || number > max)
            {
                Console.WriteLine($"Введите число от {min} до {max}:");
            }
            return number;
        }

        static Expense ReadExpense()
        {
            while (true)
            {
                string[] parts = Console.ReadLine().Split(';');

                if (parts.Length == 2)
                {
                    string name = parts[0].Trim();
                    if (double.TryParse(parts[1].Trim(), out double amount) && amount > 0)
                    {
                        return new Expense(name, amount);
                    }
                }
                Console.WriteLine("Ошибка! Формат: Название; Сумма");
            }
        }

        static void ShowMenu()
        {
            while (true)
            {
                Console.WriteLine("\nМеню:");
                Console.WriteLine("1 - Список трат");
                Console.WriteLine("2 - Статистика");
                Console.WriteLine("3 - Сортировка по сумме");
                Console.WriteLine("4 - Конвертация валюты");
                Console.WriteLine("5 - Поиск по названию");
                Console.WriteLine("0 - Выход");
                Console.WriteLine("Выберите: ");

                switch (Console.ReadLine())
                {
                    case "1": ShowExpenses(); break;
                    case "2": ShowStatistics(); break;
                    case "3": SortExpenses(); break;
                    case "4": ConvertCurrency(); break;
                    case "5": SearchExpenses(); break;
                    case "0": return;
                    default: Console.WriteLine("Неверный выбор!"); break;
                }
            }
        }

        static void ShowExpenses()
        {
            Console.WriteLine("\nВсе траты:");
            expenses.ForEach(Console.WriteLine);
        }

        static void ShowStatistics()
        {
            if (!expenses.Any())
            {
                Console.WriteLine("Нет данных.");
                return;
            }

            double sum = expenses.Sum(e => e.Amount);
            double avg = expenses.Average(e => e.Amount);
            double max = expenses.Max(e => e.Amount);
            double min = expenses.Min(e => e.Amount);

            Console.WriteLine($"\nСтатистика:");
            Console.WriteLine($"Сумма: {sum} руб.");
            Console.WriteLine($"Среднее: {avg:F2} руб.");
            Console.WriteLine($"Максимум: {max} руб.");
            Console.WriteLine($"Минимум: {min} руб.");
        }

        static void SortExpenses()
        {
            expenses = expenses.OrderBy(e => e.Amount).ToList();
            Console.WriteLine("\nОтсортировано по сумме:");
            ShowExpenses();
        }

        static void ConvertCurrency()
        {
            Console.WriteLine("\nВалюты:");
            Console.WriteLine("1 - Доллар (90 руб.)");
            Console.WriteLine("2 - Евро (100 руб.)");
            Console.WriteLine("3 - Свой курс");
            Console.WriteLine("Выберите: ");

            double rate;
            string currency;

            switch (Console.ReadLine())
            {
                case "1": rate = 90; currency = "долл."; break;
                case "2": rate = 100; currency = "евро"; break;
                case "3":
                    Console.WriteLine("Курс: ");
                    rate = ReadNumber(1, 1000);
                    Console.WriteLine("Валюта: ");
                    currency = Console.ReadLine();
                    break;
                default:
                    Console.WriteLine("Неверный выбор!");
                    return;
            }

            Console.WriteLine($"\nВ {currency}:");
            foreach (var exp in expenses)
            {
                Console.WriteLine($"{exp.Name}; {exp.Amount / rate:F2} {currency}");
            }
        }

        static void SearchExpenses()
        {
            Console.WriteLine("Поиск: ");
            string search = Console.ReadLine().ToLower();

            var found = expenses.Where(e => e.Name.ToLower().Contains(search)).ToList();

            if (found.Any())
            {
                Console.WriteLine("\nНайдено:");
                found.ForEach(Console.WriteLine);
            }
            else
            {
                Console.WriteLine("Не найдено.");
            }
        }
    }
}
