using System;

namespace WinGL.Util
{
    public static partial class Glm
    {
        private static float[] sinTable = new float[65536];

        public static void Initialized()
        {
            for (int i = 0; i < 65536; i++)
            {
                sinTable[i] = (float)Math.Sin((float)i * Math.PI * 2.0f / 65536.0f);
            }
        }

        /// <summary>
        /// Четверть Пи 0.79, аналог 45гр
        /// </summary>
        public const float pi45 = 0.7853981625f;
        /// <summary>
        /// Половина Пи 1.57, аналог 90гр
        /// </summary>
        public const float pi90 = 1.570796325f;
        /// <summary>
        /// 3/4 Пи 2.36, аналог 135гр
        /// </summary>
        public const float pi135 = 2.356194487f;
        /// <summary>
        /// Пи 3.14, аналог 180гр
        /// </summary>
        public const float pi = 3.14159265f;
        /// <summary>
        /// 3/2 Пи 4.71, аналог 270гр
        /// </summary>
        public const float pi270 = 4.712388985f;
        /// <summary>
        /// Два Пи 6.28, аналог 360гр
        /// </summary>
        public const float pi360 = 6.28318531f;
        /// <summary>
        /// Множетель для умножения радиан, чтоб получить градусы
        /// </summary>
        public const float degreesInRadians = 57.295779513082320876798154814105f;

        public static float Degrees(float radians)
            => radians * 57.295779513082320876798154814105f;

        public static float Radians(float degrees)
            => degrees * 0.01745329251994329576923690768489f;

        public static float Cos(float angle)
        {
            angle %= pi360;
            return sinTable[(int)(angle * 10430.378f + 16384.0f) & 65535];
        }

        public static float Sin(float angle)
        {
            angle %= pi360;
            return sinTable[(int)(angle * 10430.378f) & 65535];
        }

        public static float Tan(float angle)
        {
            float c = Cos(angle);
            return c == 0 ? float.PositiveInfinity : Sin(angle) / c;
        }

        public static float Atan2(float y, float x) => (float)Math.Atan2(y, x);

        public static float Acos(float x) => (float)Math.Acos(x);

        public static float Asin(float x) => (float)Math.Asin(x);

        /// <summary>
        /// Перевести угол в радианах в -180 .. 180
        /// </summary>
        public static float WrapAngleToPi(float angle)
        {
            angle %= pi360;
            if (angle >= pi) angle -= pi360;
            if (angle < -pi) angle += pi360;
            return angle;
        }
    }
}
