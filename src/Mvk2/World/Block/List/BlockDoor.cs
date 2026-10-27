using System.Runtime.CompilerServices;
using Vge.Audio;
using Vge.Entity.Player;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;
using WinGL.Util;

namespace Mvk2.World.Block.List
{
    /// <summary>
    /// Блок двери
    /// </summary>
    public class BlockDoor : BlockBase
    {
        /***
        * Met
        * 
        * 0YYZ Xo00 NLPP
        * 
        * 0-1 bit - Pole
        * 2 bit - Left/Right
        * 3 bit - Null
        * 
        * 6 bit - Open/Close 
        * 7 bit - X
        * 8 bit - Z
        * 9-10 bit - Y
        * 
        * Open = (met & 64) != 0;
        * Left = (met & 4) != 0;
        * pole = met & 3;
        *   East = pole == 0
        *   West = pole == 1
        *   North = pole == 2
        *   South = pole == 3
        * x = met & 128 != 0; 
        * y = met >> 9 & 3;
        * z = met & 256 != 0; 
        *  
        */

        /// <summary>
        /// Индекс семпла открытия
        /// </summary>
        private readonly int _sampleOpen;
        /// <summary>
        /// Индекс семпла закрытия
        /// </summary>
        private readonly int _sampleClose;

        public BlockDoor(MaterialBase material) : base(material)
        {
            _sampleOpen = AudioIndexs.GetKey("DoorOpen");
            _sampleClose = AudioIndexs.GetKey("DoorClose");
        }

        /// <summary>
        /// Конверт данных для нулевого
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ConvMetData(bool open, bool left, int pole) 
            => pole | (left ? 1 : 0) << 2 | (open ? 64 : 0);

        /// <summary>
        /// Конверт данных всех кроме нулевого
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ConvMetData(int x, int y, int z) 
            => y << 9 | z << 8 | x << 7 | 8;

        /// <summary>
        /// Действие перед размещеннием блока, для определения метданных
        /// </summary>
        public override BlockState OnBlockPlaced(WorldBase worldIn, BlockPos blockPos, BlockState state,
            Pole side, Vector3 facing, float playerYaw)
        {
            // Определяем на какую сторону смотрит игрок
            if (side == Pole.Up || side == Pole.Down)
            {
                // Стороны по playerYaw
                side = (Pole)PoleConvert.Reverse[(int)PoleConvert.FromAngle(playerYaw)];
            }
            // Определение с какой стороны петля
            bool left = PoleConvert.IsFromAngleLeft(playerYaw, side);

            return state.NewMet(ConvMetData(false, left, (int)side - 2));
        }

        /// <summary> 
        /// Проверка установка блока, можно ли его установить тут
        /// </summary>
        public bool CanDoorStay(WorldBase world, BlockPos blockPos, bool left, Pole pole)
        {
            // Тут мы проверяем к чему дверь придерживаться будет
            int x1, x2, z1, z2;

            if (pole == Pole.North)
            {
                x1 = left ? 2 : -1;
                x2 = left ? 1 : 0;
                z1 = 0;
                z2 = -1;
            }
            else if (pole == Pole.South)
            {
                x1 = left ? -1 : 2;
                x2 = left ? 0 : 1;
                z1 = 1;
                z2 = 2;
            }
            else if (pole == Pole.West)
            {
                z1 = left ? -1 : 2;
                z2 = left ? 0 : 1;
                x1 = 0;
                x2 = -1;
            }
            else if (pole == Pole.East)
            {
                z1 = left ? 2 : -1;
                z2 = left ? 1 : 0;
                x1 = 1;
                x2 = 2;
            }
            else
            {
                return false;
            }

            return (world.GetBlockState(blockPos.Offset(x1, 0, z1)).GetBlock().FullBlock
                        || world.GetBlockState(blockPos.Offset(x2, 0, z2)).GetBlock().FullBlock)
                        && (world.GetBlockState(blockPos.Offset(x1, 3, z1)).GetBlock().FullBlock
                        || world.GetBlockState(blockPos.Offset(x2, 3, z2)).GetBlock().FullBlock);
        }

