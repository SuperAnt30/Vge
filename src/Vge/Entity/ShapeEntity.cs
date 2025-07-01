using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Animation;
using Vge.Entity.Model;
using Vge.Json;
using Vge.Util;

namespace Vge.Entity
{
    /// <summary>
    /// Форма сущности, нужна только клиенту
    /// </summary>
    public class ShapeEntity
    {
        /// <summary>
        /// Индекс в глобальном массиве
        /// </summary>
        public readonly ushort Index;
        /// <summary>
        /// Текстуры для моба
        /// </summary>
        public readonly BufferedImage[] Textures;
        /// <summary>
        /// Индекс глубины текстуры для моба
        /// </summary>
        public readonly int[] DepthTextures;
        /// <summary>
        /// Минимальная текстура
        /// </summary>
        public bool TextureSmall { get; private set; } = true;

        /// <summary>
        /// Буфер сетки формы, для рендера
        /// </summary>
        private readonly float[] _bufferMesh;
        /// <summary>
        /// Объект отвечает за определяение модели сущности
        /// </summary>
        private ModelEntityDefinition _definition;

        public ShapeEntity(ushort index, string alias, JsonCompound jsonModel)
        {
            Index = index;
            _definition = new ModelEntityDefinition(alias);
            _definition.RunModelFromJson(jsonModel);
            _bufferMesh = _definition.BufferMesh;
            Textures = _definition.Textures;
            DepthTextures = new int[Textures.Length];
        }

        #region Definition

        /// <summary>
        /// Очистить объект отвечает за определяение модели сущности
        /// </summary>
        public void ClearDefinition() => _definition = null;

        /// <summary>
        /// Сгенерировать массив костей
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bone[] GenBones(float scale) => _definition.GenBones(scale);

        /// <summary>
        /// Сгенерировать списки модели ключевых кадров для каждой кости скелета
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ModelAnimationClip[] GetModelAnimationClips(AnimationData[] animationDatas)
            => _definition.GetModelAnimationClips(animationDatas);

        #endregion 

        /// <summary>
        /// Пометить модель в максимальную группу текстур
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void TextureGroupBig() => TextureSmall = false;

        /// <summary>
        /// Корректировка размера ширины текстуры, в буффере UV
        /// </summary>
        public void SizeAdjustmentTextureWidth(float coef)
        {
            // XYZ UV B - 6 флоатов на вершину
            for(int i = 3; i < _bufferMesh.Length; i += 6)
            {
                _bufferMesh[i] *= coef;
            }
        }
        /// <summary>
        /// Корректировка размера высоты текстуры, в буффере UV
        /// </summary>
        public void SizeAdjustmentTextureHeight(float coef)
        {
            // XYZ UV B - 6 флоатов на вершину
            for (int i = 4; i < _bufferMesh.Length; i += 6)
            {
                _bufferMesh[i] *= coef;
            }
        }

        /// <summary>
        /// Копия буфера сетки с масштабом
        /// </summary>
        public float[] CopyBufferMesh(float scale = 1)
        {
            float[] buffer = new float[_bufferMesh.Length];
            Array.Copy(_bufferMesh, buffer, buffer.Length);

            if (scale != 1)
            {
                for (int i = 0; i < buffer.Length; i += 6)
                {
                    buffer[i] *= scale;
                    buffer[i + 1] *= scale;
                    buffer[i + 2] *= scale;
                }
            }
            return buffer;
        }

        public override string ToString() => Index + " " 
            + (TextureSmall ? "Small" : "Big") + " " 
            + (Textures.Length == 0 ? "" : (Textures[0].Width + ":" + Textures[0].Height));
    }
}
