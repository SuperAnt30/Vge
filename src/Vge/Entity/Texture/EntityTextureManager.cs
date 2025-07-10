using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Entity.Shape;
using Vge.Util;

namespace Vge.Entity.Texture
{
    /// <summary>
    /// Текстурный менеджер сущностей
    /// </summary>
    public class EntityTextureManager
    {
        /// <summary>
        /// Ширина маленькой текстуры
        /// </summary>
        public int WidthSmall { get; private set; }
        /// <summary>
        /// Высота маленькой текстуры
        /// </summary>
        public int HeightSmall { get; private set; }
        /// <summary>
        /// Глубина меленькой текстуры
        /// </summary>
        public int DepthSmall { get; private set; }
        /// <summary>
        /// Ширина большой текстуры
        /// </summary>
        public int WidthBig { get; private set; }
        /// <summary>
        /// Высота большой текстуры
        /// </summary>
        public int HeightBig { get; private set; }
        /// <summary>
        /// Глубина большой текстуры
        /// </summary>
        public int DepthBig { get; private set; }

        /// <summary>
        /// Массив всех групп текстур
        /// </summary>
        private GroupTexture[] _groups;

        /// <summary>
        /// Инициализация текстурного менеджера
        /// </summary>
        public void Init()
        {
            _Init();

            int index = _GetIndexDivide();
            if (index == -1) return;

            _FlagBegin();
            DepthSmall = DepthBig = 0;

            // Собираем группу Small
            GroupTexture group = _groups[index];
            WidthSmall = group.Width;
            HeightSmall = group.Height;
            _SetDepthTextures(group, false);

            group.Flag = true;
            for (int i = 0; i < group.ArrayCan.Count; i++)
            {
                _ModifyTextures(_groups[group.ArrayCan[i]], WidthSmall, HeightSmall, false);
            }
            // Собираем группу Big
            int maxId = _groups.Length - 1;
            // index не может быть maxId, в _GetIndexDivide() учтено!
            group = _groups[maxId];
            WidthBig = group.Width;
            HeightBig = group.Height;
            _SetDepthTextures(group, true);

            // Изменить статус максимальной группы
            for (int i = 0; i < group.ArrayId.Count; i++)
            {
                _TextureGroupBig(group.ArrayId[i]);
            }
            // Нет смысла пробегать с 0
            for (int i = 1; i < maxId; i++)
            {
                if (!_groups[i].Flag)
                {
                    _ModifyTextures(_groups[i], group.Width, group.Height, true);
                }
            }

            // Освобождаем не нужную память
            _groups = null;
        }

