using System.Runtime.CompilerServices;
using Vge.Json;
using Vge.Util;
using WinGL.Util;

namespace Vge.World.Block.List
{
    /// <summary>
    /// Объект жидких блоков.
    /// Всё жидкости должны быть в отдельной маске для смещения защиты от Z-Fighting
    /// </summary>
    public class BlockLiquid : BlockBase
    {
        /// <summary>
        /// Стороны для прорисовки жидкого блока
        /// </summary>
        protected SideLiquid[] _sideLiquids;

        public BlockLiquid(bool alphaSort)
        {
            Alpha = true;
            AlphaSort = alphaSort;
            Liquid = true;
            FullBlock = false;
            CullFaceAll = true;
            LiquidOutside = 1024;
            NotLiquidOutside = new int[] { 0, 0, 0, 0, 0, 0 };
        }

        /// <summary>
        /// Инициализация объекта рендера для блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void _InitBlockRender()
            => BlockRender = Gi.BlockLiquidAlphaRendFull;

        /// <summary>
        /// Имеется ли отбраковка конкретной стороны, конкретного варианта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsCullFace(int met, int indexSide) => false;
        /// <summary>
        /// Надо ли принудительно рисовать сторону, конкретного варианта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsForceDrawFace(int met, int indexSide) => false;
        /// <summary>
        /// Надо ли принудительно рисовать не крайнюю сторону, конкретного варианта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool IsForceDrawNotExtremeFace(int met, int indexSide) => false;
        /// <summary>
        /// Проверка масок сторон
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool ChekMaskCullFace(int indexSide, int met, BlockBase blockSide, int metSide) => false;

        /// <summary>
        /// Шаг волны для дистанции растекания, 15 / step = максимальная длинна
        /// 1 = 14; 2 = 7; вода 3 = 4; нефть 4 = 3; лава
        /// </summary>
        protected int _stepWave = 2;
        /// <summary>
        /// Сколько игровых тиков между тиками
        /// 5 вода, 15 нефть, 30 лава
        /// </summary>
        protected uint _tickRate = 8;
        /// <summary>
        /// Лужа
        /// </summary>
        protected bool _puddle = true;

        /// <summary>
        /// Получить модель
        /// </summary>
        protected override void _ShapeDefinition(JsonCompound state, JsonCompound shapes)
        {
            BlockShapeDefinition shapeDefinition = new BlockShapeDefinition(Alias);
            _sideLiquids = shapeDefinition.RunShapeLiquidFromJson(state, shapes);
            _maskCullFaces = shapeDefinition.MaskCullFaces;
        }

        /// <summary>
        /// Получить сторону для прорисовки жидкого блока
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override SideLiquid GetSideLiquid(int index) => _sideLiquids[index];

        /// <summary>
        /// Получить угол течения
        /// </summary>
        public static float GetAngleFlow(int l11, int l01, int l10, int l12, int l21)
        {
            Vector3 vec = new Vector3(0);
            if (l11 > 0)
            {
                // 14 это ограничение стыка между разными типами жидкости, для блокировки волны
                if (l11 == 14) l11 = 15;
                if (l01 > 0)
                {
                    if (l01 == 14) vec.X -= l11 - 15;
                    else vec.X -= l11 - l01;
                }
                if (l10 > 0)
                {
                    if (l10 == 14) vec.Z -= l11 - 15;
                    else vec.Z -= l11 - l10;
                }
                if (l21 > 0)
                {
                    if (l21 == 14) vec.X += l11 - 15;
                    else vec.X += l11 - l21;
                }
                if (l12 > 0)
                {
                    if (l12 == 14) vec.Z += l11 - 15;
                    else vec.Z += l11 - l12;
                }
                vec.Y -= 6f;
                vec = vec.Normalize();
            }

            return (vec.X == 0f && vec.Z == 0f) ? -1000f : Glm.Atan2(vec.Z, vec.X) - Glm.Pi90;
        }

        /// <summary>
        /// Действие перед размещеннием блока, для определения метданных
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override BlockState OnBlockPlaced(WorldBase worldIn, BlockPos blockPos, BlockState state, Pole side, Vector3 facing)
            => state.NewMet(7);

        #region Методы для тиков

