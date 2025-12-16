using Model;
using System;

namespace Model
{
    public static class RandomChoice
    {
        private static readonly Random _random = new Random();

        public static int Next(int minValue, int maxValue)
        {
            {
                return _random.Next(minValue, maxValue);
            }
        }

        public static int Next(int maxValue)
        {
            {
                return _random.Next(maxValue);
            }
        }

        public static double NextDouble()
        {
            {
                return _random.NextDouble();
            }
        }

        public static int Range(int minValue, int maxValue)
        {
            {
                return _random.Next(minValue, maxValue + 1);
            }
        }

        public static bool Chance(double probability)
        {
            {
                return _random.NextDouble() < probability;
            }
        }

        public static T Choose<T>(T[] items)
        {
            {
                return items[_random.Next(items.Length)];
            }
        }
    }
}