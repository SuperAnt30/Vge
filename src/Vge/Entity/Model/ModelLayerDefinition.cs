using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
