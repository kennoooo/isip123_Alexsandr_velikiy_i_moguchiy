using Model;
using System;

namespace Model
{
    public static class RandomChoice
    {
        private static readonly Random _random = new Random();
        private static readonly object _lock = new object();

        public static int Next(int minValue, int maxValue)
        {
            lock (_lock)
            {
                return _random.Next(minValue, maxValue);
            }
        }

        public static int Next(int maxValue)
        {
            lock (_lock)
            {
                return _random.Next(maxValue);
            }
        }

        public static double NextDouble()
        {
            lock (_lock)
            {
                return _random.NextDouble();
            }
        }

        public static int Range(int minValue, int maxValue)
        {
            lock (_lock)
            {
                return _random.Next(minValue, maxValue + 1);
            }
        }

        public static bool Chance(double probability)
        {
            lock (_lock)
            {
                return _random.NextDouble() < probability;
            }
        }

        public static T Choose<T>(T[] items)
        {
            lock (_lock)
            {
                return items[_random.Next(items.Length)];
            }
        }
    }
}