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
        public Bone CreateBone(string nameBoneHead, byte parentIndex, float scale)
            => new Bone(parentIndex, Name.Equals(nameBoneHead),
                RotationX, RotationY, RotationZ, 
                OriginX * scale, OriginY * scale, OriginZ * scale, Children.Count);

        /// <summary>
        /// Получить тип папки по префиксу
        /// </summary>
        public static EnumType ConvertPrefix(string prefix)
        {
            if (prefix == "_") return EnumType.Folder;
            if (prefix == "#") return EnumType.Layer;
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
