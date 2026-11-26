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
        /// <param name="preview">Предпросмотр установки, true - без установки блока(ов)</param>
        /// <param name="begin">Первый клик, без паузы</param>
        /// <param name="counter">Счётчик тактов от нажатия вспомогательное действия ПКМ</param>
        public override ResultHandSecond OnSecond(bool preview, bool begin, ItemStack stack, 
            PlayerClientOwner player, int counter)
        {
            ResultHandSecond result = _CheckEntitySecond(player);
            if (result.Action != ResultHandSecond.ActionType.None)
            {
                return result;
            }

            MovingObjectPosition moving = player.MovingObject;
            if (_CheckMoving(preview, moving, player))
            {
                if (OnItemOnBlockPlacement(preview, stack, player, moving.BlockPosition, moving.Side, 
                    moving.Facing, true, player.RotationYaw))
                {
                    return new ResultHandSecond(_Pause(begin), true)
                    {
                        UseDelay = _DelayPlacement()
                    };
                }
            }

            if (preview) _PlaySampleNotCanPlacement(player);
            return _ResultHandSecondNone();
        }

        /// <summary>
        /// Не удачная установка блока
        /// </summary>
        protected virtual ResultHandSecond _ResultHandSecondNone()
        {
            return new ResultHandSecond(ResultHandSecond.ActionType.None);
        }

        /// <summary>
        /// Проиграть семпл, не может установить предмет
        /// </summary>
        protected virtual void _PlaySampleNotCanPlacement(PlayerClientOwner player)
        {
            player.PlaySampleNotCanPlacement();
        }

        /// <summary>
        /// Дополнительная проверка на moving, чтоб сократить последующие проверки
        /// </summary>
        /// <param name="preview">Предпросмотр установки, true - без установки блока(ов)</param>
        protected virtual bool _CheckMoving(bool preview, MovingObjectPosition moving, PlayerClientOwner player)
            => moving.IsBlock();

        /// <summary>
        /// Пауза между установками блоков
        /// </summary>
        /// <param name="begin">Первый блок</param>
        protected virtual int _Pause(bool begin) => begin ? 8 : 4;

        /// <summary>
        /// Задержка до установки блока.
        /// По умолчанию HandManager.SecondDelay = 2
        /// </summary>
        protected virtual int _DelayPlacement() => HandManager.SecondDelay;

        /// <summary>
        /// Вызывается, когда текущий предмет пробуюет установить на блок,
        /// возвращает true если действие состоялось
        /// </summary>
        /// <param name="preview">Предпросмотр установки, true - без установки блока(ов)</param>
        /// <param name="pos">Позиция блока, по которому щелкают ПКМ</param>
        /// <param name="side">Сторона, по которой щелкнули ПКМ</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        /// <param name="flagReplaceable">Надо ли проверять смещение на установку блока параметра IsReplaceable</param>
        /// <param name="playerYaw">Угол Yaw игрока в момент установки блока</param>
        public override bool OnItemOnBlockPlacement(bool preview, ItemStack stack, PlayerBase player,
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
                   CreateBlockState(stack), side, facing, playerYaw);
                if (Block.CanBlockStay(world, null, blockPos, blockState.Met))
                {
                    if (preview)
                    {
                        return _CanPlaceBlockPreview(player);
                    }
                    // Далее уже попытка установить блок(и)
                    BlockState blockStateOld = world.GetBlockState(blockPos);
                    if (world.SetBlockState(blockPos, blockState, BlockFlag.cParticle))
                    {
                        byte amount = _InstallAdditionalBlocks(player, blockState, blockPos, side, facing, playerYaw);
                        if (amount > 0)
                        {
                            if (!world.IsRemote && player.IsDropBreakingBlock()
                                && player is PlayerServer playerServer)
                            {
                                blockStateOld.GetBlock().DropBlockAsItem(playerServer.GetWorldServer(), 
                                    null, blockPos, blockStateOld);

                                if (Block.Material.IsSamplePlace())
                                {
                                    world.PlaySound(Block.Material.SamplePlace(world.Rnd),
                                        blockPos.ToVector3Center(), 1, 1, player.Id);
                                }
                            }

                            // Уменьшить количество предметом в текущей руке
                            _DecrCurrentItem(player, amount);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Прошла проверку возможности установить блок в превью.
        /// Только клиент, так как превью у него только.
        /// </summary>
        protected virtual bool _CanPlaceBlockPreview(PlayerBase player) => true;

        /// <summary>
        /// Уменьшить количество предметом в текущей руке
        /// </summary>
        protected virtual void _DecrCurrentItem(PlayerBase player, byte amount)
        {
            if (!player.CreativeMode)
            {
                player.Inventory.DecrCurrentItem(amount);
            }
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
        public override bool CanOutlineBlock() => true;

        /// <summary>
        /// Создать структура блок да установки
        /// </summary>
        protected virtual BlockState CreateBlockState(ItemStack stack) => new BlockState(Block.IndexBlock);

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
                CreateBlockState(stack), side, facing, player.RotationYaw);

            return new YouCanPlaceIt(canPlace, Block.GetOutline(blockPos, blockState.Met));
        }

        #endregion
    }
}
