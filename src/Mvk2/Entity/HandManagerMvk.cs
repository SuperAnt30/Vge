using Mvk2.Entity.List;
using Mvk2.World.Block;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vge.Entity.Player;
using Vge.Games;
using Vge.Item;
using Vge.Network.Packets.Client;
using Vge.Util;
using Vge.World.Block;
using Vge.World.Chunk;

namespace Mvk2.Entity
{
    /// <summary>
    /// Менеджер действий руки, на клиенте для Малювек
    /// ЛКМ и ПКМ
    /// </summary>
    public class HandManagerMvk : HandManager
    {
        protected readonly PlayerClientOwnerMvk _player;
        /// <summary>
        /// Класс игры
        /// </summary>
        protected readonly GameBase _game;

        /// <summary>
        /// Прочность блока, сколько надо ударов для разрушения
        /// </summary>
        private int _hardness;
        /// <summary>
        /// Текущее значение разрушения
        /// </summary>
        private int _destroy;
        /// <summary>
        /// Координаты блока которого разрушаем
        /// </summary>
        private BlockPos _blockPos;
        /// <summary>
        /// Разрушаем ли мы блок
        /// </summary>
        private bool _isBlockDestroy;

        public HandManagerMvk(GameBase game, PlayerClientOwnerMvk player)
        {
            _game = game;
            _player = player;
        }

        /// <summary>
        /// Такт действия
        /// </summary>
        protected override void _Action(bool begin)
        {
            if (_player.IsSpectator()) return;

            if (begin) _pauseAction = 0;

            if (_pauseAction == 0)
            {
                // == 1 OnItemUse
                // == 2 OnItemOnBlockUse
                // == 3 OnBlockUse
                // == 4 OnEntityUse

                MovingObjectPosition moving = _player.MovingObject;

                // Стак предмета в руке
                ItemStack itemStack = _player.InvPlayer.GetCurrentItem();

                if (itemStack != null)
                {
                    if (itemStack.OnItemUse(_player))
                    {
                        // 1. Действие предмета, независимо от выбранного блока или сущности
                        _pauseAction = 8;
                        return;
                    }
                    if (moving.IsBlock())
                    {
                        if (itemStack.OnItemOnBlockUse(_player, moving.BlockPosition, moving.Side, moving.Facing))
                        {
                            // 2. Действие предмета зависит от выбранного блока
                            _pauseAction = 8;
                            return;
                        }
                    }
                    else if (moving.IsEntity())
                    {
                        // 4. Действие на сущность, с предметом
                        _pauseAction = 8;
                        return;
                    }
                }

                if (moving.IsBlock())
                {
                    // 3. Действие на блок без предмета
                    _player.TestHandAction = true;
                    _BlockDigging(begin, moving.BlockPosition);
                    _pauseAction = 8;
                    return;
                }
                else if (moving.IsEntity())
                {
                    // 4. Действие на сущность, без предметом
                    _pauseAction = 8;
                    return;
                }

                return;

                if (_player.MovingObject.IsBlock())
                {
                    _player.TestHandAction = true; // TODO:: 2026-02-27 Убрать, для анимации отладка

                    // Удар по блоку

                    BlockPos blockPos = _player.MovingObject.BlockPosition;
                    if (!_isBlockDestroy || !blockPos.Equals(_blockPos))
                    {
                        begin = true;
                        _blockPos = blockPos;
                        _isBlockDestroy = true;
                    }
                    else
                    {
                        // Если блок тот же самый, то отменяем началку, удары могут быть кликами
                        begin = false;
                    }

                    BlockState blockState = _game.World.GetBlockState(_blockPos);
                    BlockBase block = blockState.GetBlock();

                    if (block.Hardness > 0)
                    {
                        if (begin)
                        {
                            _hardness = block.Hardness;
                        }

                        // Определяем текущее фиксированное состояние разрушения блока, для старта и если кто-то тоже ломает
                        int destroyFix = _game.World.GetBlockDestroy(_blockPos);
                        // Получить предположительное значение если разрушать
                        int destroy = destroyFix * _hardness / 8;

                        if (begin || destroy > _destroy)
                        {
                            // Если первый клик или значение больше, .т.е. разрушили больше, мы меняем, кто-то помогает
                            _destroy = destroy;
                        }
                        _destroy++;

                        if (_destroy > _hardness)
                        {
                            // Разрушили
                            _BlockDestroyed();
                        }
                        else
                        {
                            // Разрушаем, расчитываем значение для фиксации разрушения
                            destroy = _destroy * 8 / _hardness;
                            if (destroy != destroyFix)
                            {
                                _game.World.SetBlockDestroy(_blockPos, (byte)destroy);
                                _game.TrancivePacket(new PacketC07PlayerDigging(_blockPos, (byte)destroy));
                            }
                        }
                    }
                    else
                    {
                        // Разрушили
                        _BlockDestroyed();
                    }
                }
                else if (_player.MovingObject.IsEntity())
                {
                    // Сущность
                    _isBlockDestroy = false;
                }
                else
                {
                    // Воздух, ни в кого
                    _isBlockDestroy = false;

                    // Типа броска
                    // _game.TrancivePacket(new PacketC07PlayerDigging(new BlockPos(), PacketC07PlayerDigging.EnumDigging.About));
                }
                _pauseAction = 7;
            }
            else if(_pauseAction > 0)
            {
                _pauseAction--;
            }
        }

