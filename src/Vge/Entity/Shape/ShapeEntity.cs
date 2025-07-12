using System.Runtime.CompilerServices;
using Vge.Entity.Animation;
using Vge.Entity.Model;
using Vge.Entity.Render;
using Vge.Json;
using Vge.Util;

namespace Vge.Entity.Shape
{
    /// <summary>
    /// Форма сущности, нужна только клиенту
    /// </summary>
    public class ShapeEntity : ShapeBase
    {
        /// <summary>
        /// Буфер сетки формы, для рендера
        /// </summary>
        private readonly VertexEntityBuffer _bufferMesh;
        
        public ShapeEntity(ushort index, string alias, JsonCompound jsonModel)
            : base(index)
        {
            _definition = new ModelEntityDefinition(alias);
            _definition.RunModelFromJson(jsonModel);
            Textures = _definition.GenTextures();
            DepthTextures = new int[Textures.Length];

            _bufferMesh = _definition.GenBufferMesh();
        }

        #region Definition

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
        /// Корректировка размера ширины текстуры, в буффере UV
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SizeAdjustmentTextureWidth(float coef)
            => _bufferMesh.SizeAdjustmentTextureWidth(coef);

        /// <summary>
        /// Корректировка размера высоты текстуры, в буффере UV
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SizeAdjustmentTextureHeight(float coef)
            => _bufferMesh.SizeAdjustmentTextureHeight(coef);

        /// <summary>
        /// Копия буфера сетки с масштабом
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public VertexEntityBuffer CopyBufferFloatMesh(float scale = 1)
            => _bufferMesh.CopyBufferMesh(scale);
    }
}
