using System;
using System.Runtime.CompilerServices;

namespace Vge.Util
{
    /// <summary>
    /// Генератор случайных чисел взят с Java сборка jdk 1.8
    /// В разы быстрее чем System.Random
    /// </summary>
    public class Rand
    {
        private readonly AtomicLong _seed;
        private static readonly long _multiplier = 0x5DEECE66DL;
        private static readonly long _addend = 0xBL;
        private static readonly long _mask = (1L << 48) - 1;
        private static readonly double _doubleUnit = 1.110223024625156540e-16; // 0x1.0p-53 || 1.0 / (1L << 53)
        private static AtomicLong _seedUniquifierLong => new AtomicLong(8682522807148012L);

        private float _nextNextGaussian;
        private bool _haveNextNextGaussian;

        public Rand() : this(SeedUniquifier() ^ Environment.TickCount) { }
        public Rand(long seed)
        {
            _seed = new AtomicLong();
            SetSeed(seed);
        }

        private static long SeedUniquifier()
        {
            long current = _seedUniquifierLong.value;
            long next;
            // L'Ecuyer, "Tables of Linear Congruential Generators of
            // Different Sizes and Good Lattice Structure", 1999
            for (; ; )
            {
                next = current * 181783497276652981L;
                if (_seedUniquifierLong.CompareAndSet(current, next)) return next;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long InitialScramble(long seed) => (seed ^ _multiplier) & _mask;

        public Rand SetSeed(long seed)
        {
            _seed.value = InitialScramble(seed);
            _haveNextNextGaussian = false;
            return this;
        }

        private int _InternalSample(int bits)
        {
            long oldseed, nextseed;
            AtomicLong seed = _seed;
            do
            {
                oldseed = seed.value;
                nextseed = (oldseed * _multiplier + _addend) & _mask;
            } while (!seed.CompareAndSet(oldseed, nextseed));
            return (int)((ulong)nextseed >> (48 - bits));
        }

        /// <summary>
        /// An int [0..Int32.MaxValue)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int Next() => _InternalSample(32);

        /// <summary>
        /// An int [0..maxValue)
        /// </summary>
        public int Next(int bound)
        {
            if (bound <= 0) throw new Exception(Sr.TheValueMustBeGreaterThanZero);

            int r = _InternalSample(31);
            int m = bound - 1;
            if ((bound & m) == 0)
            {
                // i.e., bound is a power of 2
                r = (int)((bound * (long)r) >> 31);
            }
            else
            {
                for (int u = r; u - (r = u % bound) + m < 0; u = _InternalSample(31)) ;
            }
            return r;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long NextLong() => ((long)_InternalSample(32) << 32) + _InternalSample(32);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool NextBool() => _InternalSample(1) != 0;

        /// <summary>
        /// A float [0..1)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float NextFloat() => _InternalSample(24) / ((float)(1 << 24));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double NextDouble() => (((long)_InternalSample(26) << 27) + _InternalSample(27)) * _doubleUnit;

        public float NextGaussian()
        {
            // See Knuth, ACP, Section 3.4.1 Algorithm C.
            if (_haveNextNextGaussian)
            {
                _haveNextNextGaussian = false;
                return _nextNextGaussian;
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
                _nextNextGaussian = v2 * multiplier;
                _haveNextNextGaussian = true;
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
