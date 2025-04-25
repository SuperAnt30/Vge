using System.Collections.Generic;

namespace Vge.Entity.Model
{
    /// <summary>
    /// Объект кости для скелетной анимации
    /// </summary>
    public class ModelBone : ModelElement
    {
        /// <summary>
        /// Кости детей
        /// </summary>
        public readonly List<ModelElement> Children = new List<ModelElement>();

        public ModelBone(string uuid, string name, byte boneIndex) : base(uuid, name)
            => BoneIndex = boneIndex;

        public Bone CreateBone()
            => new Bone(BoneIndex, RotationX, RotationY, RotationZ, OriginX, OriginY, OriginZ, Children.Count);
    }
}
