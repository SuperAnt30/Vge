using System;
using System.Runtime.CompilerServices;
using Vge.Json;
using Vge.Realms;
using Vge.Renderer.World;
using Vge.Util;
using Vge.World.Chunk;
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
        public ushort IndexBlock { get; private set; }
        /// <summary>
        /// Псевдоним блока из таблицы
        /// </summary>
        public string Alias { get; private set; }

        /// <summary>
        /// Блок рендера
        /// </summary>
        public BlockRenderFull BlockRender;

        /// <summary>
        /// Объект материала
        /// </summary>
        public readonly IMaterial Material;

        /// <summary>
        /// Явлыется ли блок небом
        /// </summary>
        public bool IsAir { get; private set; } = false;

        #region Для логики

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
        public bool IsNotTransparent { get; private set; }
        /// <summary>
        /// Индекс картинки частички
        /// </summary>
        public int Particle { get; protected set; } = 0;
        /// <summary>
        /// Имеется ли у блока частичка
        /// </summary>
        public bool IsParticle { get; protected set; } = true;
        /// <summary>
        /// Индекс сокрощённого списка блоков жидкостей, нужен для оптимизации
        /// </summary>
        public byte IndexLiquid { get; protected set; }

        #region Для физики

        /// <summary>
        /// Можно ли выбирать блок
        /// </summary>
        public bool IsAction { get; protected set; } = true;
        /// <summary>
        /// Может ли блок сталкиваться
        /// </summary>
        public bool IsCollidable { get; private set; } = true;
        /// <summary>
        /// Ограничительная рамка занимает весь блок, для оптимизации, без проверки AABB блока.
        /// Жидкости будут false
        /// </summary>
        public bool FullBlock { get; protected set; } = true;

        #endregion

        #region Для Render

        /// <summary>
        /// Блок жидкости: вода, лава, нефть. Не доп жидкость!!!
        /// </summary>
        public bool Liquid { get; protected set; } = false;
        /// <summary>
        /// Параметр для жидкости, 0 или 1024
        /// </summary>
        public int LiquidOutside = 0;
        /// <summary>
        /// Параметры нет жидкости на стороне, 1024 или 0
        /// </summary>
        public int[] NotLiquidOutside = new int[] { 1024, 0, 0, 0, 0, 0 };
        /// <summary>
        /// Блок имеет альфа текстуру (полупрозрачный), попадает под отдельный слой которой доступна сортировка
        /// </summary>
        public bool Alpha = false;
        /// <summary>
        /// Блок имеет альфа текстуру (полупрозрачный), попадает под отдельный слой с сортировкой, если Alpha = true
        /// </summary>
        public bool AlphaSort = false;
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
        public bool Shadow { get; private set; } = true;

        /// <summary>
        /// Отбраковка всех сторон во всех вариантах
        /// </summary>
        public bool CullFaceAll = false;
        /// <summary>
        /// Принудительное рисование всех сторон
        /// </summary>
        public bool ForceDrawFace = false;

        /// <summary>
        /// Массив сторон прямоугольных форм
        /// </summary>
        protected QuadSide[][] _quads;
        /// <summary>
        /// Маска на все варианты и стороны, 4 ulong-a (256 бит)
        /// </summary>
        protected ulong[][][] _maskCullFaces;
        /// <summary>
        /// Для оптимизации отбраковка стороны, чтоб не использовать маску
        /// </summary>
        protected bool[][] _cullFaces;
        /// <summary>
        /// Принудительное рисование стороны
        /// </summary>
        protected bool[][] _forceDrawFaces;
        /// <summary>
        /// Принудительное рисование не крайней стороны 
        /// </summary>
        protected bool[][] _forceDrawNotExtremeFaces;

        #endregion

        #region Физические и химические Свойства блока

        /// <summary>
        /// Устойчивость блоков к взрывам.
        /// 10 камень, 5 дерево, руда, 0.6 стекло, 0.5 земля, песок, 0.2 листва, 0.0 трава, саженцы 
        /// </summary>
        public float Resistance { get; private set; }
        /// <summary>
        /// Скользкость
        /// 0.6 стандартная
        /// 0.8 медленее, типа по песку
        /// 0.98 по льду, скользко
        /// </summary>
        public float Slipperiness { get; private set; } = .6f;
        /// <summary>
        /// Горючесть материала
        /// </summary>
        public bool Combustibility { get; private set; } = false;
        /// <summary>
        /// Увеличить шансы загорания 0-100 %
        /// 100 мгновенно загарается без рандома
        /// 99 уже через рандом, не так быстро загорается но шанс очень большой
        /// Из-за долго жизни огня, 30 как правило загорится если вверх, рядом 1/1
        /// </summary>
        public byte IgniteOddsSunbathing { get; private set; }
        /// <summary>
        /// Шансы на сжигание 0-100 %
        /// 100 мгновенно згарает без рандома
        /// 99 уже через рандом, не так быстро сгорит но шанс очень большой
        /// Из-за долго жизни огня, 60 как правило сгорит
        /// </summary>
        public byte BurnOdds { get; private set; }
        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в ударах
        /// </summary>
        public int Hardness { get; private set; }

        #endregion

        public BlockBase(IMaterial material) => Material = material;

        #region Init

        /// <summary>
        /// Инициализация объекта рендера для блока
        /// </summary>
        protected virtual void _InitBlockRender()
            => BlockRender = Gi.BlockRendFull;

        /// <summary>
        /// Инициализация блоков, псевдоним и данные с json
        /// </summary>
        public void InitAliasAndJoinN1(string alias, JsonCompound state, JsonCompound shapes)
        {
            Alias = alias;
            if (state.Items != null)
            {
                _ReadStateFromJson(state);
                // Фигура
                _ShapeDefinition(state, shapes);
            } 
            else
            {
                // Отсутствует фигура блока в json
                throw new Exception(Sr.GetString(Sr.FileMissingJsonBlock, alias));
            }

            _InitBlockRender();
            // Задать что блок не прозрачный
            if (LightOpacity > 13) IsNotTransparent = true;
        }

        /// <summary>
        /// Инициализация блока Воздух
        /// </summary>
        public void InitAir()
        {
            Alias = "Air";
            _InitBlockRender();
            // Задать что блок не прозрачный
            if (LightOpacity > 13) IsNotTransparent = true;
        }

        /// <summary>
        /// Задать индекс блока, из таблицы
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetIndex(ushort id) => IndexBlock = id;

        /// <summary>
        /// Дополнительная инициализация блока после инициализации предметов и корректировки id блоков
        /// </summary>
        public virtual void InitAfterItems() { }

        #endregion

        #region Методы для импорта данных с json

        /// <summary>
        /// Прочесть состояние блока из Json формы
        /// </summary>
        private void _ReadStateFromJson(JsonCompound state)
        {
            try
            {
                // Статы
                foreach (JsonKeyValue json in state.Items)
                {
                    if (json.IsKey(Ctb.NeedsRandomTick)) NeedsRandomTick = json.GetBool();
                    if (json.IsKey(Ctb.LightOpacity)) LightOpacity = (byte)json.GetInt();
                    if (json.IsKey(Ctb.LightValue)) LightValue = (byte)json.GetInt();
                    if (json.IsKey(Ctb.Translucent)) Translucent = json.GetBool();
                    if (json.IsKey(Ctb.UseNeighborBrightness)) UseNeighborBrightness = json.GetBool();
                    if (json.IsKey(Ctb.АmbientOcclusion)) АmbientOcclusion = json.GetBool();
                    if (json.IsKey(Ctb.BiomeColor)) BiomeColor = json.GetBool();
                    if (json.IsKey(Ctb.Shadow)) Shadow = json.GetBool();
                    if (json.IsKey(Ctb.NoCollision)) IsCollidable = !json.GetBool();
                    if (json.IsKey(Ctb.Hardness)) Hardness = json.GetInt();
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
        }

        /// <summary>
        /// Получить модель
        /// </summary>
        protected virtual void _ShapeDefinition(JsonCompound state, JsonCompound shapes)
        {
            BlockShapeDefinition shapeDefinition = new BlockShapeDefinition(Alias);
            _quads = shapeDefinition.RunShapeBlockFromJson(state, shapes);
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

        #region Методы для жидкости

        /// <summary>
        /// Задать индекс сокрощённого списка блоков жидкостей, нужен для оптимизации
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetIndexLiquid(byte id) => IndexLiquid = id;

        /// <summary>
        /// Может ли быть дополнительная жидкость в этом блоке
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool CanAddLiquid() => !FullBlock && !Liquid;

        /// <summary>
        /// Имеется ли дополнительная жидкость в этом блоке
        /// NLLL mmmm MMMM MMMM MMMM //BBBB BBBB BBBB
        /// N - не используется
        /// L - id блока жидкости через доп массив индексов
        /// m - met данные жидкости
        /// M - met данные блока
        /// B - id блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsAddLiquid(int met) => met >> 16 != 0;

        /// <summary>
        /// Получить дополнительный блок жидкости
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockBase GetAddLiquid(int met) => Ce.Blocks.GetAddLiquid(met);

        /// <summary>
        /// Получить мет данные дополнительной жидкости
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetAddLiquidMet(int met) => (met >> 12) & 0xF;

        /// <summary>
        /// Сгенерировать параметры блока, внося данные доп жидкости
        /// </summary>
        /// <param name="met"></param>
        //public int GenMetdataAddLiquid(int met)
        //{

        //}

        #endregion

        #region Методы для физики

        /// <summary>
        /// Является ли блок проходимым, т.е. можно ли ходить через него
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool IsPassable(uint met) => false;

        /// <summary>
        /// Является ли блок проходимым на нём, т.е. можно ли ходить по нему
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool IsPassableOnIt(uint met) => !IsPassable(met);

        /// <summary>
        /// Проверить коллизию блока на пересечение луча
        /// </summary>
        /// <param name="pos">позиция блока</param>
        /// <param name="a">точка от куда идёт лучь</param>
        /// <param name="dir">вектор луча</param>
        /// <param name="maxDist">максимальная дистания</param>
        public bool CollisionRayTrace(BlockPos pos, int met, 
            float px, float py, float pz, Vector3 dir, float maxDist)
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

        /// <summary>
        /// Передать список  ограничительных рамок блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual AxisAlignedBB[] GetCollisionBoxesToList(BlockPos pos, int met)
            => new AxisAlignedBB[] { new AxisAlignedBB(pos.X, pos.Y, pos.Z, pos.X + 1, pos.Y + 1, pos.Z + 1) };

        #endregion

        #region Методы для группы

        /// <summary>
        /// Задать блок воздуха
        /// </summary>
        protected void _SetAir()
        {
           // Material = Materials.GetMaterialCache(EnumMaterial.Air);
            IsAir = true;
            IsAction = false;
            IsParticle = false;
            АmbientOcclusion = false;
            Shadow = false;
            IsReplaceable = true;
            LightOpacity = 0;
            Translucent = true;
            IsCollidable = false;
            FullBlock = false;

            _quads = new QuadSide[][] { new QuadSide[] { new QuadSide(0) } };
        }

        #endregion

        #region Методы для Render

        /// <summary>
        /// Получить перечень сторон по индексу
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected QuadSide[] _GetQuads(int index) => _quads[index];

        /// <summary>
        /// Массив сторон прямоугольных форм для рендера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual QuadSide[] GetQuads(int met, int xb, int zb) => _quads[0];

        /// <summary>
        /// Получить сторону для прорисовки жидкого блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual SideLiquid GetSideLiquid(int index) => null;

        /// <summary>
        /// Имеется ли отбраковка конкретной стороны, конкретного варианта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool IsCullFace(int met, int indexSide) => _cullFaces[met & 0xFF][indexSide];
        /// <summary>
        /// Надо ли принудительно рисовать сторону, конкретного варианта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool IsForceDrawFace(int met, int indexSide) => _forceDrawFaces[met & 0xFF][indexSide];
        /// <summary>
        /// Надо ли принудительно рисовать не крайнюю сторону, конкретного варианта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool IsForceDrawNotExtremeFace(int met, int indexSide) => _forceDrawNotExtremeFaces[met & 0xFF][indexSide];

        /// <summary>
        /// Проверка масок сторон
        /// </summary>
        /// <param name="indexSide">Индекс сторонв</param>
        /// <param name="met">Мет данные проверяющего блока</param>
        /// <param name="blockSide">Объект соседнего блока</param>
        /// <param name="metSide">Мет данные соседнего блока</param>
        public virtual bool ChekMaskCullFace(int indexSide, int met, BlockBase blockSide, int metSide)
        {
            ulong[] mask = _maskCullFaces[met & 0xFF][indexSide];
            ulong[] maskCheck = blockSide._maskCullFaces[metSide & 0xFF][PoleConvert.Reverse[indexSide]];
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual int GetLightOpacity(int met) => LightOpacity;
        /// <summary>
        /// Количество излучаемого света (плафон), зависящий от Metdata
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual int GetLightValue(int met) => LightValue;

        #endregion

        #region Методы для тиков

        /// <summary>
        /// Случайный эффект частички и/или звука на блоке только для клиента
        /// </summary>
        public virtual void RandomDisplayTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random) { }

        /// <summary>
        /// Случайный эффект блока, для сервера
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void RandomTick(WorldServer world, ChunkServer chunk, 
            BlockPos blockPos, BlockState blockState, Rand random)
            => UpdateTick(world, chunk, blockPos, blockState, random);

        /// <summary>
        /// Обновить блок в такте
        /// </summary>
        public virtual void UpdateTick(WorldServer world, ChunkServer chunk,
            BlockPos blockPos, BlockState blockState, Rand random) { }

        #endregion

        #region Действия с блоком

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public virtual void NeighborBlockChange(WorldServer world, ChunkServer chunk, 
            BlockPos blockPos, BlockState blockState, BlockBase neighborBlock)
        {
            if (IsAddLiquid(blockState.Met))
            {
                // Если имеется жидкость, надо тикать жидкость
                world.SetBlockAddLiquidTick(blockPos, 4);
            }
        }

        /// <summary> 
        /// Проверка установка блока, можно ли его установить тут
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool CanBlockStay(WorldServer world, ChunkServer chunk, 
            BlockPos blockPos, int met = 0) => true;



        /// <summary>
        /// Действие перед размещеннием блока, для определения метданных
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual BlockState OnBlockPlaced(WorldServer world, BlockPos blockPos, 
            BlockState blockState, Pole side, Vector3 facing) => blockState;

        /// <summary>
        /// Действие блока после его удаления
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void OnBreakBlock(WorldServer world, ChunkServer chunk, 
            BlockPos blockPos, BlockState stateOld, BlockState stateNew) { }

        ///// <summary>
        ///// Спавн предмета при разрушении этого блока
        ///// </summary>
        //protected virtual void _DropBlockAsItemServer(WorldServer world, Rand rand, BlockPos blockPos,
        //    BlockState state)
        //{
        //    ItemStack.SpawnAsEntity(world, blockPos, new ItemStack(Items.GetItemCache(state.id)));
        //}

        /// <summary>
        /// Спавн предмета при разрушении этого блока
        /// </summary>
        public virtual void DropBlockAsItem(WorldServer world, BlockPos blockPos, BlockState state) { }

        #endregion

        /// <summary>
        /// Получить строку мет данных, для отладки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToInfo(int met)
        {
            if (CanAddLiquid() && IsAddLiquid(met))
            {
                return ToString() + " M:" 
                    + (met & 0xFFF) + ChatStyle.Blue
                    + " Liquid " + Ce.Blocks.GetAddLiquid(met) + " M:" + GetAddLiquidMet(met)
                    + ChatStyle.Reset;
            }
            return ToString() + " M:" + met.ToString();
        }

        public int CountShape() => _quads == null ? 0 : _quads.Length;

        public override string ToString() => IndexBlock.ToString() + " " + Alias;
    }
}
