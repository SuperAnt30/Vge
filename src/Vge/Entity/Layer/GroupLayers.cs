using System.Collections.Generic;

namespace Vge.Entity.Layer
{
    /// <summary>
    /// Группа слоёв
    /// </summary>
    public class GroupLayers
    {
        /// <summary>
        /// Имя группы
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// Слои группы
        /// </summary>
        public readonly List<LayerBuffer> Layers = new List<LayerBuffer>();

        public GroupLayers(string name) => Name = name;

        public LayerBuffer GetLayer(string name)
        {
            foreach(LayerBuffer layer in Layers)
            {
                if (layer.Name == name)
                {
                    return layer;
                }
            }
            return null;
        }

        public void Add(LayerBuffer layer)
        {
            Layers.Add(layer);
        }
     }
}
