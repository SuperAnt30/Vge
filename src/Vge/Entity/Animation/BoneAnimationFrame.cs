using System.Runtime.CompilerServices;

namespace Vge.Entity.Animation
{
    /// <summary>
    /// Ключевой кадр позиции или ориентации кости
    /// </summary>
    public struct BoneAnimationFrame
    {
        /// <summary>
        /// Номер кадра
        /// </summary>
        public readonly float Time;

        /// <summary>
        /// Позиция X или вращение по X (Pitch)
        /// </summary>
        public readonly float X;
        /// <summary>
        /// Позиция Y или вращение по Y (Yaw)
        /// </summary>
        public readonly float Y;
        /// <summary>
        /// Позиция Z или вращение по Z (Roll)
        /// </summary>
        public readonly float Z;

        public BoneAnimationFrame(float time, float x, float y, float z)
        {
            Time = time;
            X = x;
            Y = y;
            Z = z;
        }

        public BoneAnimationFrame(float x, float y, float z)
        {
            Time = 0;
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Получает среднее значение позиции
        /// </summary>
        /// <param name="currentTime">Текущее время кадра</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BoneAnimationFrame Lerp(BoneAnimationFrame from, BoneAnimationFrame to, float currentTime)
        {
            // Коэффициент в диапазоне 0 .. 1
            float timeIndex = (currentTime - from.Time) / (to.Time - from.Time);
            return new BoneAnimationFrame(
                from.X + (to.X - from.X) * timeIndex,
                from.Y + (to.Y - from.Y) * timeIndex,
                from.Z + (to.Z - from.Z) * timeIndex
                );
        }

        public override string ToString()
            => string.Format("T:{0:0.00} [{1:0.00}; {2:0.00}; {3:0.00}]", Time, X, Y, Z);
    }
}
