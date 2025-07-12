using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vge.Entity.Layer;
using Vge.Entity.Render;
using Vge.Json;

namespace Vge.Entity.Model
{
    /// <summary>
    /// Объект отвечает за определяение модели одежды
    /// </summary>
    public class ModelLayerDefinition : ModelEntityDefinition
    {
        /// <summary>
        /// Карта уровней кубов, костей, по индексам
        /// </summary>
        private readonly Dictionary<string, Data> _mapLevels = new Dictionary<string, Data>();

        public ModelLayerDefinition(string alias) : base(alias) { }

        /// <summary>
        /// Помечаем текстуры которые используем
        /// </summary>
        public void TexturesUsed(Dictionary<string, GroupLayers> groups)
        {
            ushort i = 0;
            bool b = false;
            foreach(ModelTexture texture in _textures)
            {
                foreach (GroupLayers group in groups.Values)
                {
                    foreach (LayerBuffer layer in group.Layers)
                    {
                        if (texture.Name == layer.Texture)
                        {
                            layer.TextureId = i;
                            if (!texture.Used)
                            {
                                b = true;
                                texture.Used = true;
                            }
                        }
                    }
                }
                if (b)
                {
                    b = false;
                    i++;
                }
            }
        }

        /// <summary>
        /// Собираем анимации
        /// </summary>
        protected override void _Animations(JsonCompound[] animations) { }

        /// <summary>
        /// Задать смену уровня
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _SetCubeLayer(ModelBone modelBoneParent, ModelCube modelCube)
        {
            string name = modelBoneParent.Name;
            Data data;
            if (_mapLevels.ContainsKey(name))
            {
                data = _mapLevels[name];
            }
            else
            {
                data = new Data(name);
                _mapLevels.Add(name, data);
            }
            modelCube.Layer = true;
            data.Add(modelCube);
        }

        /// <summary>
        /// Генерируем буфер сетки моба, только выбранных папок для рендера
        /// </summary>
        public VertexEntityBuffer GenBufferMesh(string[] folders)
        {
            // Генерируем буффер
            List<float> listFloat = new List<float>();
            List<int> listInt = new List<int>();
            foreach(string folder in folders)
            {
                if (_mapLevels.ContainsKey(folder))
                {
                    foreach (ModelCube cube in _mapLevels[folder].Layers)
                    {
                        cube.GenBuffer(listFloat, listInt);
                    }
                }
            }
            return new VertexEntityBuffer(listFloat.ToArray(), listInt.ToArray());
        }

        private class Data
        {
            public readonly string Name;
            public readonly List<ModelCube> Layers = new List<ModelCube>();

            public Data(string name) => Name = name;
            public void Add(ModelCube modelCube) => Layers.Add(modelCube);
        }

        /// <summary>
        /// Очитстка не нужных текстур
        /// </summary>
        protected override void _CleaningUpUnnecessaryTextures()
        {
            // Пробегаемся и удаляем не нужные текстуры, которые не используются, в слоях
            int count = _textures.Count - 1;
            for (int i = count; i >= 0; i--)
            {
                if (!_textures[i].Used) // Для одежды, мы ранее должны были указать нужные слои
                {
                    // Удалить слои текстур не использующие
                    _textures.RemoveAt(i);
                }
            }
        }
    }
}