        /// <summary>
        /// Инициализация данных группы, для дальнейших операций
        /// </summary>
        private void _Init()
        {
            // Используя справочник, мы создаём однотипные группы текстур
            Dictionary<string, GroupTexture> pairs = new Dictionary<string, GroupTexture>();
            int count = _GetShapesCount();
            ShapeBase shape;
            BufferedImage buffered;
            string key;

            for (ushort id = 0; id < count; id++)
            {
                shape = _GetShape(id);
                if (shape.Textures.Length > 0)
                {
                    buffered = shape.Textures[0];
                    key = buffered.Width + ":" + buffered.Height;
                    if (!pairs.ContainsKey(key))
                    {
                        pairs.Add(key, new GroupTexture(buffered.Width, buffered.Height));
                    }
                    pairs[key].SetId(id, shape.Textures.Length);
                }
            }
            // Создаём массив наших групп текстур
            _groups = new GroupTexture[pairs.Count];
            int i = 0;
            foreach (GroupTexture group in pairs.Values)
            {
                _groups[i++] = group;
            }

            // Сортируем по размеру слоя
            Array.Sort(_groups);

            // Вносим в группы, какие группы кто может поглатить
            count = _groups.Length;
            GroupTexture groupI, groupJ;
            for (i = 0; i < count; i++)
            {
                groupI = _groups[i];
                for (ushort j = 0; j < count; j++)
                {
                    if (i != j)
                    {
                        groupJ = _groups[j];
                        if (groupJ.Width <= groupI.Width && groupJ.Height <= groupI.Height)
                        {
                            groupI.ArrayCan.Add(j);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Получить индекс разделить, эта группа и все кто входят в неё это TextureMin, остальные TextureMax
        /// </summary>
        private int _GetIndexDivide()
        {
            // На один меньше, так-как последнее значение это максималка, 2 группы обязательно!
            int count = _groups.Length - 1;
#if DEBUG
            // Получить весь размер, без увеличения, т.е. текущий
            int allSize = _AllSize();
            // Получить максимальный размер, с увеличения, для одной группы
            int maxSize = _MaxSize();
            // Собираем массив всех вариаций разделения на две группы для отладки
            int[] sizeAr = new int[count];
#endif

            int min = int.MaxValue;
            // Индекс разделения, эта группа и все кто входят в неё это TextureMin, остальные TextureMax
            int resultIndex = -1;
            int size;
            for (int i = 0; i < count; i++)
            {
                size = _GetSize(i);
                if (size < min)
                {
                    min = size;
                    resultIndex = i;
                }
#if DEBUG
                sizeAr[i] = size;
#endif
            }

            return resultIndex;
        }

        /// <summary>
        /// Сброс флага у всех групп
        /// </summary>
        private void _FlagBegin()
        {
            for (int i = 0; i < _groups.Length; i++)
            {
                _groups[i].Flag = false;
            }
        }

        /// <summary>
        /// Получить весь размер, без увеличения, т.е. текущий
        /// </summary>
        private int _AllSize()
        {
            int result = 0;
            for (int i = 0; i < _groups.Length; i++)
            {
                result += _groups[i].MaxByte();
            }
            return result;
        }

        /// <summary>
        /// Получить максимальный размер, с увеличения, для одной группы
        /// </summary>
        private int _MaxSize()
        {
            int result = 0;
            int maxId = _groups.Length - 1;
            GroupTexture group = _groups[maxId];
            int sizeLayer = group.MaxByteLayer();
            for (int i = 0; i < _groups.Length; i++)
            {
                result += sizeLayer * _groups[i].CountTextures;
            }
            return result;
        }

        /// <summary>
        /// Получить размер, разделённый на 2 группы, входыйщий параметр index с какой делить группы
        /// </summary>
        private int _GetSize(int index)
        {
            _FlagBegin();
            // Вытаскиваем с какой проверяем
            GroupTexture group = _groups[index];
            group.Flag = true;
            int result = group.MaxByte();
            int sizeLayer = group.MaxByteLayer();
            // Меняем все этого размера, всех тех которых он может поглатить
            for (int i = 0; i < group.ArrayCan.Count; i++)
            {
                result += sizeLayer * _groups[group.ArrayCan[i]].CountTextures;
                _groups[group.ArrayCan[i]].Flag = true;
            }

            // Остальные по максимум
            int maxId = _groups.Length - 1;
            if (index != maxId)
            {
                group = _groups[maxId];
                sizeLayer = group.MaxByteLayer();
                for (int i = 0; i < _groups.Length; i++)
                {
                    if (!_groups[i].Flag) result += sizeLayer * _groups[i].CountTextures;
                }
            }

            //for (int i = 0; i < _groups.Length; i++)
            //{
            //    if (!_groups[i].Flag) result += _groups[i].MaxByte();
            //}
            return result;
        }

        /// <summary>
        /// Изменить текстурные данные в ModelEntity
        /// </summary>
        private void _ModifyTextures(GroupTexture group, int width, int height, bool textureGroupBig)
        {
            // Задать индекс глубины текстуры 
            _SetDepthTextures(group, textureGroupBig);
            group.Flag = true;
            for (int i = 0; i < group.ArrayId.Count; i++)
            {
                // Изменить в группе размер текстуры
                _ModifySize(group, group.ArrayId[i], width, height, textureGroupBig);
            }
        }

        /// <summary>
        /// Изменить в группе размер текстуры
        /// </summary>
        /// <param name="index">Индекс с значения _table</param>
        /// <param name="width">Новая ширина</param>
        /// <param name="height">Новая высота</param>
        /// <param name="textureGroupBig">Если true то в максимальную группу</param>
        private void _ModifySize(GroupTexture group, ushort index, int width, int height, bool textureGroupBig)
        {
            ShapeBase shape = _GetShape(index);
            if (textureGroupBig)
            {
                shape.TextureGroupBig();
            }
            bool changeBuffer = false;
            if (group.Width != width)
            {
                // Корректировка размера ширины текстуры
                shape.SizeAdjustmentTextureWidth(group.Width / (float)width);
                changeBuffer = true;
            }
            if (group.Height != height)
            {
                // Корректировка размера высоты текстуры
                shape.SizeAdjustmentTextureHeight(group.Height / (float)height);
                changeBuffer = true;
            }

            if (changeBuffer)
            {
                // Меняем буфер, так-как менялся буфер из-за размера текстуры
                EntitiesReg.SetBufferMeshBecauseSizeTexture(shape);
            }
        }

        /// <summary>
        /// Задать индекс глубины текстуры 
        /// </summary>
        /// <param name="group">Группа</param>
        /// <param name="textureGroupBig">Большая ли текстура</param>
        private void _SetDepthTextures(GroupTexture group, bool textureGroupBig)
        {
            ShapeBase shape;
            int depth;
            for (int i = 0; i < group.ArrayId.Count; i++)
            {
                shape = _GetShape(group.ArrayId[i]);
                for (int t = 0; t < shape.DepthTextures.Length; t++)
                {
                    if (textureGroupBig)
                    {
                        depth = DepthBig;
                        DepthBig++;
                    }
                    else
                    {
                        depth = DepthSmall;
                        DepthSmall++;
                    }
                    shape.DepthTextures[t] = depth;
                }
            }
        }

        #region Shapes

        /// <summary>
        /// Пометить форму в максимальную группу текстур
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _TextureGroupBig(ushort id)
            => _GetShape(id).TextureGroupBig();

        /// <summary>
        /// Получить общее количество форм, одежды и сущности
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int _GetShapesCount() => EntitiesReg.Shapes.Count
            + EntitiesReg.LayerShapes.Count;

        /// <summary>
        /// Получить форму для сущности или одежды, зависимости от id
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ShapeBase _GetShape(ushort id)
        {
            int count = EntitiesReg.Shapes.Count;
            if (id < count) return EntitiesReg.Shapes[id];
            return EntitiesReg.LayerShapes[id - count];
        }

        #endregion
    }
}
