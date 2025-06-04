using System;
using Vge.Entity.Animation;
using Vge.Entity.Model;
using Vge.Json;
using Vge.Util;

namespace Vge.Entity
{
    /// <summary>
    /// Модель сущности, нужна для рендера и анимации (имеет скелет)
    /// На сервере будет упрощённая
    /// </summary>
    public class ModelEntity
    {
        /// <summary>
        /// Индекс сущности из таблицы
        /// </summary>
        public ushort IndexEntity { get; private set; }
        /// <summary>
        /// Название модели
        /// </summary>
        public readonly string Alias;
        /// <summary>
        /// Тип объекта сущности
        /// </summary>
        public readonly Type EntityType;
        /// <summary>
        /// Буфер сетки моба, для рендера
        /// </summary>
        public float[] BufferMesh { get; private set; }
        /// <summary>
        /// Текстуры для моба
        /// </summary>
        public BufferedImage[] Textures { get; private set; }
        /// <summary>
        /// Индекс глубины текстуры для моба
        /// </summary>
        public int[] DepthTextures { get; private set; }
        /// <summary>
        /// Массив костей скелета
        /// </summary>
        public Bone[] Bones { get; private set; }
        /// <summary>
        /// Массив объектов моделей анимационныйх клиппов
        /// </summary>
        public ModelAnimationClip[] ModelAnimationClips { get; private set; }
        /// <summary>
        /// Минимальная текстура
        /// </summary>
        public bool TextureSmall { get; private set; } = true;

        /// <summary>
        /// Название кости меняющее от Pitch
        /// </summary>
        private string _nameBonePitch;

       // private Mat4 _matrix = Mat4.Identity();

        public ModelEntity(string alias, Type entityType)
        {
            Alias = alias;
            EntityType = entityType;
        }

        /// <summary>
        /// Задать индекс сущности, из таблицы
        /// </summary>
        public void SetIndex(ushort id) => IndexEntity = id;

        /// <summary>
        /// Пометить модель в максимальную группу текстур
        /// </summary>
        public void TextureGroupBig() => TextureSmall = false;

        /// <summary>
        /// Корректировка размера ширины текстуры, в буффере UV
        /// </summary>
        public void SizeAdjustmentTextureWidth(float coef)
        {
            // XYZ UV B - 6 флоатов на вершину
            for(int i = 3; i < BufferMesh.Length; i += 6)
            {
                BufferMesh[i] *= coef;
            }
        }
        /// <summary>
        /// Корректировка размера высоты текстуры, в буффере UV
        /// </summary>
        public void SizeAdjustmentTextureHeight(float coef)
        {
            // XYZ UV B - 6 флоатов на вершину
            for (int i = 4; i < BufferMesh.Length; i += 6)
            {
                BufferMesh[i] *= coef;
            }
        }

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
            Bones = definition.GenBones();
            ModelAnimationClips = definition.GetModelAnimationClips();

            DepthTextures = new int[Textures.Length];

            return;
        }

        #endregion

        public override string ToString() => Alias + " " 
            + (TextureSmall ? "Small" : "Big") + " " 
            + (Textures.Length == 0 ? "" : (Textures[0].Width + ":" + Textures[0].Height));
    }
}
