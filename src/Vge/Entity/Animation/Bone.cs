using WinGL.Util;

namespace Vge.Entity.Animation
{
    /// <summary>
    /// Кость
    /// </summary>
    public class Bone
    {
        /// <summary>
        /// Индекс корневой кости
        /// </summary>
        public const byte RootBoneParentIndex = 255;

        /// <summary>
        /// Индекс родительской кости (для корневой кости будет зарезервированный идентификатор RootBoneParentIndex,
        /// таким образом, скелет сможет содержать не более, чем 254 кости)
        /// </summary>
        public readonly byte ParentIndex;
        /// <summary>
        /// Обратная bind-pose матрица для перехода в систему координат кости из пространства модели
        /// </summary>
        public readonly Mat4 MatrixInverse;

        /// <summary>
        /// Кость реагирует на угол Pitch, голова
        /// </summary>
        public readonly bool IsHead;
        /// <summary>
        /// Кость правой кисти руки, для предмета в руке
        /// </summary>
        public readonly bool IsHandRight;

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

        public Bone(byte parentIndex, bool isHead, bool isHandRight,
            float rotationX, float rotationY, float rotationZ,
            float originX, float originY, float originZ, int children)
        {
            ParentIndex = parentIndex;
            IsHead = isHead;
            IsHandRight = isHandRight;
            RotationX = Glm.Radians(rotationX);
            RotationY = Glm.Radians(rotationY);
            RotationZ = Glm.Radians(rotationZ);
            OriginX = originX / 16f;
            OriginY = originY / 16f;
            OriginZ = originZ / 16f;

            // Создаём обратную матрицу
            MatrixInverse = Glm.Inverse(new Mat4(OriginX, OriginY, OriginZ));
        }

        /// <summary>
        /// Создать Поза отдельной кости в заданный момент времени 
        /// </summary>
        public BonePose CreateBonePose()
        {
            BonePose bonePose = new BonePose()
            {
                PositionX = OriginX,
                PositionY = OriginY,
                PositionZ = OriginZ,
                RotationX = RotationX,
                RotationY = RotationY,
                RotationZ = RotationZ,
            };
            return bonePose;
        }

        /// <summary>
        /// Создать кости позы значение с кости
        /// </summary>
        public void SetBonePose(ref BonePose bone)
        {
            bone.PositionX = OriginX;
            bone.PositionY = OriginY;
            bone.PositionZ = OriginZ;
            bone.RotationX = RotationX;
            bone.RotationY = RotationY;
            bone.RotationZ = RotationZ;
        }

        public override string ToString()
        {
            string s = ParentIndex.ToString();
            if (IsHead) s += " Head";
            return s;
        }
    }
}
