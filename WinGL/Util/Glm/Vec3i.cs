using System;
using System.Runtime.CompilerServices;

namespace WinGL.Util
{
    /// <summary>
    /// Представляет трехмерный вектор
    /// </summary>
    public struct Vec3i
    {
        public int x;
        public int y;
        public int z;

        public int this[int index]
        {
            get
            {
                if (index == 0) return x;
                else if (index == 1) return y;
                else if (index == 2) return z;
                else throw new Exception("Out of range.");
            }
            set
            {
                if (index == 0) x = value;
                else if (index == 1) y = value;
                else if (index == 2) z = value;
                else throw new Exception("Out of range.");
            }
        }

        public Vec3i(int s)
        {
            x = y = z = s;
        }

        public Vec3i(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vec3i(Vec3i v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public Vec3i(Vec2i xy, int z)
        {
            x = xy.x;
            y = xy.y;
            this.z = z;
        }

        /// <summary>
        /// Инверсия
        /// </summary>
        public Vec3i Inverse() => this * -1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3i operator +(Vec3i lhs, Vec3i rhs)
            => new Vec3i(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3i operator +(Vec3i lhs, int rhs)
            => new Vec3i(lhs.x + rhs, lhs.y + rhs, lhs.z + rhs);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3i operator -(Vec3i lhs, Vec3i rhs)
            => new Vec3i(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3i operator -(Vec3i lhs, int rhs)
            => new Vec3i(lhs.x - rhs, lhs.y - rhs, lhs.z - rhs);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3i operator *(Vec3i self, int s)
            => new Vec3i(self.x * s, self.y * s, self.z * s);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3i operator *(int s, Vec3i self)
            => new Vec3i(self.x * s, self.y * s, self.z * s);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3i operator /(Vec3i lhs, int rhs)
            => new Vec3i(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3i operator *(Vec3i lhs, Vec3i rhs)
            => new Vec3i(rhs.x * lhs.x, rhs.y * lhs.y, rhs.z * lhs.z);

        public int[] ToArray() => new[] { x, y, z };

        /// <summary>
        /// Равно ли значение нулю
        /// </summary>
        public bool IsZero() => x == 0 && y == 0 && z == 0;

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
            if (obj.GetType() == typeof(Vec3i))
            {
                var vec = (Vec3i)obj;
                if (x == vec.x && y == vec.y && z == vec.z)
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
        public static bool operator ==(Vec3i v1, Vec3i v2) => v1.Equals(v2);

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="v1">The first Vector.</param>
        /// <param name="v2">The second Vector.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns> 
        public static bool operator !=(Vec3i v1, Vec3i v2) => !v1.Equals(v2);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
            => x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();

        public override string ToString() => x + "; " + y + "; " + z;
    }
}
