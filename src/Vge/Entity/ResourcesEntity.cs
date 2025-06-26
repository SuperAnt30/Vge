using System;
using Vge.Entity.Animation;
using Vge.Entity.Model;
using Vge.Json;
using Vge.Util;

namespace Vge.Entity
{
    /// <summary>
    /// Ресурсы сущности, покуда для клиента и сервера, надо продумать как разделить
    /// На сервере будет упрощённая
    /// </summary>
    public class ResourcesEntity
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
        /// Имеется ли анимация
        /// </summary>
        public bool IsAnimation { get; private set; }
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
        /// <summary>
        /// Массив данных анимации
        /// </summary>
        private AnimationData[] _animationDatas;

        public ResourcesEntity(string alias, Type entityType)
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
                        //if (json.IsKey(Cte.IsAnimations)) IsAnimation = json.GetBool();
                        if (json.IsKey(Cte.Animations))
                        {
                            IsAnimation = true;
                            _Animations(json.GetArray().ToArrayObject());
                        }
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
            definition.RunModelFromJson(model, IsAnimation);

            BufferMesh = definition.BufferMesh;
            Textures = definition.Textures;
            if (IsAnimation)
            {
                // Если только есть анимация, нужны кости и клипы
                Bones = definition.GenBones();
                ModelAnimationClips = definition.GetModelAnimationClips(_animationDatas);
                _animationDatas = null;
            }

            DepthTextures = new int[Textures.Length];

            return;
        }

        /// <summary>
        /// Собираем анимации в статах
        /// </summary>
        private void _Animations(JsonCompound[] animations)
        {
            _animationDatas = new AnimationData[animations.Length];
            for (int i = 0; i < animations.Length; i++)
            {
                _animationDatas[i] = new AnimationData(
                    animations[i].GetString(Cte.Name),
                    animations[i].GetInt(Cte.TimeMixBegin),
                    animations[i].GetInt(Cte.TimeMixEnd)
                );
            }
        }

        #endregion

        public override string ToString() => Alias + " " 
            + (TextureSmall ? "Small" : "Big") + " " 
            + (Textures.Length == 0 ? "" : (Textures[0].Width + ":" + Textures[0].Height));
    }
}
