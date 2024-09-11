using System;
using System.Runtime.CompilerServices;
//using System.Runtime.InteropServices;

namespace WinGL.Util
{
    /// <summary>
    /// Представляет двумерный вектор
    /// </summary>
    public struct Vector2i
    {
        public int X;
        public int Y;

        public int this[int index]
        {
            get
            {
                if (index == 0) return X;
                else if (index == 1) return Y;
                else throw new Exception(Sr.OutOfRange);
            }
            set
            {
                if (index == 0) X = value;
                else if (index == 1) Y = value;
                else throw new Exception(Sr.OutOfRange);
            }
        }

        public Vector2i(int s) => X = Y = s;

        public Vector2i(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Vector2i(Vector2i v)
        {
            X = v.X;
            Y = v.Y;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2i operator +(Vector2i lhs, Vector2i rhs)
            => new Vector2i(lhs.X + rhs.X, lhs.Y + rhs.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2i operator +(Vector2i lhs, int rhs)
            => new Vector2i(lhs.X + rhs, lhs.Y + rhs);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2i operator -(Vector2i lhs, Vector2i rhs)
            => new Vector2i(lhs.X - rhs.X, lhs.Y - rhs.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2i operator -(Vector2i lhs, int rhs)
            => new Vector2i(lhs.X - rhs, lhs.Y - rhs);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2i operator *(Vector2i self, int s)
            => new Vector2i(self.X * s, self.Y * s);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2i operator *(int s, Vector2i self)
            => new Vector2i(self.X * s, self.Y * s);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2i operator *(Vector2i lhs, Vector2i rhs)
            => new Vector2i(rhs.X * lhs.X, rhs.Y * lhs.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2i operator /(Vector2i lhs, int rhs)
            => new Vector2i(lhs.X / rhs, lhs.Y / rhs);

        public int[] ToArray() => new[] { X, Y };

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// The Difference is detected by the different values
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Vector2i))
            {
                var vec = (Vector2i)obj;
                if (X == vec.X && Y == vec.Y)
                    return true;
            }

            return false;
        }
        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="v1">The first Vector.</param>
        /// <param name="v2">The second Vector.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Vector2i v1, Vector2i v2) => v1.Equals(v2);

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="v1">The first Vector.</param>
        /// <param name="v2">The second Vector.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Vector2i v1, Vector2i v2) => !v1.Equals(v2);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode();

        public override string ToString() => X + "; " + Y;
    }
}
