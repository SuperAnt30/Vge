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
        private readonly Dictionary<string, Vector2i> _textures = new Dictionary<string, Vector2i>();

        public void RunShape(JsonCompound shape)
        {
            _textures.Clear();
            JsonCompound texture;
            if (shape.IsKey(Ctb.Texture))
            {
                texture = shape.GetObject(Ctb.Texture);
                foreach (JsonKeyValue texutreKV in texture.Items)
                {
                    Vector2i res = BlocksReg.BlockAtlas.AddSprite(texutreKV.GetString());
                    if (res.X != -1)
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
        public Vector2i GetResult(string name)
        {
            if (_textures.ContainsKey(name))
            {
                return _textures[name];
            }
            return new Vector2i(0);
        }
    }
}
