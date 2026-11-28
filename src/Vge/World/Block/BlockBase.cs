using System;
using System.Runtime.CompilerServices;
using Vge.Entity;
using Vge.Entity.Particle;
using Vge.Entity.Physics;
using Vge.Entity.Player;
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
        public readonly MaterialBase Material;

        /// <summary>
        /// Явлыется ли блок небом
        /// </summary>
        public bool IsAir { get; private set; } = false;

        #region Для логики

        /// <summary>
        /// Сколько света вычитается для прохождения этого блока Air = 0
        /// Количество излучаемого света (плафон)
        /// LightOpacity << 4 | LightValue & 0xF
        /// </summary>
        public byte LightOpacityValue { get; protected set; } = 0xF0;
        /// <summary>
        /// Отмечает, относится ли этот блок к типу, требующему случайной пометки в тиках. 
        /// Объект ChunkStorage подсчитывает блоки, чтобы в целях эффективности отобрать фрагмент из 
        /// случайного списка обновлений фрагментов.
        /// </summary>
        public bool NeedsRandomTick { get; protected set; }
        /// <summary>
        /// Блок отмечен, что требуется обязательный случайный тик частичек, не зависимо от дистанции игрока.
        /// Только для клиента
        /// </summary>
        public bool RequiredRandomTick { get; protected set; }
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
        /// Имеется ли у блока частичка
        /// </summary>
        public bool IsParticle { get; protected set; } = false;
        /// <summary>
        /// Массив различных цветов частичек блока
        /// </summary>
        public Vector3[] ParticleColors { get; protected set; }
        /// <summary>
        /// Количесто частичек при разрушении блока
        /// </summary>
        public byte ParticleCount { get; protected set; } = 50;
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
        /// <summary>
        /// Имеются ли замедления
        /// </summary>
        public bool IsSlow { get; private set; }
        /// <summary>
        /// Коэффициент замедления по X и Z,
        /// 1 нет замедления, 0 максимальное, больше 1 ускорение
        /// </summary>
        public float FactorSlowXZ { get; private set; }
        /// <summary>
        /// Коэффициент замедления по Y,
        /// 1 нет замедления, 0 максимальное, больше 1 ускорение
        /// </summary>
        public float FactorSlowY { get; private set; }
        /// <summary>
        /// Импульс для жидкостей, для течения
        /// </summary>
        public float LiquidImpulse { get; private set; }

        /// <summary>
        /// Все блоки, или массив AxisAlignedBB
        /// </summary>
        protected AxisAlignedBB[][] _fullCube = { new AxisAlignedBB[] { new AxisAlignedBB(0, 0, 0, 1, 1, 1) } };
        /// <summary>
        /// Массив точек линий хитбокса
        /// </summary>
        protected Vector3[][] _hitbox;

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
        /// Игнорировать собственного типа, пример огонь между собой
        /// </summary>
        public bool IgnoreOwn = false;
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
        /// Вторая текстура трещины разрушения, альфа
        /// </summary>
        public bool IsDestorySecond;

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
        /// Является ли блок проходимым, т.е. можно ли ходить через него
        /// </summary>
        public bool Passable { get; private set; }
        /// <summary>
        /// Устойчивость блоков к взрывам.
        /// 10 камень, 5 дерево, руда, 0.6 стекло, 0.5 земля, песок, 0.2 листва, 0.0 трава, саженцы 
        /// </summary>
        public float Resistance { get; private set; }
        /// <summary>
        /// Скользкость
        /// f = slipperiness * 0.91
        /// t = теряется за тик	/ тиков до остановки
        /// 0.6 обычная     f=0.546 t=45% / 2.2
        /// 0.8 слизь       f=0.728 t=27% / 3.7
        /// 0.98 лёд        f=0.892 t=11% / 9.2
        /// 0.989 синий лёд f=0.900 t=10% / 10
        /// </summary>
        public float Slipperiness { get; private set; } = Cp.DefaultSlipperiness;
        /// <summary>
        /// Горючесть материала
        /// </summary>
        public bool Combustibility { get; private set; }
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

        public BlockBase(MaterialBase material) => Material = material;

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
            if ((LightOpacityValue >> 4) > 13) IsNotTransparent = true;

            _InitOutline();
        }

        /// <summary>
        /// Инициализация блока Воздух
        /// </summary>
        public void InitAir()
        {
            Alias = "Air";
            _InitBlockRender();
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

        /// <summary>
        /// Пройтись по массиву AABB fullCube и поделить все значения на 16
        /// </summary>
        protected void _InitFullCubeDivide16()
        {
            for(int i = 0; i < _fullCube.Length; i++)
            {
                for (int j = 0; j < _fullCube[i].Length; j++)
                {
                    _fullCube[i][j] = _fullCube[i][j].Divide16();
                }
            }
        }

        #endregion

        #region Методы для импорта данных с json

        /// <summary>
        /// Прочесть состояние блока из Json формы
        /// </summary>
        private void _ReadStateFromJson(JsonCompound state)
        {
            try
            {
                int lightOpacity = 15;
                int lightValue = 0;
                // Статы
                foreach (JsonKeyValue json in state.Items)
                {
                    if (json.IsKey(Ctb.NeedsRandomTick)) NeedsRandomTick = json.GetBool();
                    if (json.IsKey(Ctb.RequiredRandomTick)) RequiredRandomTick = json.GetBool();
                    if (json.IsKey(Ctb.LightOpacity)) lightOpacity = json.GetInt();
                    if (json.IsKey(Ctb.LightValue)) lightValue = json.GetInt();
                    if (json.IsKey(Ctb.Translucent)) Translucent = json.GetBool();
                    if (json.IsKey(Ctb.IgnoreOwn)) IgnoreOwn = json.GetBool();
                    if (json.IsKey(Ctb.UseNeighborBrightness)) UseNeighborBrightness = json.GetBool();
                    if (json.IsKey(Ctb.АmbientOcclusion)) АmbientOcclusion = json.GetBool();
                    if (json.IsKey(Ctb.BiomeColor)) BiomeColor = json.GetBool();
                    if (json.IsKey(Ctb.Shadow)) Shadow = json.GetBool();
                    if (json.IsKey(Ctb.IsDestorySecond)) IsDestorySecond = json.GetBool();
                    if (json.IsKey(Ctb.NoCollision)) IsCollidable = !json.GetBool();
                    if (json.IsKey(Ctb.FactorSlow))
                    {
                        float[] ar = json.GetArray().ToArrayFloat();
                        IsSlow = true;
                        FactorSlowXZ = ar[0];
                        FactorSlowY = ar[1];
                    }
                    if (json.IsKey(Ctb.LiquidImpulse)) LiquidImpulse = json.GetFloat();
                    if (json.IsKey(Ctb.Passable)) Passable = json.GetBool();
                    if (json.IsKey(Ctb.Resistance)) Resistance = json.GetFloat();
                    if (json.IsKey(Ctb.Slipperiness)) Slipperiness = json.GetFloat();
                    if (json.IsKey(Ctb.Combustibility)) Combustibility = json.GetBool();
                    if (json.IsKey(Ctb.IgniteOddsSunbathing)) IgniteOddsSunbathing = (byte)json.GetInt();
                    if (json.IsKey(Ctb.BurnOdds)) BurnOdds = (byte)json.GetInt();
                    if (json.IsKey(Ctb.Hardness)) Hardness = json.GetInt();
                    if (json.IsKey(Ctb.IsReplaceable)) IsReplaceable = json.GetBool();
                    if (json.IsKey(Ctb.ParticleCount)) ParticleCount = (byte)json.GetInt();
                    if (json.IsKey(Ctb.Color))
                    {
                        float[] ar = json.GetArray().ToArrayFloat();
                        Color = new Vector3(ar[0], ar[1], ar[2]);
                    }
                    if (json.IsKey(Ctb.ParticleColors))
                    {
                        string[] ar = json.GetArray().ToArrayString();
                        IsParticle = true;
                        ParticleColors = new Vector3[ar.Length];
                        string s;
                        for (int i = 0; i < ar.Length; i++)
                        {
                            s = ar[i];
                            if (s.Length != 6)
                            {
                                throw new Exception(Sr.GetString(Sr.ErrorReadJsonBlockStat, Alias));
                            }
                            ParticleColors[i] = new Vector3(
                                Convert.ToInt32(s.Substring(0, 2), 16) / 255f,
                                Convert.ToInt32(s.Substring(2, 2), 16) / 255f,
                                Convert.ToInt32(s.Substring(4, 2), 16) / 255f);
                        }
                    }
                }

                LightOpacityValue = (byte)(((lightOpacity & 0xF) << 4) | (lightValue & 0xF));
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

            if (state.IsKey(Ctb.ForceDrawFace) && !ForceDrawFace)
            {
                ForceDrawFace = true;
                for (int j = 0; j < _cullFaces.Length; j++)
                {
                    _cullFaces[j] = new bool[6];
                    for (int i = 0; i < _maskCullFaces[j].Length; i++)
                    {
                        _maskCullFaces[j][i] = new ulong[4];
                    }
                }
            }

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

        #endregion

        #region Методы для физики

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
            if (FullBlock) return true;
            // Если блок не полный, обрабатываем хитбокс блока
            Vector3 a = new Vector3(px, py, pz);
            AxisAlignedBB[] aabbs = _GetCollisionBoxesToList(pos, met);
            Vector3 pos2 = a + dir * maxDist;
            foreach (AxisAlignedBB aabb in aabbs)
            {
                if (aabb.CalculateIntercept(a, pos2).Intersection) return true;
            }
            return false;
        }

        /// <summary>
        /// Генерации сетки Hitbox линиями
        /// </summary>
        protected virtual void _InitOutline()
        {
            _hitbox = new Vector3[_fullCube.Length][];
            for (int i = 0; i < _fullCube.Length; i++)
            {
                if (_fullCube[i].Length > 0)
                {
                    _hitbox[i] = AabbOutline.Build(_fullCube[i]).ToArray();
                }
                else
                {
                    _hitbox[i] = new Vector3[0];
                }
            }
        }

        /// <summary>
        /// Локальные рамки коллизии для метаданных
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual Vector3[] _GetOutlineLocal(int met) => _hitbox[0];

        /// <summary>
        /// Передать фигуру в виде списка ограничительных рамок для курсора или проекции
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual Vector3[] GetOutline(BlockPos pos, int met, float dis = 0)
        {
            Vector3[] vectors = _GetOutlineLocal(met);
            Vector3[] vectorsResult = new Vector3[vectors.Length];
            if (dis == 0)
            {
                for (int i = 0; i < vectors.Length; i++)
                {
                    vectorsResult[i] = new Vector3(vectors[i].X + pos.X, 
                        vectors[i].Y + pos.Y, vectors[i].Z + pos.Z);
                }
            }
            else
            {
                // Вытащить AABB чтоб сменить центр. А то если блок смещён, коряво
                Vector3 center = GetCenterBoxes(met);
                for (int i = 0; i < vectors.Length; i++)
                {
                    vectorsResult[i] = new Vector3(vectors[i]);

                    vectorsResult[i].X += (vectorsResult[i].X < center.X ? -dis : dis) + pos.X;
                    vectorsResult[i].Y += (vectorsResult[i].Y < center.Y ? -dis : dis) + pos.Y;
                    vectorsResult[i].Z += (vectorsResult[i].Z < center.Z ? -dis : dis) + pos.Z;
                }
            }
            return vectorsResult;
        }

        /// <summary>
        /// Локальные рамки коллизии для метаданных
        /// </summary>
        public virtual AxisAlignedBB[] _GetCollisionBoxesLocal(int met) => _fullCube[0];

        /// <summary>
        /// Передать список ограничительных рамок блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AxisAlignedBB[] _GetCollisionBoxesToList(BlockPos pos, int met)
        {
            AxisAlignedBB[] boxes = _GetCollisionBoxesLocal(met);
            AxisAlignedBB[] boxesResult = new AxisAlignedBB[boxes.Length];
            for (int i = 0; i < boxes.Length; i++)
            {
                boxesResult[i] = boxes[i].Offset(pos.X, pos.Y, pos.Z);
            }
            return boxesResult;
        }

        /// <summary>
        /// Добавить список ограничительных рамок блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void AddCollisionBoxesToList(BlockPos pos, int met, ListFast<AxisAlignedBB> list)
        {
            AxisAlignedBB[] boxes = _GetCollisionBoxesLocal(met);
            for (int i = 0; i < boxes.Length; i++)
            {
                list.Add(boxes[i].Offset(pos.X, pos.Y, pos.Z));
            }
        }

        /// <summary>
        /// Получить вектор центра рамки локально координаты (0.0 - 1.0 => 0.5)
        /// </summary>
        public Vector3 GetCenterBoxes(int met)
        {
            AxisAlignedBB[] boxes = _GetCollisionBoxesLocal(met);

            if (boxes.Length > 0)
            {
                AxisAlignedBB aabb = boxes[0];
                for (int i = 1; i < boxes.Length; i++)
                {
                    aabb = aabb.AddCoord(boxes[i].Min).AddCoord(boxes[i].Max);
                }
                return aabb.GetCenter();
            }
            return new Vector3(.5f, .5f, .5f);
        }

        /// <summary>
        /// Получить одну большую рамку блока, если их несколько они объеденяться, 
        /// используется для установки блока
        /// </summary>
        public AxisAlignedBB GetCollisionOne(BlockPos pos, int met)
        {
            AxisAlignedBB[] axes = _GetCollisionBoxesToList(pos, met);
            if (axes.Length == 1)
            {
                return axes[0];
            }
            if (axes.Length > 1)
            {
                AxisAlignedBB aabb = axes[0];
                for (int i = 1; i < axes.Length; i++)
                {
                    aabb = aabb.AddCoord(axes[i].Min).AddCoord(axes[i].Max);
                }
                return aabb;
            }
            Vector3 min = pos.ToVector3();
            return new AxisAlignedBB(min, min + 1f);
        }

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
            LightOpacityValue = 0;
            IsNotTransparent = false;
            Translucent = true;
            IsCollidable = false;
            FullBlock = false;
            Passable = true;

            _quads = new QuadSide[][] { new QuadSide[] { new QuadSide(0) } };
            _hitbox = new Vector3[][] { new Vector3[] { new Vector3(0) } };
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
        public virtual bool IsCullFace(int met, int indexSide) => _cullFaces[met & 0x3F][indexSide];
        /// <summary>
        /// Надо ли принудительно рисовать сторону, конкретного варианта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool IsForceDrawFace(int met, int indexSide) => _forceDrawFaces[met & 0x3F][indexSide];
        /// <summary>
        /// Надо ли принудительно рисовать не крайнюю сторону, конкретного варианта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool IsForceDrawNotExtremeFace(int met, int indexSide) => _forceDrawNotExtremeFaces[met & 0x3F][indexSide];

        /// <summary>
        /// Проверка масок сторон
        /// </summary>
        /// <param name="indexSide">Индекс сторонв</param>
        /// <param name="met">Мет данные проверяющего блока</param>
        /// <param name="blockSide">Объект соседнего блока</param>
        /// <param name="metSide">Мет данные соседнего блока</param>
        public virtual bool ChekMaskCullFace(int indexSide, int met, BlockBase blockSide, int metSide)
        {
            ulong[] mask = _maskCullFaces[met & 0x3F][indexSide];
            ulong[] maskCheck = blockSide._maskCullFaces[metSide & 0x3F][PoleConvert.Reverse[indexSide]];
            return (maskCheck[0] & mask[0]) == mask[0]
                && (maskCheck[1] & mask[1]) == mask[1]
                && (maskCheck[2] & mask[2]) == mask[2]
                && (maskCheck[3] & mask[3]) == mask[3];
        }

        #endregion

        #region Методы для тиков

        /// <summary>
        /// Случайный эффект частички и/или звука на блоке только для клиента
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void RandomDisplayTick(WorldClient world, BlockPos blockPos, BlockState blockState) 
        {
            // TODO::2026-04-26 Временно!
            //if (!IsAir && world.Rnd.Next(10) == 0)
            //{
            //    if (world.GetBlockState(blockPos.OffsetUp()).Id == 0)
            //    {
            //        Vector3 pos = blockPos.ToVector3Center();
            //        pos.Y += .75f;
            //        world.SpawnParticle(EntitiesFXReg.DropletId, 1, pos, new Vector3(.5f), 1, 0);
            //    }
            //}
        }

        /// <summary>
        /// Обязательный случайный эффект частички и/или звука на блоке только для клиента
        /// </summary>
        public virtual void RequiredRandomDisplayTick(WorldClient world, BlockPos blockPos, BlockState blockState) { }

        /// <summary>
        /// Случайный эффект блока, для сервера
        /// </summary>
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
            BlockPos blockPos, BlockState blockState)
        {
            if (LiquidMet.IsAddLiquid(blockState.Met))
            {
                // Если имеется жидкость, надо тикать жидкость
                world.SetBlockAddLiquidTick(blockPos, 4);
            }
        }

        /// <summary> 
        /// Проверка установка блока, можно ли его установить тут
        /// </summary>
        public virtual bool CanBlockStay(WorldBase world, ChunkBase chunk, 
            BlockPos blockPos, int met = 0) => true;

        /// <summary>
        /// Действие перед размещеннием блока, для определения метданных
        /// </summary>
        public virtual BlockState OnBlockPlaced(WorldBase world, BlockPos blockPos, 
            BlockState blockState, Pole side, Vector3 facing, float playerYaw) => blockState;

        /// <summary>
        /// Действие блока после его установки, не смена met!!! Только на сервере
        /// Удалять тут другие блоки НЕЛЬЗЯ
        /// </summary>
        public virtual void OnBlockAdded(WorldServer world, ChunkServer chunk, 
            BlockPos blockPos, BlockState state) { }

        /// <summary>
        /// Действие блока после его смены met!!! Только на сервере
        /// Удалять тут другие блоки НЕЛЬЗЯ
        /// </summary>
        public virtual void OnMetSelected(WorldServer world, ChunkServer chunk,
            BlockPos blockPos, BlockState state) { }

        /// <summary>
        /// Действие блока после его удаления, только на сервере
        /// </summary>
        /// <param name="entity">Сущность которая сломала этот блок</param>
        public virtual void OnBreakBlock(WorldServer world, ChunkServer chunk, 
            BlockPos blockPos, BlockState stateOld, EntityLiving entity) { }

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
        public virtual void DropBlockAsItem(WorldServer worldServer, PlayerServer player, 
            BlockPos blockPos, BlockState stateOld) { }

        /// <summary>
        /// Активация блока, true - был клик, false - нет такой возможности
        /// </summary>
        /// <param name="pos">Позиция блока, по которому щелкают</param>
        /// <param name="side">Сторона, по которой щелкнули</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool OnBlockActivated(PlayerBase player, BlockPos blockPos, Pole side, Vector3 facing) => false;

        /// <summary>
        /// Сместить положение, как правило на ключевое значение отвечающее за рендер.
        /// Пример дверь, кровать
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual BlockPos ShiftPosition(BlockPos blockPos, BlockState state) => blockPos;

        #endregion

        /// <summary>
        /// Проиграть звук поломки
        /// </summary>
        public void PlaySoundBreak(WorldBase world, BlockPos blockPos)
        {
            if (Material.IsSampleBreak())
            {
                world.PlaySoundDistance(Material.SampleBreak(world.Rnd),
                    blockPos.ToVector3Center(), 1f, 1f);
            }
        }

        /// <summary>
        /// Получить строку мет данных, для отладки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToInfo(int met)
        {
            if (CanAddLiquid() && LiquidMet.IsAddLiquid(met))
            {
                return ToString() + " M:" 
                    + (met & 0xFFF) + ChatStyle.Blue
                    + " Liquid " + Ce.Blocks.GetAddLiquid(met) + " M:" + LiquidMet.GetAddLiquidMet(met)
                    + ChatStyle.Reset;
            }
            return ToString() + " M:" + met.ToString();
        }

        public int CountShape() => _quads == null ? 0 : _quads.Length;

        public override string ToString() => IndexBlock.ToString() + " " + Alias;
    }
}
