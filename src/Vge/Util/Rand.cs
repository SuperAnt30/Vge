using System;

namespace Vge.Util
{
    /// <summary>
    /// Генератор случайных чисел взят с Java сборка jdk 1.8
    /// В разы быстрее чем System.Random
    /// </summary>
    public class Rand
    {
        private readonly AtomicLong seed;
        private static readonly long multiplier = 0x5DEECE66DL;
        private static readonly long addend = 0xBL;
        private static readonly long mask = (1L << 48) - 1;
        private static readonly double doubleUnit = 1.110223024625156540e-16; // 0x1.0p-53 || 1.0 / (1L << 53)
        private static AtomicLong seedUniquifierLong => new AtomicLong(8682522807148012L);

        private float nextNextGaussian;
        private bool haveNextNextGaussian = false;

        public Rand() : this(SeedUniquifier() ^ Environment.TickCount) { }
        public Rand(long seed)
        {
            this.seed = new AtomicLong();
            SetSeed(seed);
        }

        private static long SeedUniquifier()
        {
            long current = seedUniquifierLong.value;
            long next;
            // L'Ecuyer, "Tables of Linear Congruential Generators of
            // Different Sizes and Good Lattice Structure", 1999
            for (; ; )
            {
                next = current * 181783497276652981L;
                if (seedUniquifierLong.CompareAndSet(current, next)) return next;
            }
        }

        private static long InitialScramble(long seed) => (seed ^ multiplier) & mask;

        public Rand SetSeed(long seed)
        {
            this.seed.value = InitialScramble(seed);
            haveNextNextGaussian = false;
            return this;
        }

        private int InternalSample(int bits)
        {
            long oldseed, nextseed;
            AtomicLong seed = this.seed;
            do
            {
                oldseed = seed.value;
                nextseed = (oldseed * multiplier + addend) & mask;
            } while (!seed.CompareAndSet(oldseed, nextseed));
            return (int)((ulong)nextseed >> (48 - bits));
        }

        /// <summary>
        /// An int [0..Int32.MaxValue)
        /// </summary>
        public int Next() => InternalSample(32);

        /// <summary>
        /// An int [0..maxValue)
        /// </summary>
        public int Next(int bound)
        {
            if (bound <= 0) throw new Exception(Sr.TheValueMustBeGreaterThanZero);

            int r = InternalSample(31);
            int m = bound - 1;
            if ((bound & m) == 0)
            {
                // i.e., bound is a power of 2
                r = (int)((bound * (long)r) >> 31);
            }
            else
            {
                for (int u = r; u - (r = u % bound) + m < 0; u = InternalSample(31)) ;
            }
            return r;
        }

        public long NextLong() => ((long)InternalSample(32) << 32) + InternalSample(32);

        public bool NextBool() => InternalSample(1) != 0;

        /// <summary>
        /// A float [0..1)
        /// </summary>
        public float NextFloat() => InternalSample(24) / ((float)(1 << 24));

        public double NextDouble() => (((long)InternalSample(26) << 27) + InternalSample(27)) * doubleUnit;

        public float NextGaussian()
        {
            // See Knuth, ACP, Section 3.4.1 Algorithm C.
            if (haveNextNextGaussian)
            {
                haveNextNextGaussian = false;
                return nextNextGaussian;
            }
            else
            {
                float v1, v2, s;
                do
                {
                    v1 = 2 * NextFloat() - 1; // between -1 and 1
                    v2 = 2 * NextFloat() - 1; // between -1 and 1
                    s = v1 * v1 + v2 * v2;
                } while (s >= 1 || s == 0);
                float multiplier = (float)Math.Sqrt(-2 * Math.Log(s) / s);
                nextNextGaussian = v2 * multiplier;
                haveNextNextGaussian = true;
                return v1 * multiplier;
            }
        }

        public void NextBytes(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)Next();
            }
        }

        private class AtomicLong
        {
            public long value;

            public AtomicLong() { }
            public AtomicLong(long initialValue) => value = initialValue;

            public bool CompareAndSet(long expect, long update)
            {
                if (value == expect)
                {
                    value = update;
                    return true;
                }
                return false;
            }
        }
    }
}
