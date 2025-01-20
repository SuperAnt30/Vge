using System.Runtime.CompilerServices;

namespace WinGL.Util
{
    public static partial class Glm
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Cross(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(
                lhs.Y * rhs.Z - rhs.Y * lhs.Z,
                lhs.Z * rhs.X - rhs.Z * lhs.X,
                lhs.X * rhs.Y - rhs.X * lhs.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(Vector2 x, Vector2 y)
            => x.X * y.X + x.Y * y.Y;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(Vector3 x, Vector3 y)
            => x.X * y.X + x.Y * y.Y + x.Z * y.Z;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(Vector4 x, Vector4 y)
            => x.X * y.X + x.Y * y.Y + x.Z * y.Z + x.W * y.W;

        /// <summary>
        /// Получить растояние между двумя точками
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(Vector3 v1, Vector3 v2)
        {
            float x = v1.X - v2.X;
            float y = v1.Y - v2.Y;
            float z = v1.Z - v2.Z;
            return Mth.Sqrt(x * x + y * y + z * z);
        }
        /// <summary>
        /// Получить растояние между двумя точками
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(Vector3i v1, Vector3i v2)
        {
            int x = v1.X - v2.X;
            int y = v1.Y - v2.Y;
            int z = v1.Z - v2.Z;
            return Mth.Sqrt(x * x + y * y + z * z);
        }

        /// <summary>
        /// Получить растояние вектора
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(Vector3 v1) => Mth.Sqrt(v1.X * v1.X + v1.Y * v1.Y + v1.Z * v1.Z);

        /// <summary>
        /// Получить растояние между двумя точками
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(Vector2 v1, Vector2 v2)
        {
            float x = v1.X - v2.X;
            float y = v1.Y - v2.Y;
            return Mth.Sqrt(x * x + y * y);
        }

        /// <summary>
        /// Получить растояние вектора
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(Vector2 v1) => Mth.Sqrt(v1.X * v1.X + v1.Y * v1.Y);

        /// <summary>
        /// Квадрат евклидова расстояния между этим и заданным вектором.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SquareDistanceTo(Vector3 v1, Vector3 v2)
        {
            float vx = v2.X - v1.X;
            float vy = v2.Y - v1.Y;
            float vz = v2.Z - v1.Z;
            return vx * vx + vy * vy + vz * vz;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Normalize(Vector2 v)
        {
            float sqr = v.X * v.X + v.Y * v.Y;
            return v * (1.0f / Mth.Sqrt(sqr));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Normalize(Vector3 v)
        {
            float sqr = v.X * v.X + v.Y * v.Y + v.Z * v.Z;
            return v * (1.0f / Mth.Sqrt(sqr));
        }

        public static Vector4 Normalize(Vector4 v)
        {
            float sqr = v.X * v.X + v.Y * v.Y + v.Z * v.Z + v.W * v.W;
            return v * (1.0f / Mth.Sqrt(sqr));
        }

        /// <summary>
        /// Вращение точки вокруг оси координат вокруг вектора
        /// </summary>
        /// <param name="pos">позиция точки</param>
        /// <param name="angle">угол</param>
        /// <param name="vec">вектор</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Rotate(Vector3 pos, float angle, Vector3 vec)
        {
            Mat4 rotat = Rotate(Mat4.Identity(), angle, vec);
            Mat4 res = Translate(rotat, pos);
            return new Vector3(res);
        }
    }
}
