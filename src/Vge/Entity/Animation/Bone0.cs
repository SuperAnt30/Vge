using WinGL.Util;

namespace Vge.Entity.Animation
{
    /// <summary>
    /// Объект кости
    /// </summary>
    public class Bone0
    {
        /// <summary>
        /// Индекс кости
        /// </summary>
        public readonly byte Index;
        /// <summary>
        /// Кость реагирует на угол Pitch
        /// </summary>
        public readonly bool IsPitch;

        /// <summary>
        /// Вращение по X (Pitch)
        /// </summary>
        public readonly float RotationX;
        /// <summary>
        /// Вращение по Y (Yaw)
        /// </summary>
        public readonly float RotationY;
        /// <summary>
        /// Вращение по Z (Roll)
        /// </summary>
        public readonly float RotationZ;

        /// <summary>
        /// Центральная точка для вращения по X
        /// </summary>
        public readonly float OriginX;
        /// <summary>
        /// Центральная точка для вращения по Y
        /// </summary>
        public readonly float OriginY;
        /// <summary>
        /// Центральная точка для вращения по Z
        /// </summary>
        public readonly float OriginZ;
        /// <summary>
        /// Массив детей
        /// </summary>
        public readonly Bone0[] Children;

        /// <summary>
        /// Кэш матрица для оптимизации
        /// </summary>
        public readonly Mat4 Matrix = Mat4.Identity();
        /// <summary>
        /// Обратная bind-pose матрица для перехода в систему координат кости из пространства модели
        /// </summary>
        public readonly Mat4 MatrixInverse;

        public Bone0(byte index, bool isPitch, float rotationX, float rotationY, float rotationZ,
            float originX, float originY, float originZ, int children)
        {
            Index = index;
            IsPitch = isPitch;
            RotationX = Glm.Radians(rotationX);
            RotationY = Glm.Radians(rotationY);
            RotationZ = Glm.Radians(rotationZ);
            OriginX = originX / 16f;
            OriginY = originY / 16f;
            OriginZ = originZ / 16f;

            // Создаём обратную матрицу
            MatrixInverse = new Mat4(OriginX, OriginY, OriginZ);

            //TODO::2025-05-05 !!!
            if (RotationX != 0 || RotationY != 0 || RotationZ != 0)
            {
                Vector4 v = Glm.ToQuaternionXYZ(-RotationY, -RotationX, -RotationZ);
                MatrixInverse.RotateQuat(v);
            }

            //TODO::2025-05-03 Почему-то для обратной так выходит, но это же не точно!!! Тем более углы отрицательные
            //MatrixInverse.RotateXYZ(-RotationY, -RotationX, -RotationZ);

            MatrixInverse = Glm.Inverse(MatrixInverse);

            Children = new Bone0[children];
        }

        public void SetChildren(Bone0[] bones)
        {
            for (int i = 0; i < Children.Length; i++)
            {
                Children[i] = bones[i];
            }
        }
        
        /// <summary>
        /// Поза кости покуда на примере pitch
        /// </summary>
        public Mat4 GetBoneMatrix(float pitch)
        {
            //TODO::2025-05-05 !!!
            Mat4 m = new Mat4(OriginX, OriginY, OriginZ);
            //Mat4 m = Mat4.Identity();
            //m[3] = m[0] * OriginX + m[1] * OriginY + m[2] * OriginZ + m[3];
            m.Rotate(pitch, 1, 0, 0);
           // m[3] = m[0] * -OriginX + m[1] * -OriginY + m[2] * -OriginZ + m[3];

            return m;
        }

        /// <summary>
        /// Поза отдельной кости в заданный момент времени
        /// </summary>
        public Mat4 GetBoneMatrix()
        {
            //TODO::2025-05-05 !!!
            Mat4 m = new Mat4(OriginX, OriginY, OriginZ);
            //Mat4 m = Mat4.Identity();
            //m[3] = m[0] * OriginX + m[1] * OriginY + m[2] * OriginZ + m[3];
            ////Glm.RotateFast(m, pitch, 1, 0, 0);
            //m[3] = m[0] * -OriginX + m[1] * -OriginY + m[2] * -OriginZ + m[3];

            return m;
        }

        public override string ToString()
        {
            string s = Index.ToString();
            if (Children.Length > 0) s += " " + Children.Length.ToString();
            if (IsPitch) s += " Pitch";
            return s;
        }
    }
}
