using System;
using System.Collections.Generic;

namespace Vge.Entity.Texture
{
    /// <summary>
    /// Группа текстур, конкретного размера
    /// </summary>
    internal class GroupTexture : IComparable
    {
        public readonly int Width;
        public readonly int Height;
        /// <summary>
        /// Количество текстур, не сущностей, у сущности может быть несколько текстур
        /// </summary>
        public int CountTextures { get; private set; }

        public bool Flag;
    
        /// <summary>
        /// Массив id текстур в этой группе
        /// </summary>
        public readonly List<ushort> ArrayId = new List<ushort>();
        /// <summary>
        /// Массив id сущностей которые может поглатить по размеру
        /// </summary>
        public readonly List<ushort> ArrayCan = new List<ushort>();

        public GroupTexture(int width, int height)
        {
            Width = width;
            Height = height;
        }
        public void SetId(ushort id, int countTextures)
        {
            CountTextures += countTextures;
            ArrayId.Add(id);
        }

        /// <summary>
        /// Размер группы
        /// </summary>
        public int MaxByte() => CountTextures * MaxByteLayer();
        /// <summary>
        /// Размер слоя
        /// </summary>
        public int MaxByteLayer() => Width * Height * 4;

        /// <summary>
        /// Метод для сортировки
        /// </summary>
        public int CompareTo(object obj)
        {
            if (obj is GroupTexture v)
            {
                return MaxByteLayer().CompareTo(v.MaxByteLayer());
            }
            else
            {
                throw new Exception(Sr.ItIsImpossibleToCompareTwoObjects);
            }
        }

        public override string ToString() => Width + ":" + Height + " " 
            + ArrayId.Count + "|" + CountTextures 
            + " =" + (MaxByte() / 1024);
    }
}