        /// <summary>
        /// Случайный эффект частички и/или звука на блоке только для клиента
        /// </summary>
        public override void RandomDisplayTick(WorldBase world, BlockPos blockPos, BlockState blockState, Rand random) { }

        /// <summary>
        /// Обновить блок в такте
        /// </summary>
        public override void UpdateTick(WorldServer world, BlockPos blockPos, BlockState blockState, Rand random)
        {
            int met0 = blockState.Met;
            bool isAddLiquid = met0 > 4096;
            if (isAddLiquid) met0 = (met0 >> 12) & 0xF;
            int met = met0 == 0 ? 15 : met0;
            if (met == 15)
            {
                // Стоячая, был тик, пробуем растечься
                met -= _stepWave;
                int side = _GetVectors(world, blockPos, 15);
                if ((side & 1) != 0) _CheckSetSide(world, blockPos.OffsetDown(), met);
                if ((side & 1 << 1) != 0) _CheckSetSide(world, blockPos.OffsetEast(), met);
                if ((side & 1 << 2) != 0) _CheckSetSide(world, blockPos.OffsetWest(), met);
                if ((side & 1 << 3) != 0) _CheckSetSide(world, blockPos.OffsetNorth(), met);
                if ((side & 1 << 4) != 0) _CheckSetSide(world, blockPos.OffsetSouth(), met);
            }
            else
            {
                if (!_CheckEndlessSource(world, blockPos))
                {
                    int levelLiquidUp = _GetLevelLiquid(world, blockPos.OffsetUp());
                    int levelLiquid = levelLiquidUp;
                    if (levelLiquid < 15)
                    {
                        // проверим и горизонтальные
                        for (int i = 2; i < 6; i++)
                        {
                            int levelLiquidCache = _GetLevelLiquid(world, blockPos.Offset(i));
                            if (levelLiquidCache > levelLiquid)
                            {
                                levelLiquid = levelLiquidCache;
                                if (levelLiquid == 15) break;
                            }
                        }
                    }

                    if (met >= levelLiquid && met < 15 && levelLiquidUp <= 0)
                    {
                        // уменьшаем, так-как рядом жидкость мельче
                        met -= _stepWave * 2;
                        if (met <= 0)
                        {
                            if (_puddle)
                            {
                                // TODO::2025-12-21 Продумать как убрать с боку воду с 1, возможно в след тике
                                BlockPos posDown = blockPos.OffsetDown();
                                if (_GetLevelLiquid(world, posDown) > 0)
                                {
                                    // высохла
                                    world.SetLiquidBlockToAir(blockPos);
                                }
                                else
                                {
                                    // Надо либо стечь, либо лужа
                                    if (_GetLevelLiquid(world, posDown.OffsetEast()) != -1
                                        || _GetLevelLiquid(world, posDown.OffsetNorth()) != -1
                                        || _GetLevelLiquid(world, posDown.OffsetSouth()) != -1
                                        || _GetLevelLiquid(world, posDown.OffsetWest()) != -1
                                        || _GetLevelLiquid(world, posDown.OffsetSouthEast()) != -1
                                        || _GetLevelLiquid(world, posDown.OffsetSouthWest()) != -1
                                        || _GetLevelLiquid(world, posDown.OffsetNorthEast()) != -1
                                        || _GetLevelLiquid(world, posDown.OffsetNorthWest()) != -1)
                                    {
                                        // высохла
                                        world.SetLiquidBlockToAir(blockPos);
                                    }
                                    else if (met0 != 1)
                                    {
                                        _SetBlock(world, blockPos, 1, isAddLiquid);
                                    }
                                }
                            }
                            else
                            {
                                // высохла
                                world.SetLiquidBlockToAir(blockPos);
                            }
                        }
                        else
                        {
                            // мельче
                            if (met0 != met)
                            {
                                _SetBlock(world, blockPos, met, isAddLiquid);
                            }
                        }
                    }
                    else if (met < levelLiquid - _stepWave)
                    {
                        // добавляем уровень жидкости
                        met += _stepWave;
                        if (met <= 15)
                        {
                            if (met == 15) met = 0;
                            if (met0 != met)
                            {
                                _SetBlock(world, blockPos, met, isAddLiquid);
                            }
                        }
                    }

                    // надо продолжить растекаться
                    BlockPos blockPosDown = blockPos.OffsetDown();
                    if (_CheckCan(world, blockPosDown))
                    {
                        // течение вниз
                        BlockState blockStateDown = world.GetBlockState(blockPosDown);

                        // Микс с низу
                        if (!_MixingDown(world, blockPos, blockState, blockPosDown, blockStateDown))
                        {
                            // TODO::2025-12-22 Доп растекание с высоты, раньше всегда 15
                            if (met < 7) met = 7;
                            //if (met < 3) met = 3;
                            _CheckSetSide(world, blockPosDown, met);
                        }
                    }
                    else
                    {
                        met -= _stepWave;
                        if (met >= 0)
                        {
                            // течение по сторонам
                            int side = _GetVectors(world, blockPos, met);
                            //if ((side & 1) != 0) _CheckSetSide(world, blockPos.OffsetDown(), met);
                            if ((side & 1 << 1) != 0) _CheckSetSide(world, blockPos.OffsetEast(), met);
                            if ((side & 1 << 2) != 0) _CheckSetSide(world, blockPos.OffsetWest(), met);
                            if ((side & 1 << 3) != 0) _CheckSetSide(world, blockPos.OffsetNorth(), met);
                            if ((side & 1 << 4) != 0) _CheckSetSide(world, blockPos.OffsetSouth(), met);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Проверка плюс растекание жидкости с уровнем met в pos
        /// </summary>
        private void _CheckSetSide(WorldServer world, BlockPos pos, int met)
        {
            int levelLiquid = _GetLevelLiquid(world, pos);
            if (levelLiquid != -1 && levelLiquid < met)
            {
                _InteractionWithBlocks(world, pos);
                _SetBlock(world, pos, met, world.GetBlockState(pos).GetBlock().CanAddLiquid());
            }
        }

        /// <summary>
        /// Смена блока, с запуском его тика
        /// </summary>
        private void _SetBlock(WorldServer world, BlockPos pos, int met, bool isAddLiquid)
        {
            world.SetBlockState(pos, new BlockState() { Id = IndexBlock, Met = met }, 46);
            if (isAddLiquid) world.SetBlockAddLiquidTick(pos, _tickRate);
            else world.SetBlockTick(pos, _tickRate);
        }

        /// <summary>
        /// Взаимодействие с блоками
        /// </summary>
        private void _InteractionWithBlocks(WorldServer world, BlockPos blockPos) 
        {

        }

        /// <summary>
        /// Получиь уровень жидкости с этой позиции где, 15 макс, 0 можно разлить воды нет, -1 нельзя разлить воды нет.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int _GetLevelLiquid(WorldServer world, BlockPos pos)
        {
            BlockState blockState = world.GetBlockState(pos);
            int met = blockState.Met;
            if (blockState.Id == IndexBlock)
            {
                // Блок такой же жидкости
                return met == 0 ? 15 : met;
            }
            
            BlockBase block = blockState.GetBlock();
            if (Ce.Blocks.GetAddLiquidIndex(met) == IndexBlock)
            {
                // Блок с доп жидкостью
                met = block.GetAddLiquidMet(met);
                return met == 0 ? 14 : met;
            }
            return _CheckCan(blockState) ? 0 : -1;
            // EnumBlock enumBlock = blockState.GetBlock().EBlock;
            //return enumBlock == eBlock ? 15 : enumBlock == eBlockFlowing ? blockState.met : CheckCan(blockState) ? 0 : -1;
        }

        /// <summary>
        /// Можно ли разлить на эту позицию жидкость
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _CheckCan(BlockState blockState)
        {
            BlockBase block = blockState.GetBlock();
            //EnumMaterial eMaterial = block.Material.EMaterial;
            if (block.IsAir || blockState.Id == IndexBlock) return true;

            if (block.CanAddLiquid())
            {
                bool b = block.IsAddLiquid(blockState.Met);
                if (!b || (b && Ce.Blocks.GetAddLiquidIndex(blockState.Met) == IndexBlock))
                    return true;
            }
            return false;
                // || block.IsLiquidDestruction() || IsFire(eMaterial)
                               //|| eMaterial == materialLiquid
                               //// Взаимодействие лавы с водой и нефтью
                               //|| (materialLiquid == EnumMaterial.Lava && (eMaterial == EnumMaterial.Oil || eMaterial == EnumMaterial.Water))
                               //// Взаимодействие воды и нефти с лавой
                               //|| (eMaterial == EnumMaterial.Lava && (materialLiquid == EnumMaterial.Oil || materialLiquid == EnumMaterial.Water));
        }

        /// <summary>
        /// Можно ли разлить на эту позицию жидкость
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _CheckCan(WorldServer world, BlockPos pos) => _CheckCan(world.GetBlockState(pos));

        /// <summary>
        /// Можно ли разлить на эту позицию жидкость, без проверки материала
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _CheckCanNotMaterial(WorldServer world, BlockPos pos)
        {
            BlockState state = world.GetBlockState(pos);
            BlockBase block = state.GetBlock();
            //EnumMaterial eMaterial = block.Material.EMaterial;
            //return block.IsAir || (state.Id == IndexBlock && state.Met > 0) || (state.Id != IndexBlock && !block.FullBlock);
            return block.IsAir || (state.Id == IndexBlock && state.Met > 0)
                || (state.Id != IndexBlock && block.CanAddLiquid());
            //block.IsLiquidDestruction() || IsFire(eMaterial)
            //  || block.EBlock == eBlockFlowing;
        }

        /// <summary>
        /// Проверка бесконечного источника
        /// </summary>
        protected virtual bool _CheckEndlessSource(WorldServer world, BlockPos blockPos) => false;


        /// <summary>
        /// Получить список направлений куда может течь жидкость, в битах
        /// 1 - Down, 2 - East, 4 - West, 8 - North, 16 - South
        /// </summary>
        protected int _GetVectors(WorldServer world, BlockPos blockPos, int level)
        {
            // течение вниз
            bool down = _CheckCan(world, blockPos.OffsetDown());

            BlockPos pos;
            // Направление в битах
            int side = down ? 1 : 0;

            int stepStart = int.MaxValue;
            for (int i = 2; i < 6; i++)
            {
                pos = blockPos.Offset(i);
                if (_CheckCanNotMaterial(world, pos))
                {
                    int step = 0;
                    if (!_CheckCan(world, pos.OffsetDown()))
                    {
                        step = _GetVectorsLong(world, pos, 1,
                            PoleConvert.Reverse[i], ((level - 1) / _stepWave) - 1);
                    }
                    if (step < stepStart)
                    {
                        side = down ? 1 : 0;
                    }

                    if (step <= stepStart)
                    {
                        side += PoleConvert.Flag[i];
                        stepStart = step;
                    }
                }
            }

            return side;
        }

        /// <summary>
        /// Находим растояние в конкретной позиции
        /// </summary>
        /// <param name="world">мир</param>
        /// <param name="blockPos">позиция проверки</param>
        /// <param name="step">шаг смещения от начала</param>
        /// <param name="pole">откуда пришёл</param>
        protected int _GetVectorsLong(WorldServer world, BlockPos blockPos, int step, int poleOpposite, int level)
        {
            int stepResult = int.MaxValue;
            BlockPos pos;
            for (int i = 2; i < 6; i++)
            {
                if (i != poleOpposite)
                {
                    pos = blockPos.Offset(i);
                    if (_CheckCanNotMaterial(world, pos))
                    {
                        if (_CheckCan(world, pos.OffsetDown())) return step;
                        if (step < level)
                        {
                            int stepCache = _GetVectorsLong(world, pos, step + 1,
                                PoleConvert.Reverse[i], level);
                            if (stepCache < stepResult)
                            {
                                stepResult = stepCache;
                            }
                        }
                    }
                }
            }

            return stepResult;
        }

        #endregion

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldServer world, BlockPos blockPos, 
            BlockState blockState, BlockBase neighborBlock)
        {
            if (!_Mixing(world, blockPos, blockState))
            {
                world.SetBlockTick(blockPos, 4);
            }
        }

        /// <summary>
        /// Смешивание блоков
        /// </summary>
        protected virtual bool _Mixing(WorldServer world, BlockPos blockPos, BlockState state)
            => false;

        /// <summary>
        /// Смешивание снизу
        /// </summary>
        protected virtual bool _MixingDown(WorldBase world, BlockPos blockPos, BlockState state,
            BlockPos blockPosDown, BlockState blockStateDown) => false;

    }
}
