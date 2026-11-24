using Mvk2.Entity.Particle;
using System.Runtime.CompilerServices;
using Vge.Json;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;
using WinGL.Util;
using System.Collections.Generic;

namespace Mvk2.World.Block.List
{
    /***
     * Met
     * 
     * 0011 1101 1111
     * 
     * 0 - 4 bit = Pole
     *  North = pole & 1 != 0
     *  West = pole & 2 != 0
     *  South = pole & 4 != 0
     *  East = pole & 8 != 0
     *  Up = pole & 16 != 0
     *  
     * 6 - 9 bit = Age
     */

    /// <summary>
    /// Блок огня
    /// </summary>
    public class BlockFire : BlockBase
    {
        public BlockFire(MaterialBase material) : base(material)
        {
            AxisAlignedBB[][] axes = new AxisAlignedBB[][]
            {
                new AxisAlignedBB[] { new AxisAlignedBB(0, 0, 0, 16, 4, 16) },
                new AxisAlignedBB[] { new AxisAlignedBB(0, 0, 0, 2, 16, 16) },
                new AxisAlignedBB[] { new AxisAlignedBB(0, 0, 14, 16, 16, 16) },
                new AxisAlignedBB[] { new AxisAlignedBB(14, 0, 0, 16, 16, 16) },
                new AxisAlignedBB[] { new AxisAlignedBB(0, 0, 0, 16, 16, 2) },
                new AxisAlignedBB[] { new AxisAlignedBB(0, 14, 0, 16, 16, 16) }
            };

            _fullCube = new AxisAlignedBB[32][];
            List<AxisAlignedBB> axesCache = new List<AxisAlignedBB>();
            _fullCube[0] = axes[0];
            for (int i = 1; i < 32; i++)
            {
                axesCache.Clear();
                if ((i & 1) != 0) axesCache.AddRange(axes[2]); // North
                if ((i & 2) != 0) axesCache.AddRange(axes[3]); // West
                if ((i & 4) != 0) axesCache.AddRange(axes[4]); // South
                if ((i & 8) != 0) axesCache.AddRange(axes[1]); // East
                if ((i & 16) != 0) axesCache.AddRange(axes[5]); // Up
                _fullCube[i] = axesCache.ToArray();
            }

            _InitFullCubeDivide16();
        }

        /// <summary>
        /// Массив сторон прямоугольных форм для рендера
        /// </summary>
        public override QuadSide[] GetQuads(int met, int xb, int zb) => _quads[met & 0x1F];

        /// <summary>
        /// Локальные рамки коллизии для метаданных
        /// </summary>
        public override AxisAlignedBB[] _GetCollisionBoxesLocal(int met) => _fullCube[met & 0x1F];
        /// <summary>
        /// Локальные рамки коллизии для метаданных
        /// </summary>
        public override Vector3[] _GetOutlineLocal(int met) => _hitbox[met & 0x1F];

        /// <summary>
        /// Получить модель
        /// </summary>
        protected override void _ShapeDefinition(JsonCompound state, JsonCompound shapes)
        {
            base._ShapeDefinition(state, shapes);

            _cullFaces = new bool[32][];
            QuadSide[][] quads = new QuadSide[32][];
            _maskCullFaces = new ulong[32][][];
            _forceDrawFaces = new bool[32][];
            ForceDrawFace = true;

            for (int j = 0; j < 32; j++)
            {
                _cullFaces[j] = new bool[6];
                _maskCullFaces[j] = new ulong[6][];
                for (int i = 0; i < 6; i++)
                {
                    _maskCullFaces[j][i] = new ulong[4];
                }
                _forceDrawFaces[j] = new bool[6];
            }

            List<QuadSide> quadsCache = new List<QuadSide>();
            quads[0] = _quads[0];
            for (int i = 1; i < 32; i++)
            {
                quadsCache.Clear();
                if ((i & 1) != 0) quadsCache.AddRange(_quads[2]); // North
                if ((i & 2) != 0) quadsCache.AddRange(_quads[3]); // West
                if ((i & 4) != 0) quadsCache.AddRange(_quads[4]); // South
                if ((i & 8) != 0) quadsCache.AddRange(_quads[1]); // East
                if ((i & 16) != 0) quadsCache.AddRange(_quads[5]); // Up
                quads[i] = quadsCache.ToArray();
            }
            _quads = quads;
        }

        /// <summary>
        /// Действие блока после его установки, не смена met!!! Только на сервере
        /// Удалять тут другие блоки НЕЛЬЗЯ
        /// </summary>
        public override void OnBlockAdded(WorldServer world, ChunkServer chunk, 
            BlockPos blockPos, BlockState state)
        {
            // запустить мгновенный тик
            //world.SetBlockTick(blockPos, (uint)(world.Rnd.Next(10) + 5));
            world.SetBlockTick(blockPos, (uint)(world.Rnd.Next(10) + 50));
        }

