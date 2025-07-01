using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Animation;
using Vge.Entity.Model;
using Vge.Json;

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
        /// Индекс формы
        /// </summary>
        public readonly ushort IndexShape;

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
        public float[] BufferMesh { get; private set; }

        public ResourcesEntity(string alias, Type entityType, ushort indexShape)
        {
            Alias = alias;
            EntityType = entityType;
            IndexShape = indexShape;
        }

        /// <summary>
        /// Задать индекс сущности, из таблицы
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetIndex(ushort id) => IndexEntity = id;

        /// <summary>
        /// Получить объект формы
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ShapeEntity GetShape() => EntitiesReg.Shapes[IndexShape];

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
                        if (json.IsKey(Cte.Scale))
                        {
                            scale = json.GetFloat();
                            if (scale == 0) scale = 1;
                        }
                    }
                }
                catch
                {
                    throw new Exception(Sr.GetString(Sr.ErrorReadJsonEntityStat, Alias));
                }
            }

            // Копия буффера
            BufferMesh = GetShape().CopyBufferMesh(scale);
            if (IsAnimation)
            {
                // Если только есть анимация, нужны кости и клипы
                Bones = GetShape().GenBones(scale);
                ModelAnimationClips = GetShape().GetModelAnimationClips(animationDatas);
            }
        }

        public override string ToString() => Alias + " IndexShape:" + IndexShape;
    }
}
