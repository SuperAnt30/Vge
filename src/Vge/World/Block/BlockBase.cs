using System;
using System.Collections.Generic;
using System.Diagnostics;
using Vge.Json;
using Vge.Renderer.World;
using Vge.Util;
using WinGL.Util;

namespace Vge.World.Block
{
    /// <summary>
    /// Базовый объект Блока
    /// </summary>
    public class BlockBase
    {
        /// <summary>
        /// Индекс блока из таблицы
        /// </summary>
        public ushort Id { get; private set; }
        /// <summary>
        /// Псевдоним блока из таблицы
        /// </summary>
        public string Alias { get; private set; }

        /// <summary>
        /// Блок рендера
        /// </summary>
        public BlockRenderFull BlockRender;

        #region Группа

        // У блока может быть один из параметров true FullBlock || Liquid || IsUnique || IsAir

        /// <summary>
        /// Ограничительная рамка занимает весь блок, для оптимизации, без проверки AABB блока
        /// </summary>
        public bool FullBlock { get; protected set; } = true;
        /// <summary>
        /// Блок жидкости: вода, лава, нефть
        /// </summary>
        public bool Liquid { get; private set; } = false;
        /// <summary>
        /// Является ли эта модель не блоком, со всеми сторонами и прозрачной
        /// </summary>
        public bool IsUnique { get; protected set; } = false;
        /// <summary>
        /// Явлыется ли блок небом
        /// </summary>
        public bool IsAir { get; private set; } = false;

        #endregion

        #region Для логики

        /// <summary>
        /// Имеет ли блок данные
        /// </summary>
        public bool IsMetadata;

        /// <summary>
        /// Сколько света вычитается для прохождения этого блока Air = 0
        /// В VoxelEngine он в public static byte GetBlockLightOpacity(EnumBlock eblock)
        /// получть инфу не создавая блок
        /// </summary>
        public byte LightOpacity { get; protected set; } = 15;
        /// <summary>
        /// Количество излучаемого света (плафон)
        /// </summary>
        public int LightValue { get; protected set; }
        /// <summary>
        /// Отмечает, относится ли этот блок к типу, требующему случайной пометки в тиках. 
        /// Объект ChunkStorage подсчитывает блоки, чтобы в целях эффективности отобрать фрагмент из 
        /// случайного списка обновлений фрагментов.
        /// </summary>
        public bool NeedsRandomTick { get; protected set; }
        /// <summary>
        /// Может на этот блок поставить другой, к примеру трава
        /// </summary>
        public bool IsReplaceable { get; protected set; } = false;

        #endregion

        /// <summary>
        /// Блок не прозрачный
        /// Для рендера и RayCast
        /// </summary>
        public bool IsNotTransparent { get; protected set; }
        /// <summary>
        /// Индекс картинки частички
        /// </summary>
        public int Particle { get; protected set; } = 0;
        /// <summary>
        /// Имеется ли у блока частичка
        /// </summary>
        public bool IsParticle { get; protected set; } = true;

        #region Для физики

        /// <summary>
        /// Можно ли выбирать блок
        /// </summary>
        public bool IsAction { get; protected set; } = true;
        /// <summary>
        /// Может ли блок сталкиваться
        /// </summary>
        public bool IsCollidable { get; protected set; } = true;

        #endregion

        #region Для Render

        /// <summary>
        /// Прорисовка возможно с обеих сторон, для уникальных блоков, типа трава, листва и подобное
        /// </summary>
        //public bool BothSides { get; private set; } = false;
        /// <summary>
        /// Полупрозрачный, альфа блок, вода, стекло...
        /// </summary>
        public bool Translucent = false;

        /// <summary>
        /// Флаг, если блок должен использовать самое яркое значение соседнего света как свое собственное
        /// Пример: листва, вода, стекло
        /// </summary>
        public bool UseNeighborBrightness = false;

        /// <summary>
        /// Все стороны принудительно, пример: трава, стекло, вода, лава
        /// *** Продумать, возможно заменить...
        /// </summary>
        public bool AllSideForcibly = false;
        /// <summary>
        /// При значении flase у AllSideForcibly + обнотипные блоков не будет между собой сетки, пример: вода, блок стекла
        /// *** Продумать, возможно заменить...
        /// </summary>
        public bool BlocksNotSame = true;

