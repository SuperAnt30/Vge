using System;
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
        /// Ограничительная рамка занимает весь блок, для оптимизации, без проверки AABB блока.
        /// Жидкости будут false
        /// </summary>
        public bool FullBlock { get; protected set; } = true;
       
        /// <summary>
        /// Является ли эта модель не блоком, со всеми сторонами и прозрачной
        /// </summary>
        //public bool IsUnique { get; protected set; } = false;
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
        /// Блок жидкости: вода, лава, нефть
        /// </summary>
        public bool Liquid = false;
        /// <summary>
        /// Параметр для жидкости, 0 или 1024
        /// </summary>
        public int LiquidOutside = 0;
        /// <summary>
        /// Параметры нет жидкости на стороне, 1024 или 0
        /// </summary>
        public int[] NotLiquidOutside = new int[] { 1024, 0, 0, 0, 0, 0 };
        /// <summary>
        /// Блок имеет альфа текстуру (полупрозрачный), попадает под отдельный слой с сортировкой
        /// </summary>
        public bool Alpha = false;
        /// <summary>
        /// Прозрачный блок, не только альфа, с прозрачной текстурой. 
        /// </summary>
        public bool Translucent = false;
        /// <summary>
        /// Флаг, если блок должен использовать самое яркое значение соседнего света как свое собственное
        /// Пример: листва, вода, стекло
        /// </summary>
        public bool UseNeighborBrightness = false;
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
        /// Отбраковка всех сторон во всех вариантах
        /// </summary>
        public bool CullFaceAll = false;
        /// <summary>
        /// Принудительное рисование всех сторон
        /// </summary>
        public bool ForceDrawFace = false;

        /// <summary>
        /// Стороны целого блока для прорисовки блока quads
        /// </summary>
        private QuadSide[][] _quads;
        /// <summary>
        /// Маска на все варианты и стороны, 4 ulong-a (256 бит)
        /// </summary>
        protected ulong[][][] _maskCullFaces;
        /// <summary>
        /// Для оптимизации отбраковка стороны, чтоб не использовать маску
        /// </summary>
        private bool[][] _cullFaces;
        /// <summary>
        /// Принудительное рисование стороны
        /// </summary>
        private bool[][] _forceDrawFaces;
        /// <summary>
        /// Принудительное рисование не крайней стороны 
        /// </summary>
        private bool[][] _forceDrawNotExtremeFaces;

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
                        if (json.IsKey(Ctb.LightOpacity)) LightOpacity = (byte)json.GetInt();
                        if (json.IsKey(Ctb.LightValue)) LightValue = (byte)json.GetInt();
                        if (json.IsKey(Ctb.Translucent)) Translucent = json.GetBool();
                        if (json.IsKey(Ctb.UseNeighborBrightness)) UseNeighborBrightness = json.GetBool();
                        if (json.IsKey(Ctb.АmbientOcclusion)) АmbientOcclusion = json.GetBool();
                        if (json.IsKey(Ctb.BiomeColor)) BiomeColor = json.GetBool();
                        if (json.IsKey(Ctb.Shadow)) Shadow = json.GetBool();
                        if (json.IsKey(Ctb.Color))
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
                _ShapeDefinition(state, shapes);
            }
        }

        /// <summary>
        /// Получить модель
        /// </summary>
        protected virtual void _ShapeDefinition(JsonCompound state, JsonCompound shapes)
        {
            BlockShapeDefinition shapeDefinition = new BlockShapeDefinition(this);
            _quads = shapeDefinition.RunShapeFromJson(state, shapes);
            BiomeColor = shapeDefinition.BiomeColor > 0 && shapeDefinition.BiomeColor < 4;
            CullFaceAll = shapeDefinition.CullFaceAll;
            FullBlock = CullFaceAll;
            ForceDrawFace = shapeDefinition.ForceDrawFace;
            _maskCullFaces = shapeDefinition.MaskCullFaces;
            _cullFaces = shapeDefinition.CullFaces;
            _forceDrawFaces = shapeDefinition.ForceDrawFaces;
            _forceDrawNotExtremeFaces = shapeDefinition.ForceDrawNotExtremeFaces;
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

        /// <summary>
        /// Проверить колизию блока на пересечение луча
        /// </summary>
        /// <param name="pos">позиция блока</param>
        /// <param name="a">точка от куда идёт лучь</param>
        /// <param name="dir">вектор луча</param>
        /// <param name="maxDist">максимальная дистания</param>
        public bool CollisionRayTrace(BlockPos pos, uint met, Vector3 a, Vector3 dir, float maxDist)
        {
            if (IsAction)
            {
                //if (FullBlock)
                if (!Liquid)
                    return true;

                //// Если блок не полный, обрабатываем хитбокс блока
                //AxisAlignedBB[] aabbs = GetCollisionBoxesToList(pos, met);
                //Vector3 pos2 = a + dir * maxDist;
                //foreach (AxisAlignedBB aabb in aabbs)
                //{
                //    if (aabb.CalculateIntercept(a, pos2) != null) return true;
                //}
            }
            return false;
        }

        #endregion

        #region Методы для группы

        /// <summary>
        /// Задать блок воздуха
        /// </summary>
        protected void SetAir()
        {
           // Material = Materials.GetMaterialCache(EnumMaterial.Air);
            IsAir = true;
            IsAction = false;
            IsParticle = false;
            АmbientOcclusion = false;
            Shadow = false;
            IsReplaceable = true;
            LightOpacity = 0;

            _quads = new QuadSide[][] { new QuadSide[] { new QuadSide(0) } };
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

        /// <summary>
        /// Получить сторону для прорисовки жидкого блока
        /// </summary>
        public virtual SideLiquid GetSideLiquid(int index) => null;

        /// <summary>
        /// Имеется ли отбраковка конкретной стороны, конкретного варианта
        /// </summary>
        public virtual bool IsCullFace(uint met, int indexSide) => _cullFaces[met][indexSide];
        /// <summary>
        /// Надо ли принудительно рисовать сторону, конкретного варианта
        /// </summary>
        public virtual bool IsForceDrawFace(uint met, int indexSide) => _forceDrawFaces[met][indexSide];
        /// <summary>
        /// Надо ли принудительно рисовать не крайнюю сторону, конкретного варианта
        /// </summary>
        public virtual bool IsForceDrawNotExtremeFace(uint met, int indexSide) => _forceDrawNotExtremeFaces[met][indexSide];

        /// <summary>
        /// Проверка масок сторон
        /// </summary>
        /// <param name="indexSide">Индекс сторонв</param>
        /// <param name="met">Мет данные проверяющего блока</param>
        /// <param name="blockSide">Объект соседнего блока</param>
        /// <param name="metSide">Мет данные соседнего блока</param>
        public virtual bool ChekMaskCullFace(int indexSide, uint met, BlockBase blockSide, uint metSide)
        {
            ulong[] mask = _maskCullFaces[met][indexSide];
            ulong[] maskCheck = blockSide._maskCullFaces[metSide][PoleConvert.Reverse[indexSide]];
            return (maskCheck[0] & mask[0]) == mask[0]
                && (maskCheck[1] & mask[1]) == mask[1]
                && (maskCheck[2] & mask[2]) == mask[2]
                && (maskCheck[3] & mask[3]) == mask[3];
        }

        #endregion

        #region Методв для освещения

        /// <summary>
        /// Сколько света вычитается для прохождения этого блока, зависящий от Metdata
        /// </summary>
        public virtual int GetLightOpacity(int met) => LightOpacity;
        /// <summary>
        /// Количество излучаемого света (плафон), зависящий от Metdata
        /// </summary>
        public virtual int GetLightValue(int met) => LightValue;

        #endregion

        /// <summary>
        /// Действие перед размещеннием блока, для определения метданных
        /// </summary>
        public virtual BlockState OnBlockPlaced(WorldBase worldIn, BlockPos blockPos, BlockState state, Pole side, Vector3 facing)
            => state.NewMet(0);

        public override string ToString() => Id.ToString() + " " + Alias;
    }
}
