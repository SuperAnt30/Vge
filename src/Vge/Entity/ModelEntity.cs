using System;
using Vge.Entity.Model;
using Vge.Json;
using Vge.Util;

namespace Vge.Entity
{
    /// <summary>
    /// Модель сущности, нужна для рендера и анимации
    /// На сервере будет упрощённая
    /// </summary>
    public class ModelEntity
    {
        /// <summary>
        /// Название модели
        /// </summary>
        public readonly string Alias;
        /// <summary>
        /// Буфер сетки моба, для рендера
        /// </summary>
        public float[] Buffer { get; private set; }
        /// <summary>
        /// Текстуры для моба
        /// </summary>
        public BufferedImage[] Textures { get; private set; }
        /// <summary>
        /// Древо костей, для скелетной анимации
        /// </summary>
        public Bone[] Bones { get; private set; }

        public ModelEntity(string alias) => Alias = alias;

        #region Методы для импорта данных с json

        /// <summary>
        /// Прочесть состояние из Json формы
        /// </summary>
        public void ReadStateFromJson(JsonCompound state)
        {
            if (state.Items != null)
            {
                try
                {
                    // Статы
                    foreach (JsonKeyValue json in state.Items)
                    {
                        //if (json.IsKey(Ctb.LightOpacity)) LightOpacity = (byte)json.GetInt();
                        //if (json.IsKey(Ctb.LightValue)) LightValue = (byte)json.GetInt();
                        //if (json.IsKey(Ctb.Translucent)) Translucent = json.GetBool();
                        //if (json.IsKey(Ctb.UseNeighborBrightness)) UseNeighborBrightness = json.GetBool();
                        //if (json.IsKey(Ctb.АmbientOcclusion)) АmbientOcclusion = json.GetBool();
                        //if (json.IsKey(Ctb.BiomeColor)) BiomeColor = json.GetBool();
                        //if (json.IsKey(Ctb.Shadow)) Shadow = json.GetBool();
                        //if (json.IsKey(Ctb.Color))
                        //{
                        //    float[] ar = json.GetArray().ToArrayFloat();
                        //    Color = new Vector3(ar[0], ar[1], ar[2]);
                        //}
                    }
                }
                catch
                {
                    throw new Exception(Sr.GetString(Sr.ErrorReadJsonEntityStat, Alias));
                }
            }
        }

        /// <summary>
        /// Прочесть состояние из Json формы и модель
        /// </summary>
        public void ReadStateFromJson(JsonCompound state, JsonCompound model)
        {
            ReadStateFromJson(state);

            ModelEntityDefinition definition = new ModelEntityDefinition(Alias);
            definition.RunModelFromJson(model);

            Buffer = definition.Buffer;
            Textures = definition.Textures;
            Bones = definition.GenBones();

            return;
        }

        #endregion
    }
}
