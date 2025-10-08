using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

enum Genre { Художественная, НонФикшн, Фэнтези, Наука, Детектив }

class Book
{
    static int nextId = 1;
    public int Id;
    public string Title;
    public string Author;
    public Genre Genre;
    public int Year;
    public decimal Price;
    public int Quantity;
    public Book(string t, string a, Genre g, int y, decimal p, int q)
    {
        Id = nextId++; Title = t; Author = a; Genre = g; Year = y; Price = p; Quantity = q;
    }
    public override string ToString() =>
        $"Id:{Id} | \"{Title}\" — {Author} | {Genre} | {Year} г. | {Price:C} | кол-во: {Quantity}";
}

class Program
{
    static List<Book> books = new List<Book>();
    static void Main()
    {

        Seed();
        while (true)
        {
            try
            {
                Console.WriteLine("0 Выход\n1 Добавить\n2 Удалить\n3 Найти\n4 Сортировать\n5 Мин/Макс\n6 По авторам\n7 Показать все");
                Console.Write("Введите команду: ");
                if (!int.TryParse(Console.ReadLine(), out var cmd))
                    continue;
                if (cmd == 0)
                    break;
                switch (cmd)
                {
                    case 1: Add(); break;
                    case 2: Remove(); break;
                    case 3: Find(); break;
                    case 4: SortMenu(); break;
                    case 5: ShowMinMax(); break;
                    case 6: GroupByAuthor(); break;
                    case 7: Print(books); break;
                    default: Console.WriteLine("Неизвестная команда."); break;
                }
            }
            catch (Exception e) { Console.WriteLine("Ошибка: " + e.Message); }
        }
    }

    static void Seed()
    {
        books.AddRange(new[] {
            new Book("Мастер и Маргарита","М. Булгаков",Genre.Художественная,1967,499,3),
            new Book("Краткая история времени","С. Хокинг",Genre.Наука,1988,799,2),
            new Book("Властелин колец","Дж. Р. Р. Толкин",Genre.Фэнтези,1954,1299,5),
            new Book("Приключения Шерлока Холмса","А. Конан Дойл",Genre.Детектив,1905,299,4),
            new Book("Размышления","Марк Аврелий",Genre.НонФикшн,180,199,1)
        });
    }

    static string ReadNonEmpty(string prompt)
    {
        for (; ; )
        {
            Console.Write(prompt); var s = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(s))
                return
                s.Trim(); Console.WriteLine("Значение не может быть пустым.");
        }
    }
    static int ReadInt(string prompt, int min, int max)
    {
        for (; ; )
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out var v) && v >= min && v <= max)
                return
                v; Console.WriteLine($"Введите целое число в диапазоне [{min}..{max}].");
        }
    }
    static int ReadNonNeg(string prompt)
    {
        for (; ; )
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out var v) && v >= 0)
                return
                 v; Console.WriteLine("Введите неотрицательное целое число.");
        }
    }
    static decimal ReadDec(string prompt)
    {
        for (; ; )
        {
            Console.Write(prompt); var s = Console.ReadLine();
            if (TryParseDecimalFlexible(s, out var d) && d >= 0)
                return d; 
            Console.WriteLine("Введите неотрицательное число.");
        }
    }

    static bool TryParseDecimalFlexible(string s, out decimal result)
    {
        result = 0;
        if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.CurrentCulture, out result))
            return
            true;
        var alt = s?.Replace(',', '.');
        return alt != null && decimal.TryParse(alt, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
    }

    static Genre ReadGenre()
    {
        var vals = Enum.GetValues(typeof(Genre)).Cast<Genre>().ToArray();
        for (; ; )
        {
            Console.WriteLine("Выберите жанр:");
            for (int i = 0; i < vals.Length; i++) Console.WriteLine($"{i + 1} - {vals[i]}");
            Console.Write("Номер жанра: ");
            if (int.TryParse(Console.ReadLine(), out var idx) && idx >= 1 && idx <= vals.Length) 
                return vals[idx - 1];
            Console.WriteLine("Неверный выбор жанра.");
        }
    }

    static void Add()
    {
        Console.WriteLine("Добавление книги");
        var t = ReadNonEmpty("Название: ");
        var a = ReadNonEmpty("Автор: ");
        var g = ReadGenre();
        var y = ReadInt("Год издания: ", 1, DateTime.Now.Year);
        var p = ReadDec("Цена: ");
        var q = ReadNonNeg("Количество: ");
        var b = new Book(t, a, g, y, p, q);
        books.Add(b);
        Console.WriteLine("Книга добавлена: " + b);
    }

    static void Remove()
    {
        var id = ReadInt("Id для удаления: ", 1, int.MaxValue);
        var b = books.FirstOrDefault(x => x.Id == id);
        if (b == null) Console.WriteLine("Книга с таким Id не найдена.");
        else { books.Remove(b); Console.WriteLine("Книга удалена."); }
    }

    static void Find()
    {
        Console.WriteLine("1 По названию 2 По автору 3 По жанру");
        Console.Write("Выберите тип поиска: ");
        if (!int.TryParse(Console.ReadLine(), out var t))
            return;
        IEnumerable<Book> res = Enumerable.Empty<Book>();
        if (t == 1)
        {
            var s = ReadNonEmpty("Часть названия: ");
            res = books.Where(b => b.Title.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);
        }
        if (t == 2)
        {
            var s = ReadNonEmpty("Часть имени автора: ");
            res = books.Where(b => b.Author.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);
        }
        if (t == 3)
        {
            var g = ReadGenre();
            res = books.Where(b => b.Genre == g);
        }
        Print(res);
    }

    static void SortMenu()
    {
        Console.WriteLine("1 По названию 2 По году"); Console.Write("Выберите: ");
        if (!int.TryParse(Console.ReadLine(), out var t))
            return;
        Console.WriteLine("1 По возрастанию 2 По убыванию"); Console.Write("Направление: ");
        if (!int.TryParse(Console.ReadLine(), out var d))
            return;
        bool asc = d == 1;
        IEnumerable<Book> outb = t == 1 ? (asc ? books.OrderBy(b => b.Title) : books.OrderByDescending(b => b.Title))
                                      : (asc ? books.OrderBy(b => b.Year) : books.OrderByDescending(b => b.Year));
        Print(outb);
    }

    static void ShowMinMax()
    {
        if (!books.Any())
        {
            Console.WriteLine("Библиотека пуста.");
            return;
        }
        var max = books.OrderByDescending(b => b.Price).First();
        var min = books.OrderBy(b => b.Price).First();
        Console.WriteLine("Самая дорогая книга: " + max);
        Console.WriteLine("Самая дешёвая книга: " + min);
    }

    static void GroupByAuthor()
    {
        var g = books.GroupBy(b => b.Author).Select(gr => new { Author = gr.Key, Count = gr.Count() }).OrderByDescending(x => x.Count);
        Console.WriteLine("Количество книг по авторам");
        foreach (var a in g) Console.WriteLine($"{a.Author} — {a.Count} шт.");
    }

    static void Print(IEnumerable<Book> list)
    {
        var arr = list.ToArray();
        if (!arr.Any()) { Console.WriteLine("Ничего не найдено."); return; }
        Console.WriteLine("Результаты");
        foreach (var b in arr) Console.WriteLine(b);
        Console.WriteLine($"Всего: {arr.Length}");
    }
}
