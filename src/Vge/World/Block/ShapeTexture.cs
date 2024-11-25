using System.Collections.Generic;
using Vge.Json;

namespace Vge.World.Block
{
    /// <summary>
    /// Текстуры к фигуре
    /// </summary>
    public class ShapeTexture
    {
        /// <summary>
        /// Справочник текстур, название текстуры, индекс расположения текстуры в атласе
        /// </summary>
        private readonly Dictionary<string, int> _textures = new Dictionary<string, int>();

        public void RunShape(JsonCompound shape)
        {
            _textures.Clear();
            JsonCompound texture;
            if (shape.IsKey(Ctb.Texture))
            {
                texture = shape.GetObject(Ctb.Texture);
                foreach (JsonKeyValue texutreKV in texture.Items)
                {
                    int index = BlocksReg.BlockAtlas.AddSprite(texutreKV.GetString());
                    if (index != -1)
                    {
                        _textures.Add(texutreKV.Key, index);
                    }
                }
            }
        }

        /// <summary>
        /// Получить индекс текстуры, если нет по названию вернёт 0.
        /// 0 должны создать первую текстуру в Debug
        /// </summary>
        public int GetIndex(string name)
        {
            if (_textures.ContainsKey(name))
            {
                return _textures[name];
            }
            return 0;
        }
    }
}
