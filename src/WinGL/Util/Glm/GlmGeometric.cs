using System;
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

        /// <summary>
        /// Скалярное произведение двух векторов возвращает число (скаляр)
        /// Если угол между векторами острый, то скалярное произведение положительно.
        /// Если угол тупой, то скалярное произведение отрицательно.
        /// Если векторы перпендикулярны, то скалярное произведение равно нулю.
        /// Если вектора сошлись то равно 1. (Угол = 0)
        /// Если прямая получилась, то равно -1. (Угол = 180)
        /// </summary>
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        #region Quaternion

        /// <summary>
        /// Конвертируем из углов Эйллера(Тейт–Брайан) в Кватерниона
        /// </summary>
        /// <param name="yaw">По оси Y</param>
        /// <param name="pitch">По оси X</param>
        /// <param name="roll">По оси Z</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 ToQuaternionZYX(float yaw, float pitch, float roll)
        {
            yaw *= .5f;
            pitch *= .5f;
            roll *= .5f;
            // https://github.com/Philip-Trettner/GlmSharp/blob/master/GlmSharp/GlmSharp/Quat/quat.cs
            float cz = Cos(roll);
            float cy = Cos(yaw);
            float cx = Cos(pitch);
            float sz = Sin(roll);
            float sy = Sin(yaw);
            float sx = Sin(pitch);
            return new Vector4(
                sx * cy * cz - cx * sy * sz,
                cx * sy * cz + sx * cy * sz,
                cx * cy * sz - sx * sy * cz,
                cx * cy * cz + sx * sy * sz
            );
        }

        /// <summary>
        /// Конвертируем из углов Эйлера(Тейт–Брайан) в Кватерниона
        /// </summary>
        /// <param name="yaw">По оси Y</param>
        /// <param name="pitch">По оси X</param>
        /// <param name="roll">По оси Z</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 ToQuaternionXYZ(float yaw, float pitch, float roll)
        {
            yaw *= .5f;
            pitch *= .5f;
            roll *= .5f;
            float cx = Cos(pitch);
            float cy = Cos(yaw);
            float cz = Cos(roll);
            float sx = Sin(pitch);
            float sy = Sin(yaw);
            float sz = Sin(roll);
            // XYZ - https://stackoverflow.com/questions/50011864/changing-xyz-order-when-converting-euler-angles-to-quaternions
            return new Vector4(
                sx * cy * cz + cx * sy * sz,
                cx * sy * cz - sx * cy * sz,
                cx * cy * sz + sx * sy * cz,
                cx * cy * cz - sx * sy * sz
            );
        }

        /// <summary>
        /// Конвертируем из угла Кватерниона в углы Эйлера(Тейт–Брайан)  
        /// </summary>
        /// <returns>X=pitch; Y=yaw; Z=roll</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToEulerAnglesZYX(float x, float y, float z, float w)
        {
            // https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
            Vector3 ypr = new Vector3
            {
                // pitch (x-axis rotation)
                X = (float)Math.Atan2(2 * (w * x + y * z), 1 - 2 * (x * x + y * y)),
                // roll (z-axis rotation)
                Z = (float)Math.Atan2(2 * (w * z + x * y), 1 - 2 * (y * y + z * z))
            };

            // yaw (y-axis rotation)
            float sinp = 2 * (w * y - z * x);
            ypr.Y = Math.Abs(sinp) >= 1 ? sinp > 0 ? Pi90 : -Pi90 : (float)Math.Asin(sinp);

            return ypr;
        }

        /// <summary>
        /// Конвертируем из угла Кватерниона в углы Эйлера(Тейт–Брайан).
        /// </summary>
        /// <returns>X=pitch; Y=yaw; Z=roll</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToEulerAnglesXYZ(Vector4 quaternions)
            => ToEulerAnglesXYZ(quaternions.X, quaternions.Y, quaternions.Z, quaternions.W);

        /// <summary>
        /// Конвертируем из угла Кватерниона в углы Эйлера(Тейт–Брайан).
        /// </summary>
        /// <returns>X=pitch; Y=yaw; Z=roll</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToEulerAnglesXYZ(float x, float y, float z, float w)
        {
            // https://discourse.mcneel.com/t/what-is-the-right-method-to-convert-quaternion-to-plane-using-rhinocommon/92411/21?page=2
            Vector3 ypr = new Vector3
            {
                // pitch (x-axis rotation)
                X = (float)Math.Atan2(-2 * (y * z - w * x), w * w - x * x - y * y + z * z),
                // roll (z-axis rotation)
                Z = (float)Math.Atan2(-2 * (x * y - w * z), w * w + x * x - y * y - z * z)
            };

            // yaw (y-axis rotation)
            float sinp = 2 * (x * z + w * y);
            ypr.Y = Math.Abs(sinp) >= 1 ? sinp > 0 ? Pi90 : -Pi90 : (float)Math.Asin(sinp);
            return ypr;
        }

        #endregion


    }
}
