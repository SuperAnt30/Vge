namespace Vge.Entity.Model
{
    /// <summary>
    /// Объект кости
    /// </summary>
    public class Bone
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
        /// Вращение по X
        /// </summary>
        public readonly float RotationX;
        /// <summary>
        /// Вращение по Y
        /// </summary>
        public readonly float RotationY;
        /// <summary>
        /// Вращение по Z
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
        public readonly Bone[] Children;

        public Bone(byte index, bool isPitch, float rotationX, float rotationY, float rotationZ,
            float originX, float originY, float originZ, int children)
        {
            Index = index;
            IsPitch = isPitch;
            RotationX = rotationX;
            RotationY = rotationY;
            RotationZ = rotationZ;
            OriginX = originX / 16f;
            OriginY = originY / 16f;
            OriginZ = originZ / 16f;
            Children = new Bone[children];
        }

        public void SetChildren(Bone[] bones)
        {
            for (int i = 0; i < Children.Length; i++)
            {
                Children[i] = bones[i];
            }
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
