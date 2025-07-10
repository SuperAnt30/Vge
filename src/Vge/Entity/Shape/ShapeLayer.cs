using Vge.Entity.Model;
using Vge.Json;

namespace Vge.Entity.Shape
{
    /// <summary>
    /// Форма слоёв (одежды) сущности, нужна только клиенту
    /// Объект хронящий много одежды одного типа сущности, образно Гуманоид
    /// </summary>
    public class ShapeLayer : ShapeBase
    {
        /// <summary>
        /// Буфер сетки формы, для рендера
        /// </summary>
       // private readonly VertexEntityBuffer _bufferMesh;

        public ShapeLayer(ushort index, string alias, JsonCompound jsonModel)
            : base(index, alias, jsonModel)
        {
            //_bufferMesh = _definition.BufferMesh;
        }

        /// <summary>
        /// Создаём объект отвечает за определяение модели сущности
        /// </summary>
        protected override void _CreateDefinition(string alias)
            => _definition = new ModelLayerDefinition(alias);

        /*
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
        */
    }
}
