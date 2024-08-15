using System;
using System.Runtime.CompilerServices;

namespace WinGL.Util
{
    /// <summary>
    /// Представляет четырехмерный вектор
    /// </summary>
    public struct Vec4
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public float this[int index]
        {
            get
            {
                if (index == 0) return x;
                else if (index == 1) return y;
                else if (index == 2) return z;
                else if (index == 3) return w;
                else throw new Exception("Out of range.");
            }
            set
            {
                if (index == 0) x = value;
                else if (index == 1) y = value;
                else if (index == 2) z = value;
                else if (index == 3) w = value;
                else throw new Exception("Out of range.");
            }
        }

        public Vec4(float s)
        {
            x = y = z = w = s;
        }

        public Vec4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Vec4(Vec4 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
            w = v.w;
        }

        public Vec4(Vec3 xyz, float w)
        {
            x = xyz.x;
            y = xyz.y;
            z = xyz.z;
            this.w = w;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec4 operator +(Vec4 lhs, Vec4 rhs)
            => new Vec4(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z, lhs.w + rhs.w);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec4 operator +(Vec4 lhs, float rhs)
            => new Vec4(lhs.x + rhs, lhs.y + rhs, lhs.z + rhs, lhs.w + rhs);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec4 operator -(Vec4 lhs, float rhs)
            => new Vec4(lhs.x - rhs, lhs.y - rhs, lhs.z - rhs, lhs.w - rhs);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec4 operator -(Vec4 lhs, Vec4 rhs)
            => new Vec4(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z, lhs.w - rhs.w);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec4 operator *(Vec4 self, float s)
            => new Vec4(self.x * s, self.y * s, self.z * s, self.w * s);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec4 operator *(float lhs, Vec4 rhs)
            => new Vec4(rhs.x * lhs, rhs.y * lhs, rhs.z * lhs, rhs.w * lhs);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec4 operator *(Vec4 lhs, Vec4 rhs)
            => new Vec4(rhs.x * lhs.x, rhs.y * lhs.y, rhs.z * lhs.z, rhs.w * lhs.w);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec4 operator /(Vec4 lhs, float rhs)
            => new Vec4(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs, lhs.w / rhs);

        public float[] ToArray() => new[] { x, y, z, w };

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
            if (obj.GetType() == typeof(Vec4))
            {
                var vec = (Vec4)obj;
                if (x == vec.x && y == vec.y && z == vec.z && w == vec.w)
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
        public static bool operator ==(Vec4 v1, Vec4 v2)
        {
            return v1.Equals(v2);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="v1">The first Vector.</param>
        /// <param name="v2">The second Vector.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Vec4 v1, Vec4 v2)
        {
            return !v1.Equals(v2);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode() ^ w.GetHashCode();
        }

        public override string ToString()
            => "[" + x + ", " + y + ", " + z + ", " + w + "]";
    }
}
