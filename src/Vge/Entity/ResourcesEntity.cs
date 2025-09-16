using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Animation;
using Vge.Entity.Model;
using Vge.Entity.Render;
using Vge.Entity.Shape;
using Vge.Json;

namespace Vge.Entity
{
    /// <summary>
    /// Ресурсы сущности, нужны везде, но форма только на клиенте
    /// </summary>
    public class ResourcesEntity : ResourcesEntityBase
    {
        /// <summary>
        /// Имеется ли анимация
        /// </summary>
        public bool IsAnimation { get; private set; }
        /// <summary>
        /// Только двигаться для тригерров анимации, вместо Forward, Back, Left, Right.
        /// </summary>
        public bool OnlyMove { get; private set; }
        /// <summary>
        /// Интервал между морганием глаз в игровых тиках, если равно 0, значит нет глаз
        /// </summary>
        public int BlinkEye { get; private set; }
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
        /// Масштаб формы
        /// </summary>
        public float Scale { get; private set; }
        /// <summary>
        /// Индекс глубины текстуры
        /// </summary>
        public int DepthTexture { get; private set; } = -1;
        /// <summary>
        /// Количество предметов которые может держать, (не одежда)
        /// </summary>
        public byte CountPositionItem { get; private set; } = 0;
        /// <summary>
        /// Имя слоёв одежды
        /// </summary>
        public string NameShapeLayers { get; private set; } = "";

        /// <summary>
        /// Индекс формы
        /// </summary>
        private readonly ushort _indexShape;
        /// <summary>
        /// Индекс текстуры
        /// </summary>
        private ushort _indexTexture;

        public ResourcesEntity(string alias, Type entityType, ushort indexShape)
            : base (alias, entityType) => _indexShape = indexShape;

        /// <summary>
        /// Получить параметр глубины текстуры и параметр группы текстуры для шейдера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetDepthTextureAndSmall()
        {
            if (DepthTexture == -1)
            {
                DepthTexture = EntitiesReg.Shapes[_indexShape].DepthTextures[_indexTexture];
            }
            return DepthTexture;
        }

        /// <summary>
        /// Получить параметр есть ли анимация для шейдора
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetIsAnimation() => IsAnimation ? 1 : 0;

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
            Scale = 1;
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
                                string code = animations[i].GetString(Cte.Code);
                                if (code == "") code = i.ToString();
                                AnimationData animationData = new AnimationData(code,
                                    animations[i].GetString(Cte.Animation),
                                    animations[i].GetFloat(Cte.AnimationSpeed),
                                    animations[i].GetArray(Cte.TriggeredBy).ToArrayString()
                                );

                                if (animations[i].IsKey(Cte.ElementWeight))
                                {
                                    // Имеются веса для костей
                                    JsonCompound elementsWeight = animations[i].GetObject(Cte.ElementWeight);
                                    foreach (JsonKeyValue elementWeight in elementsWeight.Items)
                                    {
                                        animationData.ElementWeight.Add(
                                            elementWeight.Key,
                                            (byte)elementWeight.GetInt());
                                    }
                                }
                                animationDatas[i] = animationData;
                            }
                        }
                        else if (json.IsKey(Cte.Scale))
                        {
                            Scale = json.GetFloat();
                            if (Scale == 0) Scale = 1;
                        }
                        else if (json.IsKey(Cte.TextureId)) _indexTexture = (ushort)json.GetInt();
                        else if (json.IsKey(Cte.OnlyMove)) OnlyMove = json.GetBool();
                        else if (json.IsKey(Cte.BlinkEye)) BlinkEye = json.GetInt();
                        else if (json.IsKey(Cte.NameShapeLayers)) NameShapeLayers = json.GetString();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(Sr.GetString(Sr.ErrorReadJsonEntityStat, Alias));
                }
            }

            // Копия буффера
            ShapeEntity shapeEntity = EntitiesReg.Shapes[_indexShape];
            BufferMesh = shapeEntity.CopyBufferFloatMesh(Scale);
            if (IsAnimation)
            {
                // Если только есть анимация, нужны кости и клипы
                Bones = shapeEntity.GenBones(Scale);
                ModelAnimationClips = shapeEntity.GetModelAnimationClips(animationDatas);
                foreach (Bone bone in Bones)
                {
                    if (bone.NumberHold > CountPositionItem)
                    {
                        CountPositionItem = bone.NumberHold;
                    }
                }
            }
        }

        /// <summary>
        /// Заменить буфер из-за смены размера текстуры
        /// </summary>
        public override void SetBufferMeshBecauseSizeTexture(ShapeEntity shapeEntity)
        {
            if (_indexShape == shapeEntity.Index)
            {
                BufferMesh = shapeEntity.CopyBufferFloatMesh(Scale);
            }
        }

        public override string ToString() => Alias + " IndexShape:" + _indexShape;
    }
}
