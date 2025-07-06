using System.Runtime.CompilerServices;
using Vge.Entity.Animation;
using Vge.Entity.Model;
using Vge.Entity.Render;
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
        private readonly VertexEntityBuffer _bufferMesh;
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
            Textures = _definition.GenTextures();
            DepthTextures = new int[Textures.Length];
        }

        #region Definition

        /// <summary>
        /// Очистить объект отвечает за определяение модели сущности
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SizeAdjustmentTextureWidth(float coef)
            => _bufferMesh.SizeAdjustmentTextureWidth(coef);

        /// <summary>
        /// Корректировка размера высоты текстуры, в буффере UV
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SizeAdjustmentTextureHeight(float coef)
            => _bufferMesh.SizeAdjustmentTextureHeight(coef);

        /// <summary>
        /// Копия буфера сетки с масштабом
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VertexEntityBuffer CopyBufferFloatMesh(float scale = 1)
            => _bufferMesh.CopyBufferMesh(scale);

        public override string ToString() => Index + " " 
            + (TextureSmall ? "Small" : "Big") + " " 
            + (Textures.Length == 0 ? "" : (Textures[0].Width + ":" + Textures[0].Height));
    }
}
