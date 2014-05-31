using System;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace AsyncRPGSharedLib.Utility
{
    public class Range<T> where T : IComparable
    {
        public T Min { get; private set; }
        public T Max { get; private set; }

        public Range(T x0, T x1)
        {
            this.Min = (x0.CompareTo(x1) >= 0) ? x0 : x1;
            this.Max = (x1.CompareTo(x0) >= 0) ? x1 : x0;
        }
    }

    public class RNGUtilities
    {
        private static Random random = new Random((int)DateTime.Now.Ticks);

        public static bool RandomBool(double chanceTrue)
        {
            return RNGUtilities.random.NextDouble() <= chanceTrue;
        }

        public static bool RandomBool(Random rng, double chanceTrue)
        {
            return rng.NextDouble() <= chanceTrue;
        }

        public static int RandomInt(int minValue, int maxValue)
        {
            return RNGUtilities.random.Next(minValue, maxValue + 1);
        }

        public static int RandomInt(Random rng, int minValue, int maxValue)
        {
            return rng.Next(minValue, maxValue + 1);
        }

        public static float RandomFloat(float minValue, float maxValue)
        {
            return (float)RNGUtilities.random.NextDouble() * (maxValue - minValue) + minValue;
        }

        public static float RandomFloat(Random rng, float minValue, float maxValue)
        {
            return (float)rng.NextDouble() * (maxValue - minValue) + minValue;
        }

        public static string CreateNonDeterministicRandomBase64String(int bit_count)
        {
            int k_random_bytes = bit_count / 8;
            byte[] randomBytes = new byte[k_random_bytes];

            try
            {
                RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();

                random.GetNonZeroBytes(randomBytes);
            }
            catch (System.Exception)
            {
                Random random = new Random((int)DateTime.Now.Ticks);

                random.NextBytes(randomBytes);
            }

            return Convert.ToBase64String(randomBytes);
        }

        public static void DeterministicKnuthShuffle<T>(int seed, IList<T> list)
        {
            DeterministicKnuthShuffle(new Random(seed), list);
        }

        public static void DeterministicKnuthShuffle<T>(Random rng, IList<T> list)
        {
            int n = list.Count - 1;
            while (n > 1)
            {
                int k = rng.Next(n + 1);

                T value = list[k];
                list[k] = list[n];
                list[n] = value;

                n--;
            }
        }

    }
}
