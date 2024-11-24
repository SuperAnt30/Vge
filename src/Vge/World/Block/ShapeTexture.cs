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
            if (shape.IsKey("Texture2"))
            {
                texture = shape.GetObject("Texture2");
                foreach (JsonKeyValue texutreKV in texture.Items)
                {
                    int index = BlocksReg.BlockAtlas.AddSprite(texutreKV.GetString());
                    if (index != -1)
                    {
                        //_textures.Add(texutreKV.Key, index);
                    }
                }
            }

            texture = shape.GetObject("Texture");
            foreach (JsonKeyValue texutreKV in texture.Items)
            {
                _textures.Add(texutreKV.Key, texutreKV.GetInt());
            }
        }

        public int GetIndex(string name) => _textures[name];


    }
}
