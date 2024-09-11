using System;
using System.Runtime.CompilerServices;

namespace WinGL.Util
{
    /// <summary>
    /// Представляет трехмерный вектор
    /// </summary>
    public struct Vector3i
    {
        public int X;
        public int Y;
        public int Z;

        public int this[int index]
        {
            get
            {
                if (index == 0) return X;
                else if (index == 1) return Y;
                else if (index == 2) return Z;
                else throw new Exception(Sr.OutOfRange);
            }
            set
            {
                if (index == 0) X = value;
                else if (index == 1) Y = value;
                else if (index == 2) Z = value;
                else throw new Exception(Sr.OutOfRange);
            }
        }

        public Vector3i(int s)
        {
            X = Y = Z = s;
        }

        public Vector3i(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3i(Vector3i v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }

        public Vector3i(Vector2i xy, int z)
        {
            X = xy.X;
            Y = xy.Y;
            Z = z;
        }

        /// <summary>
        /// Инверсия
        /// </summary>
        public Vector3i Inverse() => this * -1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3i operator +(Vector3i lhs, Vector3i rhs)
            => new Vector3i(lhs.X + rhs.X, lhs.Y + rhs.Y, lhs.Z + rhs.Z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3i operator +(Vector3i lhs, int rhs)
            => new Vector3i(lhs.X + rhs, lhs.Y + rhs, lhs.Z + rhs);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3i operator -(Vector3i lhs, Vector3i rhs)
            => new Vector3i(lhs.X - rhs.X, lhs.Y - rhs.Y, lhs.Z - rhs.Z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3i operator -(Vector3i lhs, int rhs)
            => new Vector3i(lhs.X - rhs, lhs.Y - rhs, lhs.Z - rhs);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3i operator *(Vector3i self, int s)
            => new Vector3i(self.X * s, self.Y * s, self.Z * s);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3i operator *(int s, Vector3i self)
            => new Vector3i(self.X * s, self.Y * s, self.Z * s);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3i operator /(Vector3i lhs, int rhs)
            => new Vector3i(lhs.X / rhs, lhs.Y / rhs, lhs.Z / rhs);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3i operator *(Vector3i lhs, Vector3i rhs)
            => new Vector3i(rhs.X * lhs.X, rhs.Y * lhs.Y, rhs.Z * lhs.Z);

        public int[] ToArray() => new[] { X, Y, Z };

        /// <summary>
        /// Равно ли значение нулю
        /// </summary>
        public bool IsZero() => X == 0 && Y == 0 && Z == 0;

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
            if (obj.GetType() == typeof(Vector3i))
            {
                var vec = (Vector3i)obj;
                if (X == vec.X && Y == vec.Y && Z == vec.Z)
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
        public static bool operator ==(Vector3i v1, Vector3i v2) => v1.Equals(v2);

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="v1">The first Vector.</param>
        /// <param name="v2">The second Vector.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns> 
        public static bool operator !=(Vector3i v1, Vector3i v2) => !v1.Equals(v2);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
            => X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();

        public override string ToString() => X + "; " + Y + "; " + Z;
    }
}
