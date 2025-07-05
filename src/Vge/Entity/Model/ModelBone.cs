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
        /// Если true, то это папка, не кость
        /// </summary>
        public readonly bool NotBone;
        /// <summary>
        /// Кости детей
        /// </summary>
        public readonly List<ModelElement> Children = new List<ModelElement>();

        public ModelBone(string uuid, string name, bool notBone) : base(uuid, name)
            => NotBone = notBone;

        /// <summary>
        /// Создать кость
        /// </summary>
        /// <param name="nameBoneHead">Название кости меняющее от Pitch</param>
        public Bone CreateBone(string nameBoneHead, byte parentIndex, float scale)
            => new Bone(parentIndex, Name.Equals(nameBoneHead),
                RotationX, RotationY, RotationZ, 
                OriginX * scale, OriginY * scale, OriginZ * scale, Children.Count);
    }
}
