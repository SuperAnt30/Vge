using System;
using Vge.Entity.Animation;
using Vge.Json;
using Vge.Util;
using WinGL.Util;

namespace Vge.Entity
{
    /// <summary>
    /// Модель сущности, нужна для рендера и анимации (имеет скелет)
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
        public float[] BufferMesh { get; private set; }
        /// <summary>
        /// Текстуры для моба
        /// </summary>
        public BufferedImage[] Textures { get; private set; }
        /// <summary>
        /// Древо костей, для скелетной анимации
        /// </summary>
        public Bone0[] TreeBones { get; private set; }
        /// <summary>
        /// Массив костей скелета
        /// </summary>
        public Bone[] Bones { get; private set; }
        /// <summary>
        /// Массив объектов моделей анимационныйх клиппов
        /// </summary>
        public ModelAnimationClip[] ModelAnimationClips { get; private set; }

        /// <summary>
        /// Название кости меняющее от Pitch
        /// </summary>
        private string _nameBonePitch;

        private Mat4 _matrix = Mat4.Identity();

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
                        if (json.IsKey(Cte.Pitch)) _nameBonePitch = json.GetString();
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

            ModelEntityDefinition definition = new ModelEntityDefinition(Alias, _nameBonePitch);
            definition.RunModelFromJson(model);

            BufferMesh = definition.BufferMesh;
            Textures = definition.Textures;
            TreeBones = definition.GenTreeBones();
            Bones = definition.Bones;
            ModelAnimationClips = definition.GetModelAnimationClips();
              
            return;
        }

        #endregion
    }
}
