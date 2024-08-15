using System;
using System.Runtime.CompilerServices;
//using System.Runtime.InteropServices;

namespace WinGL.Util
{
    /// <summary>
    /// Представляет двумерный вектор
    /// </summary>
    public struct Vec2i
    {
        public int x;
        public int y;

        public int this[int index]
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

        public Vec2i(int s) => x = y = s;

        public Vec2i(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Vec2i(Vec2i v)
        {
            x = v.x;
            y = v.y;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2i operator +(Vec2i lhs, Vec2i rhs)
            => new Vec2i(lhs.x + rhs.x, lhs.y + rhs.y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2i operator +(Vec2i lhs, int rhs)
            => new Vec2i(lhs.x + rhs, lhs.y + rhs);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2i operator -(Vec2i lhs, Vec2i rhs)
            => new Vec2i(lhs.x - rhs.x, lhs.y - rhs.y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2i operator -(Vec2i lhs, int rhs)
            => new Vec2i(lhs.x - rhs, lhs.y - rhs);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2i operator *(Vec2i self, int s)
            => new Vec2i(self.x * s, self.y * s);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2i operator *(int s, Vec2i self)
            => new Vec2i(self.x * s, self.y * s);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2i operator *(Vec2i lhs, Vec2i rhs)
            => new Vec2i(rhs.x * lhs.x, rhs.y * lhs.y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec2i operator /(Vec2i lhs, int rhs)
            => new Vec2i(lhs.x / rhs, lhs.y / rhs);

        public int[] ToArray() => new[] { x, y };

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
            if (obj.GetType() == typeof(Vec2i))
            {
                var vec = (Vec2i)obj;
                if (x == vec.x && y == vec.y)
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
        public static bool operator ==(Vec2i v1, Vec2i v2) => v1.Equals(v2);

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="v1">The first Vector.</param>
        /// <param name="v2">The second Vector.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Vec2i v1, Vec2i v2) => !v1.Equals(v2);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => x.GetHashCode() ^ y.GetHashCode();

        public override string ToString() => x + "; " + y;
    }
}
