using System.Collections.Generic;
using Vge.Entity.Animation;

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

        /// <summary>
        /// Создать кость
        /// </summary>
        /// <param name="nameBonePitch">Название кости меняющее от Pitch</param>
        public Bone CreateBone(string nameBonePitch, byte parentIndex)
            => new Bone(parentIndex, Name.Equals(nameBonePitch),
                RotationX, RotationY, RotationZ, OriginX, OriginY, OriginZ, Children.Count);
    }
}
