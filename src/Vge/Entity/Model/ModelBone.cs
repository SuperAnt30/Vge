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
        /// Тип папку кости
        /// </summary>
        public readonly EnumType TypeFolder;
        /// <summary>
        /// Кости детей
        /// </summary>
        public readonly List<ModelElement> Children = new List<ModelElement>();

        public ModelBone(string uuid, string name, EnumType typeFolder) : base(uuid, name)
            => TypeFolder = typeFolder;

        /// <summary>
        /// Создать кость
        /// </summary>
        /// <param name="nameBoneHead">Название кости меняющее от Pitch</param>
        public Bone CreateBone(byte parentIndex, float scale)
            => new Bone(parentIndex, Name == ModelEntityDefinition.NameBoneHead,
                RotationX, RotationY, RotationZ, 
                OriginX * scale, OriginY * scale, OriginZ * scale, Children.Count);

        /// <summary>
        /// Получить тип папки по префиксу
        /// </summary>
        public static EnumType ConvertPrefix(string prefix)
        {
            if (prefix[0] == '_') return EnumType.Folder;
            if (prefix.Length > 1 && prefix[0] == 'L' && prefix[1] == '-') return EnumType.Layer;
            return EnumType.Bone;
        }

        /// <summary>
        /// Тип папку кости
        /// </summary>
        public enum EnumType
        {
            /// <summary>
            /// Кость, должна быть только видимой
            /// </summary>
            Bone,
            /// <summary>
            /// Папка, можно быить скрытой, дети подтянуться
            /// </summary>
            Folder,
            /// <summary>
            /// Одежда, можно быить скрытой, дети подтянуться
            /// </summary>
            Layer
        }

    }
}
