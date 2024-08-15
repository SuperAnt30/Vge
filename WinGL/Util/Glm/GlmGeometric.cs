namespace WinGL.Util
{
    public static partial class Glm
    {
        public static Vec3 Cross(Vec3 lhs, Vec3 rhs)
        {
            return new Vec3(
                lhs.y * rhs.z - rhs.y * lhs.z,
                lhs.z * rhs.x - rhs.z * lhs.x,
                lhs.x * rhs.y - rhs.x * lhs.y);
        }

        public static float Dot(Vec2 x, Vec2 y)
        {
            Vec2 tmp = new Vec2(x * y);
            return tmp.x + tmp.y;
        }

        public static float Dot(Vec3 x, Vec3 y)
        {
            Vec3 tmp = new Vec3(x * y);
            return tmp.x + tmp.y + tmp.z;
        }

        public static float Dot(Vec4 x, Vec4 y)
        {
            Vec4 tmp = new Vec4(x * y);
            return tmp.x + tmp.y + tmp.z + tmp.w;
        }

        /// <summary>
        /// Получить растояние между двумя точками
        /// </summary>
        public static float Distance(Vec3 v1, Vec3 v2)
        {
            float var2 = v1.x - v2.x;
            float var4 = v1.y - v2.y;
            float var6 = v1.z - v2.z;
            return Mth.Sqrt(var2 * var2 + var4 * var4 + var6 * var6);
        }
        /// <summary>
        /// Получить растояние между двумя точками
        /// </summary>
        public static float Distance(Vec3i v1, Vec3i v2)
        {
            int var2 = v1.x - v2.x;
            int var4 = v1.y - v2.y;
            int var6 = v1.z - v2.z;
            return Mth.Sqrt(var2 * var2 + var4 * var4 + var6 * var6);
        }

        /// <summary>
        /// Получить растояние вектора
        /// </summary>
        public static float Distance(Vec3 v1) => Mth.Sqrt(v1.x * v1.x + v1.y * v1.y + v1.z * v1.z);

        /// <summary>
        /// Получить растояние между двумя точками
        /// </summary>
        public static float Distance(Vec2 v1, Vec2 v2)
        {
            float var2 = v1.x - v2.x;
            float var4 = v1.y - v2.y;
            return Mth.Sqrt(var2 * var2 + var4 * var4);
        }

        /// <summary>
        /// Получить растояние вектора
        /// </summary>
        public static float Distance(Vec2 v1) => Mth.Sqrt(v1.x * v1.x + v1.y * v1.y);

        public static Vec2 Normalize(Vec2 v)
        {
            float sqr = v.x * v.x + v.y * v.y;
            return v * (1.0f / Mth.Sqrt(sqr));
        }

        public static Vec3 Normalize(Vec3 v)
        {
            float sqr = v.x * v.x + v.y * v.y + v.z * v.z;
            return v * (1.0f / Mth.Sqrt(sqr));
        }

        public static Vec4 Normalize(Vec4 v)
        {
            float sqr = v.x * v.x + v.y * v.y + v.z * v.z + v.w * v.w;
            return v * (1.0f / Mth.Sqrt(sqr));
        }

        /// <summary>
        /// Вращение точки вокруг оси координат вокруг вектора
        /// </summary>
        /// <param name="pos">позиция точки</param>
        /// <param name="angle">угол</param>
        /// <param name="vec">вектор</param>
        public static Vec3 Rotate(Vec3 pos, float angle, Vec3 vec)
        {
            Mat4 rotat = Rotate(new Mat4(1.0f), angle, vec);
            Mat4 res = Translate(rotat, pos);
            return new Vec3(res);
        }
    }
}
