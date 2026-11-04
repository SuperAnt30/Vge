using System.Runtime.CompilerServices;
using Vge.Entity.Player;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using WinGL.Util;

namespace Vge.Item.List
{
    /// <summary>
    /// Предмет который ставится как блок
    /// </summary>
    public class ItemBlock : ItemBase
    {
        /// <summary>
        /// Объект блока который устанавливается этим предметом
        /// </summary>
        public readonly BlockBase Block;

        public ItemBlock(BlockBase block) => Block = block;

        #region Дейстыия рук, ЛКМ и ПКМ

        /// <summary>
        /// Вспомогательное действие предмета ПКМ.
        /// Для клиента.
        /// </summary>
        /// <param name="begin">Первый клик, без паузы</param>
        /// <param name="counter">Счётчик тактов от нажатия вспомогательное действия ПКМ</param>
        public override ResultHandSecond OnSecond(bool begin, ItemStack stack, 
            PlayerClientOwner player, int counter)
        {
            ResultHandSecond result = base.OnSecond(begin, stack, player, counter);
            if (result.Action != ResultHandSecond.ActionType.None)
            {
                return result;
            }

            MovingObjectPosition moving = player.MovingObject;
            if (_CheckMoving(moving))
            {
                if (OnItemOnBlockPlacement(stack, player, moving.BlockPosition, moving.Side, 
                    moving.Facing, true, player.RotationYaw))
                {
                    return new ResultHandSecond(_Pause(begin), true);
                }
            }

            // TODO::2026-10-27 подумать, как один раз фиксировать, begin перезапускается
            //if (begin) 
            player.PlaySampleNotCanPlacement();
            return new ResultHandSecond(ResultHandSecond.ActionType.None);
        }

        /// <summary>
        /// Дополнительная проверка на moving, чтоб сократить последующие проверки
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool _CheckMoving(MovingObjectPosition moving)
            => moving.IsBlock();

        /// <summary>
        /// Пауза между установками блоков
        /// </summary>
        /// <param name="begin">Первый блок</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual int _Pause(bool begin) => begin ? 8 : 4;

        /// <summary>
        /// Вызывается, когда текущий предмет пробуюет установить на блок,
        /// возвращает true если действие состоялось
        /// </summary>
        /// <param name="pos">Позиция блока, по которому щелкают ПКМ</param>
        /// <param name="side">Сторона, по которой щелкнули ПКМ</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        /// <param name="flagReplaceable">Надо ли проверять смещение на установку блока параметра IsReplaceable</param>
        /// <param name="playerYaw">Угол Yaw игрока в момент установки блока</param>
        public override bool OnItemOnBlockPlacement(ItemStack stack, PlayerBase player,
            BlockPos blockPos, Pole side, Vector3 facing, bool flagReplaceable, float playerYaw)
        {
            WorldBase world = player.GetWorld();

            if (flagReplaceable && !world.GetBlockState(blockPos).GetBlock().IsReplaceable)
            {
                blockPos = blockPos.Offset(side);
            }

            // Сместить начальное положение
            blockPos = _ShiftInitialPosition(blockPos, side, facing, playerYaw);

            if (_CanPlaceBlockOnSide(stack, player, world, blockPos, Block, side, facing, playerYaw))
            {
                BlockState blockState = Block.OnBlockPlaced(world, blockPos, 
                    new BlockState(Block.IndexBlock), side, facing, playerYaw);
                if (Block.CanBlockStay(world, null, blockPos, blockState.Met))
                {
                    BlockState blockStateOld = world.GetBlockState(blockPos);
                    if (world.SetBlockState(blockPos, blockState, 47))
                    {
                        byte amount = _InstallAdditionalBlocks(player, blockState, blockPos, side, facing, playerYaw);
                        if (amount > 0)
                        {
                            if (!world.IsRemote && !player.CreativeMode 
                                && player is PlayerServer playerServer)
                            {
                                blockStateOld.GetBlock().DropBlockAsItem(playerServer.GetWorldServer(), 
                                    playerServer, blockPos, blockStateOld);

                                world.PlaySound(Block.Material.SamplePlace(world.Rnd),
                                    blockPos.ToVector3Center(), 1, 1, player.Id);
                            }

                            if (!player.CreativeMode)
                            {
                                player.Inventory.DecrCurrentItem(amount);
                            }
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Сместить начальное положение
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual BlockPos _ShiftInitialPosition(BlockPos blockPos, Pole side, 
            Vector3 facing, float playerYaw) => blockPos;

        /// <summary>
        /// Установить дополнительные блоки.
        /// Вернёт количество предметов на удаление. 0, не смог установить
        /// </summary>
        /// <param name="blockState">Данные блока которые уже установленны на blockPos</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual byte _InstallAdditionalBlocks(PlayerBase player, BlockState blockState,
            BlockPos blockPos, Pole side, Vector3 facing, float playerYaw) => 1;

        /// <summary>
        /// Имеется ли сетка контура для предмета
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool CanOutlineBlock() => true;

        /// <summary>
        /// Получить сетку контура предмета
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override YouCanPlaceIt GetOutlineBlock(ItemStack stack, PlayerBase player,
            BlockPos blockPos, Pole side, Vector3 facing)
        {
            // Сместить начальное положение
            if (!player.GetWorld().GetBlockState(blockPos).GetBlock().IsReplaceable)
            {
                blockPos = blockPos.Offset(side);
            }
            blockPos = _ShiftInitialPosition(blockPos, side, facing, player.RotationYaw);

            bool canPlace = _CanPlaceBlockOnSide(stack, player, player.GetWorld(), blockPos, Block,
                side, facing, player.RotationYaw);

            BlockState blockState = Block.OnBlockPlaced(player.GetWorld(), blockPos,
                new BlockState(Block.IndexBlock), side, facing, player.RotationYaw);

            return new YouCanPlaceIt(canPlace, Block.GetOutline(blockPos, blockState.Met, 0));
        }

        #endregion
    }
}