        /// <summary>
        /// Обрабатывается блок эффектом АmbientOcclusion
        /// </summary>
        public bool АmbientOcclusion = true;
        /// <summary>
        /// Обрабатывается блок эффектом Плавного перехода цвета между биомами
        /// </summary>
        public bool BiomeColor = false;
        /// <summary>
        /// Цвет блока, используется при BiomeColor = true;
        /// </summary>
        public Vector3 Color = new Vector3(1);
        /// <summary>
        /// Может ли быть тень сущности на блоке, только для целых блоков
        /// </summary>
        public bool Shadow { get; protected set; } = true;

        /// <summary>
        /// Стороны целого блока для прорисовки блока quads
        /// </summary>
        private QuadSide[][] _quads = new QuadSide[][] { new QuadSide[] { new QuadSide() } };

        #endregion

        #region Init

        /// <summary>
        /// Инициализация объекта рендера для блока
        /// </summary>
        protected virtual void _InitBlockRender()
            => BlockRender = Gi.BlockRendFull;

        /// <summary>
        /// Инициализация блоков, псевдоним и данные с json
        /// </summary>
        public void InitAliasAndJoinN1(string alias, JsonCompound state, JsonCompound model)
        {
            Alias = alias;
            _ReadStateFromJson(state, model);
            _InitBlockRender();
            // Задать что блок не прозрачный
            if (LightOpacity > 13) IsNotTransparent = true;
        }

        /// <summary>
        /// Инициализация id после корректировки карты ID блоков
        /// </summary>
        public void InitIdN2(ushort id)
        {
            Id = id;
        }

        /// <summary>
        /// Дополнительная инициализация блока после инициализации предметов
        /// </summary>
        public virtual void InitializationAfterItemsN3() { }

        #endregion

        #region Методы для импорта данных с json

        /// <summary>
        /// Прочесть состояние блока из Json формы
        /// </summary>
        private void _ReadStateFromJson(JsonCompound state, JsonCompound shapes)
        {
            if (state.Items != null)
            {
                try
                {
                    // Статы
                    foreach (JsonKeyValue json in state.Items)
                    {
                        if (json.IsKey("LightOpacity")) LightOpacity = (byte)json.GetInt();
                        if (json.IsKey("LightValue")) LightValue = (byte)json.GetInt();
                        if (json.IsKey("Translucent")) Translucent = json.GetBool();
                        if (json.IsKey("АmbientOcclusion")) АmbientOcclusion = json.GetBool();
                        if (json.IsKey("BiomeColor")) BiomeColor = json.GetBool();
                        if (json.IsKey("Shadow")) Shadow = json.GetBool();
                        if (json.IsKey("Color"))
                        {
                            float[] ar = json.GetArray().ToArrayFloat();
                            Color = new Vector3(ar[0], ar[1], ar[2]);
                        }
                    }
                }
                catch
                {
                    throw new Exception(Sr.GetString(Sr.ErrorReadJsonBlockStat, Alias));
                }
                // Модель
                BlockShapeDefinition shapeDefinition = new BlockShapeDefinition(this);
                _quads = shapeDefinition.RunShapeFromJson(state, shapes);
                BiomeColor = shapeDefinition.BiomeColor > 0 && shapeDefinition.BiomeColor < 4;
            }
        }

        #endregion

        #region Методы для физики

        /// <summary>
        /// Является ли блок проходимым, т.е. можно ли ходить через него
        /// </summary>
        public virtual bool IsPassable(uint met) => false;

        /// <summary>
        /// Является ли блок проходимым на нём, т.е. можно ли ходить по нему
        /// </summary>
        public virtual bool IsPassableOnIt(uint met) => !IsPassable(met);

        #endregion

        #region Методы для группы

        /// <summary>
        /// Задать блок воздуха
        /// </summary>
        protected void SetAir()
        {
           // Material = Materials.GetMaterialCache(EnumMaterial.Air);
            IsAir = true;
            FullBlock = false;
            IsAction = false;
            IsParticle = false;
            АmbientOcclusion = false;
            Shadow = false;
            IsReplaceable = true;
            LightOpacity = 0;
            //canDropPresent = false;
        }

        #endregion

        #region Методы для Render

        /// <summary>
        /// Получить перечень сторон по индексу
        /// </summary>
        protected QuadSide[] _GetQuads(int index) => _quads[index];

        /// <summary>
        /// Стороны целого блока для рендера
        /// </summary>
        public virtual QuadSide[] GetQuads(uint met, int xb, int zb) => _quads[0];

        #endregion

        

        public override string ToString() => Id.ToString() + " " + Alias;
    }
}
