using Mvk2.World.Block;
using Vge.Entity;
using Vge.Entity.Physics;
using Vge.Entity.Sizes;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using Vge.World.Block.List;
using Vge.World.Chunk;
using WinGL.Util;

namespace Mvk2.Entity
{
    /// <summary>
    /// Наличия блоков в которой находится сущность
    /// </summary>
    public class PresenceBlocksMvk : IPresenceBlocks
    {
        public readonly EntityBase Entity;

        public PresenceBlocksMvk(EntityBase entity)
            => Entity = entity;

        /// <summary>
        /// Находится ли в воде
        /// </summary>
        public bool IsInWater { get; private set; }

        /// <summary>
        /// Находится ли в любой из жидкостей
        /// </summary>
        public bool IsInLiquid { get; private set; }
        /// <summary>
        /// Можно ли совершить авто прыжок
        /// </summary>
        public bool IsInLiquidAutoJump { get; private set; }
        /// <summary>
        /// Ускорение всплытия в жидкости
        /// </summary>
        public float AccelerationAscentInLiquid { get; private set; } = .048f;
        /// <summary>
        /// Ускорение для погружения в жидкости
        /// </summary>
        public float AccelerationToDiveInLiquid { get; private set; } = -.032f;
        /// <summary>
        /// Коэффициент трения в жидкости
        /// </summary>
        public float FactorInertiaInLiquid { get; private set; } = .8f;
        /// <summary>
        /// Коэффициент гравитации в жидкости
        /// </summary>
        public float FactorGravityInLiquid { get; private set; } = .01f;

        /// <summary>
        /// Имеются ли замедления
        /// </summary>
        public bool IsSlow { get; private set; }
        /// <summary>
        /// Коэффициент замедления по X и Z
        /// </summary>
        public float FactorSlowXZ { get; private set; }
        /// <summary>
        /// Коэффициент замедления по Y
        /// </summary>
        public float FactorSlowY { get; private set; }

        /// <summary>
        /// Имеется импульс
        /// </summary>
        public bool IsImpulse { get; private set; }
        /// <summary>
        /// Импульс по координате X
        /// </summary>
        public float ImpulseX { get; private set; }
        /// <summary>
        /// Импульс по координате X
        /// </summary>
        public float ImpulseY { get; private set; }
        /// <summary>
        /// Импульс по координате X
        /// </summary>
        public float ImpulseZ { get; private set; }
        /// <summary>
        /// Счётчик для авто прыжка из воды
        /// </summary>
        private int _numberAutoJump;

        /// <summary>
        /// Обновление перерасчёта в такте
        /// </summary>
        public void Update(WorldBase world)
        {
            _EmptyBlocks();

            if (Entity.Size is ISizeEntityBox sizeBox)
            {
                // Коробка
                AxisAlignedBB aabb = sizeBox.GetBoundingBox().Expand(-.2f, -.4f, -.2f);

                Vector3i min = aabb.MinInt();
                Vector3i max = aabb.MaxInt();

                int numberBlocks = world.ChunkPr.Settings.NumberBlocks;

                int minCx = min.X >> 4;
                int minCz = min.Z >> 4;
                int maxCx = max.X >> 4;
                int maxCz = max.Z >> 4;
                int minY = min.Y;
                if (minY < 0) minY = 0; else if (minY > numberBlocks) minY = numberBlocks;
                int maxY = max.Y;
                if (maxY < 0) maxY = 0; else if (maxY > numberBlocks) maxY = numberBlocks;

                int xb, zb, x, y, z;
                int xc, zc;
                BlockPos blockPos = new BlockPos();

                for (xc = minCx; xc <= maxCx; xc++)
                {
                    for (zc = minCz; zc <= maxCz; zc++)
                    {
                        ChunkBase chunk = world.GetChunk(xc, zc);
                        for (x = min.X; x <= max.X; x++)
                        {
                            if (x >> 4 == xc)
                            {
                                xb = x & 15;
                                for (z = min.Z; z <= max.Z; z++)
                                {
                                    if (z >> 4 == zc)
                                    {
                                        zb = z & 15;
                                        for (y = minY; y <= maxY; y++)
                                        {
                                            if (chunk != null)
                                            {
                                                blockPos.X = x;
                                                blockPos.Y = y;
                                                blockPos.Z = z;
                                                _CheckBlock(world, blockPos,
                                                    chunk.GetBlockStateNotCheckLight(xb, y, zb));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Точка, проверяем по позиции
                BlockPos blockPos = new BlockPos(Entity.PosX, Entity.PosY, Entity.PosZ);
                _CheckBlock(world, blockPos, world.GetBlockState(blockPos));
            }
        }

        private void _EmptyBlocks()
        {
            IsInLiquid = false;
            IsImpulse = false;
            IsSlow = false;
            if (_numberAutoJump > 0)
            {
                _numberAutoJump--;
                if (_numberAutoJump == 0)
                {
                    IsInLiquidAutoJump = false;
                }
            }
        }

        private void _CheckBlock(WorldBase world, BlockPos blockPos, BlockState blockState)
        {
            // TODO::2026-05-26 тут определение в каких блоках находится сущность для изменения движения
            BlockBase block = blockState.GetBlock();
            if (block.Material.Liquid)
            {
                if (blockState.Met != 0)
                {
                    Vector3 vec = BlockLiquid.GetVectorFlow(world, blockPos);
                    ImpulseX = vec.X * .026f;
                    ImpulseY = vec.Y * .026f;
                    ImpulseZ = vec.Z * .026f;
                    IsImpulse = true;
                }
                IsInLiquid = true;
                IsInLiquidAutoJump = true;
                _numberAutoJump = 5;
            }
            else if (block.Material.IndexMaterial == (int)EnumMaterial.Plant)
            {
                FactorSlowXZ = .67f;
                FactorSlowY = .85f;
                IsSlow = true;
            }
        }

        public override string ToString()
        {
            return (IsInLiquid ? " Liquid" : "") 
                + (IsSlow ? " Slow" : "")
                + (IsImpulse ? " Impulse" : "")
                + (IsInLiquidAutoJump ? " AutoJump" : "");
        }
    }
}