        /// <summary>
        /// Конец действия
        /// </summary>
        protected override void _ActionEnd()
        {
            //Console.WriteLine("HandAction-End");

        }

        /// <summary>
        /// Такт действия
        /// </summary>
        protected override void _Second(bool begin)
        {
            if (_player.IsSpectator()) return;

            if (begin) _pauseSecond = 0;

            if (_pauseSecond == 0)
            {
                //if (!IsSpectator() && true)
               // Console.WriteLine(begin ? "HandnSecond-Begin" : "HandSecond");

                MovingObjectPosition moving = _player.MovingObject;
                if (moving.IsEntity())
                {
                    _game.TrancivePacket(new PacketC03UseEntity(moving.Entity.Id,
                        PacketC03UseEntity.EnumAction.Interact));

                }
                else if (moving.IsBlock())
                {
                    ItemStack itemStack = _player.InvPlayer.GetCurrentItem();
                    if (itemStack != null)
                    {
                        // Действие предмета в руке
                        BlockPos blockPos = moving.BlockPosition;
                        //if (!_game.World.GetBlockState(blockPos).GetBlock().IsReplaceable)
                        //{
                        //    blockPos = blockPos.Offset(moving.Side);
                        //}
                        if (itemStack.Item.OnItemOnBlockUseSecond(itemStack, _player, blockPos,
                             moving.Side, moving.Facing))
                        {
                            _game.TrancivePacket(new PacketC08PlayerBlockPlacement(blockPos,
                                moving.Side, moving.Facing));
                            _pauseSecond = 8;
                        }
                    }
                    else
                    {
                        // пип, нет предмета. или какието действия голой рукой
                        Console.WriteLine("PipBlock");
                        _pauseSecond = -1;
                    }
                    //    _game.World.SetBlockState(moving.BlockPosition.Offset(moving.Side),
                    //        new BlockState(BlocksRegMvk.GlassBlue.IndexBlock), 46);
                    //_game.TrancivePacket(new PacketC08PlayerBlockPlacement(moving.BlockPosition,
                    //    moving.Side, moving.Facing));
                }
                else
                {
                    Console.WriteLine("PipNull");
                    _pauseSecond = -1;
                }
                
            }
            else if (_pauseSecond > 0)
            {
                _pauseSecond--;
            }
        }

        /// <summary>
        /// Конец действия
        /// </summary>
        protected override void _SecondEnd()
        {
          //  Console.WriteLine("HandSecond-End");
        }

        /// <summary>
        /// Разрушение блока
        /// </summary>
        private void _BlockDigging(bool begin, BlockPos blockPos)
        {
            if (!_isBlockDestroy || !blockPos.Equals(_blockPos))
            {
                begin = true;
                _blockPos = blockPos;
                _isBlockDestroy = true;
            }
            else
            {
                // Если блок тот же самый, то отменяем началку, удары могут быть кликами
                begin = false;
            }

            BlockState blockState = _game.World.GetBlockState(_blockPos);
            BlockBase block = blockState.GetBlock();

            if (block.Hardness > 0)
            {
                if (begin)
                {
                    _hardness = block.Hardness;
                }

                // Определяем текущее фиксированное состояние разрушения блока, для старта и если кто-то тоже ломает
                int destroyFix = _game.World.GetBlockDestroy(_blockPos);
                // Получить предположительное значение если разрушать
                int destroy = destroyFix * _hardness / 8;

                if (begin || destroy > _destroy)
                {
                    // Если первый клик или значение больше, .т.е. разрушили больше, мы меняем, кто-то помогает
                    _destroy = destroy;
                }
                _destroy++;

                if (_destroy > _hardness)
                {
                    // Разрушили
                    _BlockDestroyed();
                }
                else
                {
                    // Разрушаем, расчитываем значение для фиксации разрушения
                    destroy = _destroy * 8 / _hardness;
                    if (destroy != destroyFix)
                    {
                        _game.World.SetBlockDestroy(_blockPos, (byte)destroy);
                        _game.TrancivePacket(new PacketC07PlayerDigging(_blockPos, (byte)destroy));
                    }
                }
            }
            else
            {
                // Разрушили
                _BlockDestroyed();
            }
        }

        /// <summary>
        /// Разрушили выбранный блок
        /// </summary>
        private void _BlockDestroyed()
        {
            _game.TrancivePacket(new PacketC07PlayerDigging(_blockPos));
            _game.World.SetBlockToAir(_blockPos, 46);
            _isBlockDestroy = false;
        }

        
    }
}
