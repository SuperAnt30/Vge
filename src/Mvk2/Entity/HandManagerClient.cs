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
                Console.WriteLine(begin ? "HandAction-Begin" : "HandAction");
                if (_player.MovingObject.IsBlock())
                {
                    _player.TestHandAction = true;
                    _game.TrancivePacket(new PacketC07PlayerDigging(_player.MovingObject.BlockPosition, PacketC07PlayerDigging.EnumDigging.Destroy));
                    _game.World.SetBlockToAir(_player.MovingObject.BlockPosition, 46);
                }
                else
                {
                    // Типа броска
                    _game.TrancivePacket(new PacketC07PlayerDigging(new BlockPos(), PacketC07PlayerDigging.EnumDigging.About));
                }
                _pauseAction = 15;
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
            Console.WriteLine("HandAction-End");
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
                    //_game.TrancivePacket(new PacketC08PlayerBlockPlacement(_player.MovingObject.BlockPosition,
                    //    _player.MovingObject.Side, _player.MovingObject.Facing));
                    _game.World.SetBlockState(_player.MovingObject.BlockPosition.Offset(_player.MovingObject.Side),
                        new BlockState(BlocksRegMvk.Stone.IndexBlock), 46);
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
    }
}
