using Mvk2.World.Block.List;
using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Player;
using Vge.Item;
using Vge.Item.List;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using Vge.World.Chunk;
using WinGL.Util;

namespace Mvk2.Item.List
{
    /// <summary>
    /// Предмет верёвка
    /// </summary>
    public class ItemRope : ItemBlock
    {
        private readonly BlockRope _blockRope;

        public ItemRope(BlockRope blockRope) : base(blockRope)
            => _blockRope = blockRope;

        
        /// <summary>
        /// Дополнительная проверка на moving, чтоб сократить последующие проверки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _CheckMoving(MovingObjectPosition moving)
            => moving.IsBlock() && moving.Side != Pole.Up;

        /// <summary>
        /// Установить дополнительные блоки, только на сервере.
        /// Вернёт количество дополнительных предметов на удаление. -1, не смог установить
        /// </summary>
        /// <param name="blockState">Данные блока которые уже установленны на blockPos</param>
        protected override byte _InstallAdditionalBlocks(PlayerBase player, BlockState blockState,
            BlockPos blockPos, Pole side, Vector3 facing, float playerYaw)
        {
            byte count = 1;
            WorldBase world = player.GetWorld();
            ChunkBase chunk = world.GetChunk(blockPos);
            if (chunk != null)
            {
                // Определяем максимальную длинну
                int countMax;
                if (player.CreativeMode)
                {
                    countMax = 100;
                }
                else
                {
                    countMax = player.Inventory.GetCurrentItem() == null 
                        ? 0 : player.Inventory.GetCurrentItem().Amount - 1; // Так-как это дополнительные
                }
                
                while (countMax > 0 && blockPos.Y > 2)
                {
                    blockPos.Y--;
                    if (chunk.GetBlockState(blockPos).IsAir()
                        && chunk.GetBlockState(blockPos.OffsetDown()).IsAir())
                    {
                        if (!world.SetBlockState(blockPos, blockState, 47))
                        {
                            countMax = 0;
                        }
                        else
                        {
                            count++;
                            countMax--;
                        }
                    }
                    else
                    {
                        countMax = 0;
                    }
                }

                //Console.WriteLine("Rope " + count);
            }

            return count;
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
            if (!base._CanPlaceBlockOnSide(stack, player, world,
                blockPos, Block, side, facing, playerYaw))
            {
                return false;
            }

            return _blockRope.CanBlockStay(world, null, blockPos, 
                _blockRope.OnBlockPlaced(world, blockPos, new BlockState(), side, facing, playerYaw).Met);
        }
    }
}
