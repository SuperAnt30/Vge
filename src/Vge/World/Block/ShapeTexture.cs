using System.Collections.Generic;
using Vge.Json;
using WinGL.Util;

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
        private readonly Dictionary<string, SpriteData> _textures = new Dictionary<string, SpriteData>();

        public void RunShape(JsonCompound shape)
        {
            _textures.Clear();
            JsonCompound texture;
            if (shape.IsKey(Ctb.Texture))
            {
                texture = shape.GetObject(Ctb.Texture);
                foreach (JsonKeyValue texutreKV in texture.Items)
                {
                    SpriteData res = BlocksReg.BlockItemAtlas.AddSprite(texutreKV.GetString());
                    if (res.Index != -1)
                    {
                        _textures.Add(texutreKV.Key, res);
                    }
                }
            }
        }

        /// <summary>
        /// Получить индекс текстуры, если нет по названию вернёт 0. И количество кадров для анимации, если 2 и более анимация.
        /// 0 должны создать первую текстуру в Debug
        /// </summary>
        public SpriteData GetResult(string name)
        {
            if (_textures.ContainsKey(name))
            {
                return _textures[name];
            }
            return new SpriteData(0);
        }
    }
}
