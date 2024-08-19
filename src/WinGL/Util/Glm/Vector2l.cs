using System;

namespace WinGL.Util
{
    /// <summary>
    /// Представляет двумерный вектор с 64 разнрядным числом
    /// </summary>
    public struct Vector2l
    {
        public long X;
        public long Y;

        public long this[int index]
        {
            get
            {
                if (index == 0) return X;
                else if (index == 1) return Y;
                else throw new Exception("Out of range.");
            }
            set
            {
                if (index == 0) X = value;
                else if (index == 1) Y = value;
                else throw new Exception("Out of range.");
            }
        }

        public Vector2l(long x, long y)
        {
            X = x;
            Y = y;
        }

        public override string ToString() => X + "; " + Y;
    }
}
