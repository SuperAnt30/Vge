using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Animation;
using Vge.Entity.Model;
using Vge.Entity.Render;
using Vge.Json;
using Vge.World.Block;

namespace Vge.Entity
{
    /// <summary>
    /// Ресурсы сущности, нужны везде, но форма только на клиенте
    /// </summary>
    public class ResourcesEntity
    {
        /// <summary>
        /// Индекс сущности из таблицы
        /// </summary>
        public ushort IndexEntity { get; private set; }
        /// <summary>
        /// Название сущности
        /// </summary>
        public readonly string Alias;
        /// <summary>
        /// Тип объекта сущности
        /// </summary>
        public readonly Type EntityType;
        
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
        /// Буфер сетки моба по умолчанию, для рендера
        /// </summary>
        public VertexEntityBuffer BufferMesh { get; private set; }

        /// <summary>
        /// Индекс формы
        /// </summary>
        private readonly ushort _indexShape;
        /// <summary>
        /// Индекс текстуры
        /// </summary>
        private ushort _indexTexture;

        public ResourcesEntity(string alias, Type entityType, ushort indexShape)
        {
            Alias = alias;
            EntityType = entityType;
            _indexShape = indexShape;
        }

        /// <summary>
        /// Задать индекс сущности, из таблицы
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetIndex(ushort id) => IndexEntity = id;

        /// <summary>
        /// Получить параметр глубины текстуры для шейдера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetDepthTexture() => EntitiesReg.Shapes[_indexShape].DepthTextures[_indexTexture];

        /// <summary>
        /// Получить параметр группы текстуры и есть ли анимация
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetSmallAnimation() => EntitiesReg.Shapes[_indexShape].TextureSmall ? 0 : 1
            + (IsAnimation ? 2 : 0);

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
                    //foreach (JsonKeyValue json in state.Items)
                    //{
                    //    //if (json.IsKey(Ctb.Translucent)) Translucent = json.GetBool();
                    //    //if (json.IsKey(Ctb.UseNeighborBrightness)) UseNeighborBrightness = json.GetBool();
                    //    //if (json.IsKey(Ctb.АmbientOcclusion)) АmbientOcclusion = json.GetBool();
                    //    //if (json.IsKey(Ctb.BiomeColor)) BiomeColor = json.GetBool();
                    //    //if (json.IsKey(Ctb.Shadow)) Shadow = json.GetBool();
                    //    //if (json.IsKey(Ctb.Color))
                    //    //{
                    //    //    float[] ar = json.GetArray().ToArrayFloat();
                    //    //    Color = new Vector3(ar[0], ar[1], ar[2]);
                    //    //}
                    //}
                }
                catch
                {
                    throw new Exception(Sr.GetString(Sr.ErrorReadJsonEntityStat, Alias));
                }
            }
        }

        /// <summary>
        /// Прочесть состояние для клиента из Json формы
        /// </summary>
        public void ReadStateClientFromJson(JsonCompound state)
        {
            float scale = 1;
            // Массив данных анимации
            AnimationData[] animationDatas = new AnimationData[0];
            if (state.Items != null)
            {
                try
                {
                    // Статы
                    foreach (JsonKeyValue json in state.Items)
                    {
                        if (json.IsKey(Cte.Animations))
                        {
                            IsAnimation = true;
                            // Собираем анимации в статах
                            JsonCompound[] animations = json.GetArray().ToArrayObject();
                            animationDatas = new AnimationData[animations.Length];
                            for (int i = 0; i < animations.Length; i++)
                            {
                                animationDatas[i] = new AnimationData(
                                    animations[i].GetString(Cte.Name),
                                    animations[i].GetFloat(Cte.AnimationSpeed)
                                );
                            }
                        }
                        else if (json.IsKey(Cte.Scale))
                        {
                            scale = json.GetFloat();
                            if (scale == 0) scale = 1;
                        }
                        else if (json.IsKey(Ctb.Texture)) _indexTexture = (ushort)json.GetInt();
                    }
                }
                catch
                {
                    throw new Exception(Sr.GetString(Sr.ErrorReadJsonEntityStat, Alias));
                }
            }

            // Копия буффера
            ShapeEntity shapeEntity = EntitiesReg.Shapes[_indexShape];
            BufferMesh = shapeEntity.CopyBufferFloatMesh(scale);
            if (IsAnimation)
            {
                // Если только есть анимация, нужны кости и клипы
                Bones = shapeEntity.GenBones(scale);
                ModelAnimationClips = shapeEntity.GetModelAnimationClips(animationDatas);
            }
        }

        public override string ToString() => Alias + " IndexShape:" + _indexShape;
    }
}
