using System;

namespace WinGL.Util
{
    /// <summary>
    /// Представляет двумерный вектор с 64 разнрядным числом
    /// </summary>
    public struct Vec2l
    {
        public long x;
        public long y;

        public long this[int index]
        {
            get
            {
                if (index == 0) return x;
                else if (index == 1) return y;
                else throw new Exception("Out of range.");
            }
            set
            {
                if (index == 0) x = value;
                else if (index == 1) y = value;
                else throw new Exception("Out of range.");
            }
        }

        public Vec2l(long x, long y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString() => x + "; " + y;
    }
}