        /// <summary>
        /// Активация блока, true - был клик, false - нет такой возможности
        /// </summary>
        /// <param name="pos">Позиция блока, по которому щелкают</param>
        /// <param name="side">Сторона, по которой щелкнули</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        public override bool OnBlockActivated(PlayerBase player, BlockPos blockPos, Pole side, Vector3 facing)
        {
            WorldBase world = player.GetWorld();
            if (!world.IsRemote)
            {
                BlockState state = world.GetBlockState(blockPos);
                int met = state.Met;
                int bx = (met & 128) != 0 ? 1 : 0;
                int bz = (met & 256) != 0 ? 1 : 0;
                int by = met >> 9 & 3;

                bool open = (met & 64) != 0;
                int location = met & 7;

                if (open) met = 0; else met = 64;

                switch (location)
                {
                    case 0: met += 6; break;
                    case 1: met += 7; break; 
                    case 2: met += 5; break;
                    case 3: met += 4; break;
                    case 4: met += 3; break;
                    case 5: met += 2; break;
                    case 6: met += 0; break;
                    case 7: met += 1; break;
                }

                // Получить мет данные дополнительной жидкости
                BlockPos pos = blockPos.Offset(-bx, -by, -bz);
                world.SetBlockStateMet(pos, met | world.GetLiquidBlock(pos));

                for (int x = 0; x < 2; x++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        for (int y = 0; y < 4; y++)
                        {
                            if (x != 0 || z != 0 || y != 0)
                            {
                                pos = blockPos.Offset(x - bx, y - by, z - bz);
                                world.SetBlockStateMet(pos,
                                    (ConvMetData(x, y, z) + met) | world.GetLiquidBlock(pos));
                            }
                        }
                    }
                }

                world.PlaySoundDistance(open ? _sampleOpen : _sampleClose,
                    blockPos.ToVector3Center(), 1f, 1f);
            }
            return true;
        }

        /// <summary>
        /// Действие блока после его удаления
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void OnBreakBlock(WorldBase world, ChunkServer chunk,
            BlockPos blockPos, BlockState stateOld)
        {
            int met = stateOld.Met;
            int bx = (met & 128) != 0 ? 1 : 0;
            int bz = (met & 256) != 0 ? 1 : 0;
            int by = met >> 9 & 3;

            for (int x = 0; x < 2; x++)
            {
                for (int z = 0; z < 2; z++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        world.SetBlockToAir(blockPos.Offset(x - bx, y - by, z - bz), 110); // 2 4 8 32 64 без частичек и звука, и отключен OnBreakBlock
                    }
                }
            }
        }

        /// <summary>
        /// Смена соседнего блока
        /// </summary>
        public override void NeighborBlockChange(WorldServer world, ChunkServer chunk, 
            BlockPos blockPos, BlockState blockState, BlockBase neighborBlock)
        {
            base.NeighborBlockChange(world, chunk, blockPos, blockState, neighborBlock);

            int met = blockState.Met;
            int bx = (met & 128) != 0 ? 1 : 0;
            int bz = (met & 256) != 0 ? 1 : 0;
            int by = met >> 9 & 3;
            Pole pole = (Pole)((met & 3) + 2);
            bool left = (met & 4) != 0;

            if (!CanDoorStay(world, blockPos.Offset(-bx, -by, -bz), left, pole))
            {
                OnBreakBlock(world, chunk, blockPos, blockState);
                DropBlockAsItem(world, null, blockPos, blockState);
                PlaySoundBreak(world, blockPos);
            }
        }

        /// <summary>
        /// Коробки для рендера 
        /// </summary>
        public override QuadSide[] GetQuads(int met, int xb, int zb) => _quads[met & 15];

        /// <summary>
        /// Передать список ограничительных рамок блока
        /// </summary>
        public override AxisAlignedBB[] GetCollisionBoxesToList(BlockPos pos, int met)
        {
            int pole = met & 3;
            int z = (met & 256) != 0 ? 1 : 0;
            if (pole == 2 && z == 0) return new AxisAlignedBB[]
                { new AxisAlignedBB(pos.X, pos.Y, pos.Z, pos.X + 1, pos.Y + 1, pos.Z + .1875f) };
            if (pole == 3 && z == 1) return new AxisAlignedBB[]
                { new AxisAlignedBB(pos.X, pos.Y, pos.Z + .8125f, pos.X + 1, pos.Y + 1, pos.Z + 1) };
            int x = (met & 128) != 0 ? 1 : 0;
            if (pole == 1 && x == 0) return new AxisAlignedBB[]
                { new AxisAlignedBB(pos.X, pos.Y, pos.Z, pos.X + .1875f, pos.Y + 1, pos.Z + 1) };
            if (pole == 0 && x == 1) return new AxisAlignedBB[]
                { new AxisAlignedBB(pos.X + .8125f, pos.Y, pos.Z, pos.X + 1, pos.Y + 1, pos.Z + 1) };

            return new AxisAlignedBB[0];
        }
    }
}
