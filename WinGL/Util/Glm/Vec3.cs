using System;
using System.Runtime.CompilerServices;

namespace WinGL.Util
{
    /// <summary>
    /// Представляет трехмерный вектор
    /// </summary>
    public struct Vec3
    {
        public float x;
        public float y;
        public float z;

        public float this[int index]
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

        public Vec3(float s)
        {
            x = y = z = s;
        }

        public Vec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vec3(Vec3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public Vec3(Vec3i v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public Vec3(Vec4 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public Vec3(Vec2 xy, float z)
        {
            x = xy.x;
            y = xy.y;
            this.z = z;
        }

        /// <summary>
        /// Получить точку из матрици перемещения, вращения
        /// </summary>
        /// <param name="m4"></param>
        public Vec3(Mat4 m4)
        {
            x = m4[3][0];
            y = m4[3][1];
            z = m4[3][2];
        }

        /// <summary>
        /// Угол по вертикали в радианах
        /// </summary>
        public Vec3 RotatePitch(float angle)
        {
            float c = Glm.Cos(angle);
            float s = Glm.Sin(angle);
            float y2 = y * c + z * s;
            float z2 = z * c - y * s;
            return new Vec3(x, y2, z2);
        }

        /// <summary>
        /// Угол по горизонтали в радианах
        /// </summary>
        public Vec3 RotateYaw(float angle)
        {
            float c = Glm.Cos(angle);
            float s = Glm.Sin(angle);
            float x2 = x * c + z * s;
            float z2 = z * c - x * s;
            return new Vec3(x2, y, z2);
        }

        /// <summary>
        /// Инверсия
        /// </summary>
        public Vec3 Inverse() => this * -1f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 operator +(Vec3 lhs, Vec3 rhs)
            => new Vec3(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 operator +(Vec3 lhs, float rhs)
            => new Vec3(lhs.x + rhs, lhs.y + rhs, lhs.z + rhs);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 operator -(Vec3 lhs, Vec3 rhs)
            => new Vec3(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 operator -(Vec3 lhs, float rhs)
            => new Vec3(lhs.x - rhs, lhs.y - rhs, lhs.z - rhs);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 operator *(Vec3 self, float s)
            => new Vec3(self.x * s, self.y * s, self.z * s);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 operator *(float s, Vec3 self)
            => new Vec3(self.x * s, self.y * s, self.z * s);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 operator /(Vec3 lhs, float rhs)
            => new Vec3(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec3 operator *(Vec3 lhs, Vec3 rhs)
            => new Vec3(rhs.x * lhs.x, rhs.y * lhs.y, rhs.z * lhs.z);

        public float[] ToArray() => new[] { x, y, z };

        public Vec3 Normalize() => Glm.Normalize(this);

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
            if (obj.GetType() == typeof(Vec3))
            {
                var vec = (Vec3)obj;
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
        public static bool operator ==(Vec3 v1, Vec3 v2) => v1.Equals(v2);

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="v1">The first Vector.</param>
        /// <param name="v2">The second Vector.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns> 
        public static bool operator !=(Vec3 v1, Vec3 v2) => !v1.Equals(v2);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
            => x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();

        public override string ToString()
            => string.Format("{0:0.00}; {1:0.00}; {2:0.00}", x, y, z);
    }
}
