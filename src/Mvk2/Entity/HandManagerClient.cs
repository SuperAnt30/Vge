using Mvk2.Entity.List;
using Mvk2.World.Block;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vge.Entity.Player;
using Vge.Games;
using Vge.Network.Packets.Client;
using Vge.World.Block;
using Vge.World.Chunk;

namespace Mvk2.Entity
{
    /// <summary>
    /// Менеджер действий руки, на клиенте
    /// ЛКМ и ПКМ
    /// </summary>
    public class HandManagerClient : HandManager
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

        public HandManagerClient(GameBase game, PlayerClientOwnerMvk player)
        {
            _game = game;
            _player = player;
        }

        /// <summary>
        /// Такт действия
        /// </summary>
        protected override void _Action(bool begin)
        {
            //if (!IsSpectator() && true)

            if (begin) _pauseAction = 0;
            //Console.Write(DateTime.Now.Second + ":");
            //Console.Write(DateTime.Now.Millisecond + " ");

            if (_pauseAction <= 0)
            {
                //Console.WriteLine(begin ? "HandAction-Begin" : "HandAction");
                if (_player.MovingObject.IsBlock())
                {
                    _player.TestHandAction = true;

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
            else
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
            if (begin) _pauseSecond = 0;

            if (_pauseSecond <= 0)
            {
                //if (!IsSpectator() && true)
                Console.WriteLine(begin ? "HandnSecond-Begin" : "HandSecond");
                if (_player.MovingObject.IsEntity())
                {
                    _game.TrancivePacket(new PacketC03UseEntity(_player.MovingObject.Entity.Id,
                        PacketC03UseEntity.EnumAction.Interact));

                }
                else if (_player.MovingObject.IsBlock())
                {
                    
                    _game.World.SetBlockState(_player.MovingObject.BlockPosition.Offset(_player.MovingObject.Side),
                        new BlockState(BlocksRegMvk.GlassBlue.IndexBlock), 46);
                    _game.TrancivePacket(new PacketC08PlayerBlockPlacement(_player.MovingObject.BlockPosition,
                        _player.MovingObject.Side, _player.MovingObject.Facing));
                }
                _pauseSecond = 8;
            }
            else
            {
                _pauseSecond--;
            }
        }

        /// <summary>
        /// Конец действия
        /// </summary>
        protected override void _SecondEnd()
        {
            Console.WriteLine("HandSecond-End");
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
