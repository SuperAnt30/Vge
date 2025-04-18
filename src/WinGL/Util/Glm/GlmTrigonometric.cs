﻿using System;

namespace WinGL.Util
{
    public static partial class Glm
    {
        private static float[] _sinTable = new float[65536];

        public static void Initialized()
        {
            for (int i = 0; i < 65536; i++)
            {
                _sinTable[i] = (float)Math.Sin(i * Pi360 / 65536f);
            }
        }

        /// <summary>
        /// Четверть Пи 0.79, аналог 45гр
        /// </summary>
        public const float Pi45 = 0.7853982f;
        /// <summary>
        /// Половина Пи 1.57, аналог 90гр
        /// </summary>
        public const float Pi90 = 1.57079637f;
        /// <summary>
        /// 3/4 Пи 2.36, аналог 135гр
        /// </summary>
        public const float Pi135 = 2.3561945f;
        /// <summary>
        /// Пи 3.14, аналог 180гр
        /// </summary>
        public const float Pi = 3.14159274f;
        /// <summary>
        /// 3/2 Пи 4.71, аналог 270гр
        /// </summary>
        public const float Pi270 = 4.712389f;
        /// <summary>
        /// Два Пи 6.28, аналог 360гр
        /// </summary>
        public const float Pi360 = 6.28318548f;
        /// <summary>
        /// Множетель для умножения радиан, чтоб получить градусы
        /// </summary>
        public const float DegreesInRadians = 57.29578f;

        public static float Degrees(float radians)
            => radians * 57.295779513082320876798154814105f;

        public static float Radians(float degrees)
            => degrees * 0.01745329251994329576923690768489f;

        public static float Cos(float angle)
        {
            angle %= Pi360;
            return _sinTable[(int)(angle * 10430.378f + 16384.0f) & 65535];
        }

        public static float Sin(float angle)
        {
            angle %= Pi360;
            return _sinTable[(int)(angle * 10430.378f) & 65535];
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
            angle %= Pi360;
            if (angle >= Pi) angle -= Pi360;
            if (angle < -Pi) angle += Pi360;
            return angle;
        }

        /// <summary>
        /// Получить вектор направления по поворотам
        /// </summary>
        public static Vector3 Ray(float yaw, float pitch)
        {
            //float var3 = glm.cos(-yaw - glm.pi);
            //float var4 = glm.sin(-yaw - glm.pi);
            //float var5 = -glm.cos(-pitch);
            //float var6 = glm.sin(-pitch);
            //return new vec3(var4 * var5, var6, var3 * var5);

            float pitchxz = Cos(pitch);
            return new Vector3(Sin(yaw) * pitchxz, Sin(pitch), -Cos(yaw) * pitchxz);
        }
    }
}
