using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Entity.Layer;
using Vge.Entity.Model;
using Vge.Entity.Render;
using Vge.Json;

namespace Vge.Entity.Shape
{
    /// <summary>
    /// Форма слоёв (одежды) сущности, нужна только клиенту
    /// Объект хронящий много одежды одного типа сущности, образно Гуманоид
    /// </summary>
    public class ShapeLayers : ShapeBase
    {
        private readonly Dictionary<string, GroupLayers> _groups;
        /// <summary>
        /// Объект отвечает за определяение модели одежды
        /// </summary>
        private readonly ModelLayerDefinition _definitionLevel;
        /// <summary>
        /// Буфер сетки формы, для рендера
        /// </summary>
       // private readonly VertexEntityBuffer _bufferMesh;

        public ShapeLayers(ushort index, string alias,
            Dictionary<string, GroupLayers> groups, JsonCompound jsonModel) : base(index)
        {
            _groups = groups;

            _definition = _definitionLevel = new ModelLayerDefinition(alias);
            _definition.RunModelFromJson(jsonModel, true);
            _definitionLevel.TexturesUsed(_groups);
            Textures = _definition.GenTextures();
            DepthTextures = new int[Textures.Length];

            // Сгенерировать буферы
            foreach (GroupLayers group in _groups.Values)
            {
                foreach (LayerBuffer layer in group.Layers)
                {
                    layer.BufferMesh = _definitionLevel.GenBufferMesh(layer.Folder);
                    layer.BufferMesh.SetDepthTexture(layer.TextureId);
                }
            }
        }

        /// <summary>
        /// Пометить модель в максимальную группу текстур
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void TextureGroupBig()
        {
            if (TextureSmall)
            {
                TextureSmall = false;
                foreach (GroupLayers group in _groups.Values)
                {
                    foreach (LayerBuffer layer in group.Layers)
                    {
                        layer.BufferMesh.SetSmallDepthTexture();
                    }
                }
            }
        }

        /// <summary>
        /// Задать глубину в текстуру
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetDepthTextures(int index, int depth)
        {
            base.SetDepthTextures(index, depth);
            foreach (GroupLayers group in _groups.Values)
            {
                foreach (LayerBuffer layer in group.Layers)
                {
                    if (layer.TextureId == index)
                    {
                        layer.BufferMesh.SetDepthTexture(depth);
                    }
                }
            }
        }

        /// <summary>
        /// Получить буфер одежды
        /// </summary>
        public VertexEntityBuffer GetBuffer(string group, string name, float scale)
        {
            return _groups[group].GetLayer(name).BufferMesh.CopyBufferMesh(scale);
        }

        /// <summary>
        /// Получить слой одежды, по запросе для смены её
        /// </summary>
        /// <param name="group">Группа одежды</param>
        /// <param name="name">Имя одежды</param>
        public LayerBuffer GetLayer(string group, string name) => _groups[group].GetLayer(name);
    }
}
