using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class TextStatistics
{
    public string OriginalText;
    public int WordCount;
    public string ShortestWord;
    public string LongestWord;
    public int SentenceCount;
    public int VowelCount;
    public int ConsonantCount;
    public Dictionary<char, int> LetterFrequency;

    public void Print()
    {
        Console.WriteLine($"Количество слов: {WordCount}");
        Console.WriteLine($"Самое короткое слово: {ShortestWord}");
        Console.WriteLine($"Самое длинное слово: {LongestWord}");
        Console.WriteLine($"Количество предложений: {SentenceCount}");
        Console.WriteLine($"Гласных: {VowelCount}");
        Console.WriteLine($"Согласных: {ConsonantCount}");
        Console.WriteLine("Частота букв:");

        foreach (var pair in LetterFrequency.OrderBy(x => x.Key))
        {
            Console.WriteLine($"{pair.Key} : {pair.Value}");
        }
    }
}

class Program
{
    static char[] vowels = { 'а','е','ё','и','о','у','ы','э','ю','я'};
    static char[] consonants = { 'б','в','г','д','ж','з','й','к','л','м','н','п','р','с','т','ф','х','ц','ч','ш','щ'};

    static void Main()
    {


        List<TextStatistics> history = new List<TextStatistics>();

        while (true)
        {
            Console.WriteLine("1. Ввести новый текст");
            Console.WriteLine("2. Показать всю статистику по прошлым текстам");
            Console.WriteLine("3. Выход");
            Console.Write("Выберите пункт: ");
            string choice = Console.ReadLine();

            if (choice == "1")
            {
                Console.WriteLine("\nВведите текст (минимум 100 символов):");
                StringBuilder inputBuilder = new StringBuilder();
                string line;
                int totalChars = 0;

                while (totalChars < 100)
                {
                    line = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        Console.WriteLine("Нужно ввести хотя бы 100 символов. Продолжайте ввод.");
                        continue;
                    }

                    inputBuilder.AppendLine(line);
                    totalChars += line.Length;

                    if (totalChars >= 100)
                        break;

                    Console.WriteLine($"Введено {totalChars}/100 символов. Продолжайте:");
                }

                string input = inputBuilder.ToString().Trim();

                TextStatistics stats = AnalyzeText(input);
                history.Add(stats);

                Console.WriteLine("\nРезультат анализа:");
                stats.Print();
                Console.WriteLine();
            }
            else if (choice == "2")
            {
                if (history.Count == 0)
                {
                    Console.WriteLine("История пуста.\n");
                }
                else
                {
                    for (int i = 0; i < history.Count; i++)
                    {
                        Console.WriteLine($"Текст #{i + 1}:");
                        history[i].Print();
                        Console.WriteLine();
                    }
                }
            }
            else if (choice == "3")
            {
                Console.WriteLine("Выход из программы...");
                break;
            }
            else
            {
                Console.WriteLine("Неверный ввод. Попробуйте снова.\n");
            }
        }
    }

    static TextStatistics AnalyzeText(string text)
    {
        TextStatistics stats = new TextStatistics();
        stats.OriginalText = text;
        stats.LetterFrequency = new Dictionary<char, int>();

        char[] wordSeparators = { ' ', '\n', ',', '.', '!', '?', ';', ':', '(', ')', '[', ']', '{', '}', '"', '–', };
        string[] words = text.Split(wordSeparators, StringSplitOptions.RemoveEmptyEntries)
                            .Where(word => word.Any(char.IsLetter))
                            .ToArray();

        stats.WordCount = words.Length;

        if (words.Length > 0)
        {
            stats.ShortestWord = words[0];
            stats.LongestWord = words[0];

            foreach (string word in words)
            {
                string cleanWord = new string(word.Where(char.IsLetter).ToArray());

                if (string.IsNullOrEmpty(cleanWord))
                    continue;

                if (cleanWord.Length < stats.ShortestWord.Length)
                    stats.ShortestWord = cleanWord;
                if (cleanWord.Length > stats.LongestWord.Length)
                    stats.LongestWord = cleanWord;
            }
        }


        stats.SentenceCount = 0;
        bool inSentence = false;

        for (int i = 0; i < text.Length; i++)
        {
            char ch = text[i];

            if (char.IsLetter(ch) && !inSentence)
            {
                inSentence = true;
                stats.SentenceCount++;
            }
            else if ((ch == '.' || ch == '!' || ch == '?') && inSentence)
            {
                if (i == text.Length - 1 || !char.IsLetter(text[i + 1]))
                {
                    inSentence = false;
                }
            }
        }

        stats.VowelCount = 0;
        stats.ConsonantCount = 0;

        foreach (char ch in text)
        {
            char lowerCh = char.ToLower(ch);

            if (Array.IndexOf(vowels, lowerCh) >= 0)
                stats.VowelCount++;
            else if (Array.IndexOf(consonants, lowerCh) >= 0)
                stats.ConsonantCount++;

            if (char.IsLetter(lowerCh))
            {
                if (!stats.LetterFrequency.ContainsKey(lowerCh))
                    stats.LetterFrequency[lowerCh] = 0;
                stats.LetterFrequency[lowerCh]++;
            }
        }

        return stats;
    }
}

