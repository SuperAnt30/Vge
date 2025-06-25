using System.Runtime.CompilerServices;
using WinGL.Util;

namespace Vge.Entity.Animation
{
    /// <summary>
    /// Поза отдельной кости в заданный момент времени
    /// </summary>
    public struct BonePose
    {
        private static Mat4 _matrixCache = Mat4.Identity();

        /// <summary>
        /// Центральная точка для вращения по X
        /// </summary>
        public float PositionX;
        /// <summary>
        /// Центральная точка для вращения по Y
        /// </summary>
        public float PositionY;
        /// <summary>
        /// Центральная точка для вращения по Z
        /// </summary>
        public float PositionZ;

        /// <summary>
        /// Вращение по X (Pitch)
        /// </summary>
        public float RotationX;
        /// <summary>
        /// Вращение по Y (Yaw)
        /// </summary>
        public float RotationY;
        /// <summary>
        /// Вращение по Z (Roll)
        /// </summary>
        public float RotationZ;

        /// <summary>
        /// Поза отдельной кости в заданный момент времени
        /// </summary>
        public Mat4 GetBoneMatrix()
        {
            _matrixCache.Clear();
            _matrixCache.Translate(PositionX, PositionY, PositionZ);
            _matrixCache.RotateZYX(RotationY, RotationX, RotationZ);
            return _matrixCache;
        }

        /// <summary>
        /// Добавить смещение и вращение к текущей позе
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(BonePose bonePose, float mix)
        {
            RotationX += bonePose.RotationX * mix;
            RotationY += bonePose.RotationY * mix;
            RotationZ += bonePose.RotationZ * mix;

            PositionX += bonePose.PositionX * mix;
            PositionY += bonePose.PositionY * mix;
            PositionZ += bonePose.PositionZ * mix;
        }

        /// <summary>
        /// Добавить смещение и вращение к текущей позе
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(BonePose bonePose)
        {
            RotationX += bonePose.RotationX;
            RotationY += bonePose.RotationY;
            RotationZ += bonePose.RotationZ;

            PositionX += bonePose.PositionX;
            PositionY += bonePose.PositionY;
            PositionZ += bonePose.PositionZ;
        }

        /// <summary>
        /// Внести данные
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPositionRotation(float posX, float posY, float posZ, float rotateX, float rotateY, float rotateZ)
        {
            PositionX = posX;
            PositionY = posY;
            PositionZ = posZ;
            RotationX = rotateX;
            RotationY = rotateY;
            RotationZ = rotateZ;
        }
    }
}
