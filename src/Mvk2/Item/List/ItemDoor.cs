using Mvk2.World.Block.List;
using System.Runtime.CompilerServices;
using Vge.Entity.Player;
using Vge.Item;
using Vge.Item.List;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using WinGL.Util;

namespace Mvk2.Item.List
{
    /// <summary>
    /// Предмет дверь
    /// </summary>
    public class ItemDoor : ItemBlock
    {
        private readonly BlockDoor _blockDoor;

        public ItemDoor(BlockDoor blockDoor) : base(blockDoor) 
            => _blockDoor = blockDoor;

        #region Дейстыия рук, ЛКМ и ПКМ

        /// <summary>
        /// Дополнительная проверка на moving, чтоб сократить последующие проверки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _CheckMoving(MovingObjectPosition moving)
            => moving.IsBlock() && moving.Side == Pole.Up;

        /// <summary>
        /// Сместить начальное положение
        /// </summary>
        protected override BlockPos _ShiftInitialPosition(BlockPos blockPos, Pole side,
            Vector3 facing, float playerYaw)
        {
            // Определяем на какую сторону смотрит игрок
            if (side == Pole.Up || side == Pole.Down)
            {
                // Стороны по playerYaw
                side = (Pole)PoleConvert.Reverse[(int)PoleConvert.FromAngle(playerYaw)];
            }
            // Определение с какой стороны петля
            bool left = PoleConvert.IsFromAngleLeft(playerYaw, side);

            if (side == Pole.East)
            {
                blockPos.X--;
                if (left) blockPos.Z--;
            }
            else if (side == Pole.South)
            {
                blockPos.Z--;
                if (!left) blockPos.X--;
            }
            else if (side == Pole.North && left) blockPos.X--;
            else if (side == Pole.West && !left) blockPos.Z--;

            return blockPos;
        }

        /// <summary>
        /// Установить дополнительные блоки, только на сервере
        /// </summary>
        protected override bool _InstallAdditionalBlocks(PlayerServer player, BlockPos blockPos,
            Pole side, Vector3 facing, float playerYaw)
        {
            WorldServer world = player.GetWorldServer();

            // Определяем на какую сторону смотрит игрок
            if (side == Pole.Up || side == Pole.Down)
            {
                // Стороны по playerYaw
                side = (Pole)PoleConvert.Reverse[(int)PoleConvert.FromAngle(playerYaw)];
            }
            // Определение с какой стороны петля
            bool left = PoleConvert.IsFromAngleLeft(playerYaw, side);

            int add = BlockDoor.ConvMetData(false, left, (int)side - 2);

            bool result = true;
            BlockPos blockPosCheck;
            BlockState blockStateOld;
            for (int x = 0; x < 2; x++)
            {
                for (int z = 0; z < 2; z++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        if (x != 0 || z != 0 || y != 0)
                        {
                            blockPosCheck = blockPos.Offset(x, y, z);
                            blockStateOld = world.GetBlockState(blockPosCheck);
                            int met = BlockDoor.ConvMetData(x, y, z) + add;
                            if (!world.SetBlockState(blockPosCheck, 
                                new BlockState(Block.IndexBlock | met << 12), 47))
                            {
                                result = false;
                            }
                            else
                            {
                                if (!player.CreativeMode)
                                {
                                    blockStateOld.GetBlock().DropBlockAsItem(player.GetWorldServer(),
                                        player, blockPos, blockStateOld);
                                }
                            }
                        }
                    }
                }
            }

            if (!result)
            {
                // Откатать на воздух!
                for (int x = 0; x < 2; x++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        for (int y = 0; y < 4; y++)
                        {
                            world.SetBlockToAir(blockPos.Offset(x, y, z));
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Проверка, может ли блок устанавливаться в этом месте
        /// </summary>
        /// <param name="blockPos">Координата блока, по которому щелкают правой кнопкой мыши</param>
        /// <param name="block">Объект блока который хотим установить</param>
        /// <param name="side">Сторона, по которой щелкнули правой кнопкой мыши</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        protected override bool _CanPlaceBlockOnSide(ItemStack stack, PlayerBase player, WorldBase world,
            BlockPos blockPos, BlockBase block, Pole side, Vector3 facing, float playerYaw)
        {
            for (int x = 0; x < 2; x++)
            {
                for (int z = 0; z < 2; z++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        if (!base._CanPlaceBlockOnSide(stack, player, world, 
                            blockPos.Offset(x, y, z), Block, side, facing, playerYaw))
                        {
                            return false;
                        }
                    }
                }
            }

            // Определяем на какую сторону смотрит игрок
            if (side == Pole.Up || side == Pole.Down)
            {
                // Стороны по playerYaw
                side = (Pole)PoleConvert.Reverse[(int)PoleConvert.FromAngle(playerYaw)];
            }
            // Определение с какой стороны петля
            bool left = PoleConvert.IsFromAngleLeft(playerYaw, side);

            return _blockDoor.CanDoorStay(world, blockPos, left, side);
        }

        #endregion
    }
}