        /// <summary>
        /// Проверка установи блока, можно ли его установить тут
        /// </summary>
        public override bool CanBlockStay(WorldBase world, ChunkBase chunk,
            BlockPos blockPos, int met = 0)
        {
            BlockPos blockPosDown = blockPos.OffsetDown();
            return _DoesBlockHaveSolidTopSurface(world, blockPosDown)
                || _BlockThatCanBurn(world, blockPosDown)
                || _SideBlockCombustibility(world, blockPos) != 0;
        }

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldServer world, ChunkServer chunk, 
            BlockPos blockPos, BlockState blockState)
        {
            base.NeighborBlockChange(world, chunk, blockPos, blockState);
            if (CanBlockStay(world, chunk, blockPos))
            {
                int newMet = _MetUpdate(world, blockPos, blockState.Met);
                if (newMet != blockState.Met) world.SetBlockStateMet(blockPos, newMet);
            }
            else
            {
                world.SetBlockToAir(blockPos);
            }
        }

        /// <summary>
        /// Действие перед размещеннием блока, для определения метданных
        /// </summary>
        public override BlockState OnBlockPlaced(WorldBase world, BlockPos blockPos, BlockState blockState, 
            Pole side, Vector3 facing, float playerYaw)
        {
            return blockState.NewMet(_MetUpdate(world, blockPos, 0));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int _MetUpdate(WorldBase world, BlockPos blockPos, int met)
        {
            BlockPos blockPosDown = blockPos.OffsetDown();

            met &= ~0x3F; // обнулили 6 бит для графики 
            if (_BlockThatCanBurn(world, blockPosDown)) return met;
            return met | _SideBlockCombustibility(world, blockPos);
        }

        /// <summary>
        /// Блок имеет твердую непрозрачную поверхность
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool _DoesBlockHaveSolidTopSurface(WorldBase world, BlockPos blockPos)
        {
            BlockState blockState = world.GetBlockState(blockPos);
            BlockBase block = blockState.GetBlock();
            return block.IsNotTransparent && block.FullBlock;
        }

        /// <summary>
        /// Блок который может гореть
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _BlockThatCanBurn(WorldBase world, BlockPos blockPos)
        {
            BlockState blockState = world.GetBlockState(blockPos);
            BlockBase block = blockState.GetBlock();
            return block.Combustibility && block.IsNotTransparent && block.FullBlock;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int _SideBlockCombustibility(WorldBase world, BlockPos blockPos)
        {
            int pole = 0;
            if (_BlockThatCanBurn(world, blockPos.OffsetSouth())) pole |= 1;
            if (_BlockThatCanBurn(world, blockPos.OffsetEast())) pole |= 2;
            if (_BlockThatCanBurn(world, blockPos.OffsetNorth())) pole |= 4;
            if (_BlockThatCanBurn(world, blockPos.OffsetWest())) pole |= 8;
            if (_BlockThatCanBurn(world, blockPos.OffsetUp())) pole |= 16;
            return pole;
        }


        /// <summary>
        /// Случайный эффект частички и/или звука на блоке только для клиента
        /// </summary>
        public override void RandomDisplayTick(WorldClient world, BlockPos blockPos, BlockState blockState)
        {

            //if (random.Next(24) == 0)
            //{
            //    world.PlaySound(AssetsSample.Fire, blockPos.ToVec3() + .5f, 1f + random.NextFloat(), random.NextFloat() * .7f + .3f);
            //}

            if (world.Rnd.Next(3) == 0)
            {
                if (blockState.Met == 0) // На земле
                {
                    world.SpawnParticle(EntitiesFXRegMvk.FlameSmokeId, 16,
                        blockPos.ToVector3Center(), new Vector3(1, 0, 1), 1, 0);
                }
                else if ((blockState.Met & 16) != 0) // Сверху
                {
                    world.SpawnParticle(EntitiesFXRegMvk.FlameSmokeId, 8,
                        blockPos.ToVector3Center(), new Vector3(2, 0, 2), 1, 0);
                }
                else
                {
                    world.SpawnParticle(EntitiesFXRegMvk.FlameSmokeId, 8,
                        blockPos.ToVector3Center(), new Vector3(1, 0, 1), 1, 0);
                }
            }
        }

        /// <summary>
        /// Случайный эффект частички и/или звука на блоке только для клиента
        /// </summary>
        public override void RequiredRandomDisplayTick(WorldClient world, BlockPos blockPos, BlockState blockState)
        {
            if (world.Rnd.Next(3) == 0)
            {
                if (blockState.Met == 0) // На земле
                {
                    world.SpawnParticle(EntitiesFXRegMvk.FlameSmokeId, 4,
                        blockPos.ToVector3Center(), new Vector3(1, 0, 1), 1, 0);
                }
                else if ((blockState.Met & 16) != 0) // Сверху
                {
                    world.SpawnParticle(EntitiesFXRegMvk.FlameSmokeId, 2,
                        blockPos.ToVector3Center(), new Vector3(2, 0, 2), 1, 0);
                }
                else
                {
                    world.SpawnParticle(EntitiesFXRegMvk.FlameSmokeId, 2,
                        blockPos.ToVector3Center(), new Vector3(1, 0, 1), 1, 0);
                }
            }
        }

        #region UpdateServer

        /// <summary>
        /// Обновить блок в такте
        /// </summary>
        public override void UpdateTick(WorldServer world, ChunkServer chunk, BlockPos blockPos, BlockState blockState, Rand random)
        {
            world.SetBlockToAir(blockPos, BlockFlag.cNotify);
        }

        #endregion
    }
}
