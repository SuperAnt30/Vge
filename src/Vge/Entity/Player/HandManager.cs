using Vge.Games;
using Vge.Item;
using Vge.Network.Packets.Client;
using Vge.Util;
using Vge.World.Block;
using WinGL.Util;

namespace Vge.Entity.Player
{
    /// <summary>
    /// Объект менеджер действий рук игрока. Левой и правой клавишей мыши.
    /// В прошлом было объект ItemInWorldManager, что-то подобное, но там именно очередь ударов
    /// </summary>
    public class HandManager
    {
        protected readonly PlayerClientOwner _player;
        /// <summary>
        /// Класс игры
        /// </summary>
        protected readonly GameBase _game;

        /// <summary>
        /// Пауза для активного действия (ЛКМ)
        /// </summary>
        protected int _pauseAction;
        /// <summary>
        /// Пауза для вспомогательное действие ПКМ
        /// </summary>
        protected int _pauseSecond;
        /// <summary>
        /// Была ли остановка с отменой, флаг для вспомогательного действия
        /// </summary>
        protected bool _flagAbort;

        /// <summary>
        /// Счётчик тактов от нажатия вспомогательное действия ПКМ
        /// </summary>
        private int _counterSecond;
        /// <summary>
        /// Активно ли действие ЛКМ
        /// </summary>
        private bool _action;
        /// <summary>
        /// Было ли активно действие ЛКМ в прошлом такте
        /// </summary>
        private bool _actionPrev;
        /// <summary>
        /// Активно ли вспомогательное действие ПКМ
        /// </summary>
        private bool _second;
        /// <summary>
        /// Было ли вспомогательное действие ПКМ в прошлом такте
        /// </summary>
        private bool _secondPrev;

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
        /// <summary>
        /// Флаг запуска вспомогательного предмета
        /// </summary>
        private bool _flagBeginUseSecond;


        public HandManager(GameBase game, PlayerClientOwner player)
        {
            _game = game;
            _player = player;
        }


        public void SetStop()
        {
            _action = false;
            _second = false;
            _flagAbort = true;
        }

        /// <summary>
        /// Изменить состояние активного действия ЛКМ
        /// </summary>
        public void SetAction(bool action) => _action = action;

        /// <summary>
        /// Изменить состояние вспомогательного действия ПКМ
        /// </summary>
        public void SetSecond(bool action) => _second = action;
        
        /// <summary>
        /// В такте
        /// </summary>
        public void Update()
        {
            if (_action)
            {
                _Action(!_actionPrev);
            }
            else if (_actionPrev)
            {
                _ActionEnd();
            }
            if (_second)
            {
                if (!_secondPrev)
                {
                    _counterSecond = 0;
                    _flagAbort = false;
                }
                _Second(!_secondPrev);
            }
            else if (_secondPrev)
            {
                _SecondEnd();
            }
            _actionPrev = _action;
            _secondPrev = _second;
            _counterSecond++;
        }

        /// <summary>
        /// Такт действия
        /// </summary>
        protected virtual void _Action(bool begin)
        {
            if (_player.IsSpectator()) return;

            if (begin) _pauseAction = 0;

            if (_pauseAction == 0)
            {
                _player.TestHandAction = true;

                MovingObjectPosition moving = _player.MovingObject;
                // Стак предмета в руке
                ItemStack itemStack = _player.Inventory.GetCurrentItem();

                if (itemStack != null)
                {
                    ResultHandAction resultAction = itemStack.Item.OnAction(begin, itemStack, _player);
                    if (resultAction.Action == ResultHandAction.ActionType.DestroyBlock)
                    {
                        // Разрушение блока с предметом
                        //Console.WriteLine("BlockDiggingItem");
                        _BlockDigging(moving.BlockPosition, resultAction.Acceleration);
                    }
                    else if (resultAction.Action == ResultHandAction.ActionType.AttackEntity)
                    {
                        // Атака на сущность, с предметом
                        //Console.WriteLine("AttackItem");
                        _game.TrancivePacket(new PacketC03UseEntity(moving.Entity.Id,
                            PacketC03UseEntity.EnumAction.Attack));
                    }
                    else if (resultAction.Action == ResultHandAction.ActionType.ItemOnBlock)
                    {
                        // Размещение блока (Взаимодействие предмета на выбранный блок)
                        //Console.WriteLine("BlockPlacement " + resultAction.Pause);
                        _game.TrancivePacket(new PacketC08PlayerBlockPlacement(moving.BlockPosition,
                                moving.Side, moving.Facing, resultAction.Replaceable, true));
                    }
                    else if (resultAction.Action == ResultHandAction.ActionType.UseItem)
                    {
                        // Взаимодействия предмета, ЛКМ без блока
                        //Console.WriteLine("UseItem");
                        _game.TrancivePacket(new PacketC05UseItem());
                    }
                    else
                    {
                        // Нет действий с предметом
                        //Console.WriteLine("Нет действий с предметом");
                    }
                    _pauseAction = resultAction.Pause;
                }
                else if (moving.IsBlock())
                {
                    if (_player.CanDestroyedBlock(moving.Block.GetBlock()))
                    {
                        // Разрушение блока без предмета
                        _BlockDigging(moving.BlockPosition);
                        //Console.WriteLine("BlockDigging");
                        _pauseAction = 8;
                    }
                    else
                    {
                        _pauseAction = -1; // Но тут если не begin нет анимации
                    }
                }
                else if (moving.IsEntity())
                {
                    // Атака на сущность, без предмета
                    //Console.WriteLine("Attack");
                    _game.TrancivePacket(new PacketC03UseEntity(moving.Entity.Id,
                        PacketC03UseEntity.EnumAction.Attack));
                    _pauseAction = -1;
                }
                else
                {
                    // Нет действий без предмета
                    //Console.WriteLine("Нет действий без предмета");
                    _pauseAction = -1;
                }
            }
            else if (_pauseAction > 0)
            {
                _pauseAction--;
            }

            if (_pauseAction == -1)
            {
                _pauseAction = 16;
            }
        }

        /// <summary>
        /// Конец действия
        /// </summary>
        protected virtual void _ActionEnd() { }

        /// <summary>
        /// Такт вспомогательного действия 
        /// </summary>
        protected virtual void _Second(bool begin)
        {
            if (_player.IsSpectator()) return;

            if (begin) _pauseSecond = 0;

            if (_pauseSecond == 0)
            {
                _flagBeginUseSecond = false;
                MovingObjectPosition moving = _player.MovingObject;

                if (!_player.IsKeyShift() && moving.IsBlock() && moving.Block.GetBlock().OnBlockActivated(
                    _player, moving.BlockPosition, moving.Side, moving.Facing))
                {
                    // Активация блока
                    _game.TrancivePacket(new PacketC08PlayerBlockPlacement(moving.BlockPosition,
                                    moving.Side, moving.Facing, false, false));
                    _pauseSecond = -1;
                }
                else
                {
                    // Стак предмета в руке
                    ItemStack itemStack = _player.Inventory.GetCurrentItem();

                    if (itemStack != null)
                    {
                        ResultHandSecond resultSecond = itemStack.Item.OnSecond(begin,
                            itemStack, _player, _counterSecond);
                        if (resultSecond.Action == ResultHandSecond.ActionType.BlockPlacement)
                        {
                            // Размещение блока (Взаимодействие предмета на выбранный блок)
                            //Console.WriteLine("BlockPlacement " + resultSecond.Pause);
                            _game.TrancivePacket(new PacketC08PlayerBlockPlacement(moving.BlockPosition,
                                    moving.Side, moving.Facing, resultSecond.Replaceable, true));
                        }
                        else if (resultSecond.Action == ResultHandSecond.ActionType.InteractEntity)
                        {
                            // Взаимодействие на сущность, с предметом
                            //Console.WriteLine("InteractItem");
                            _game.TrancivePacket(new PacketC03UseEntity(moving.Entity.Id,
                                PacketC03UseEntity.EnumAction.Interact));
                        }
                        else if (resultSecond.Action == ResultHandSecond.ActionType.UseItem)
                        {
                            // Взаимодействия предмета, ПКМ без блока
                            _flagBeginUseSecond = true;
                            // Console.WriteLine("UseItem");
                            _game.TrancivePacket(new PacketC05UseItem(resultSecond.Number));
                        }
                        else
                        {
                            // Нет вспомогательного действий с предметом
                            //Console.WriteLine("Нет вспомогательного действий с предметом");
                        }
                        _pauseSecond = resultSecond.Pause;
                    }
                    else if (moving.IsBlock())
                    {
                        //OnBlockActivated
                        // Клик ПКМ на блок без предмета
                        //Console.WriteLine("Click RightMouse");
                        _pauseSecond = 8;
                    }
                    else if (moving.IsEntity())
                    {
                        // Взаимодействие на сущность, без предмета
                        //Console.WriteLine("Interact");
                        _game.TrancivePacket(new PacketC03UseEntity(moving.Entity.Id,
                            PacketC03UseEntity.EnumAction.Interact));
                        _pauseSecond = -1;
                    }
                    else
                    {
                        // Нет вспомогательного действий без предмета
                        //Console.WriteLine("Нет вспомогательного действий без предмета");
                        _pauseSecond = -1;
                    }
                }
            }
            else if (_pauseSecond > 0)
            {
                _pauseSecond--;
            }
        }

        /// <summary>
        /// Конец вспомогательного действия
        /// </summary>
        protected virtual void _SecondEnd()
        {
            if (_player.IsSpectator()) return;
            if (_flagBeginUseSecond)
            {
                ItemStack itemStack = _player.Inventory.GetCurrentItem();
                if (itemStack != null)
                {
                    int number = itemStack.Item.OnSecondEnd(itemStack, _player,
                        _flagAbort, _counterSecond);
                    if (number >= 0)
                    {
                        _game.TrancivePacket(new PacketC05UseItem(number));
                    }
                }
            }
        }

        /// <summary>
        /// Разрушение блока
        /// </summary>
        private void _BlockDigging(BlockPos blockPos, float power = 1)
        {
            bool begin;
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

            if (block.Hardness > 0 && !_player.CreativeMode)
            {
                if (begin)
                {
                    _hardness = (int)(block.Hardness * power);
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
                    _BlockDestroyed(block);
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
                    _game.World.PlaySound(block.Material.SampleBreak(_game.World.Rnd),
                        _blockPos.ToVector3Center(), 1, .9f + _game.World.Rnd.NextFloat() * .2f);

                    if (block.IsParticle)
                    {
                        _game.World.SpawnParticle(0, 5, _player.MovingObject.RayHit,
                            new Vector3(.25f), 1, block.IndexBlock);
                    }
                }
            }
            else
            {
                // Разрушили
                _BlockDestroyed(block);
            }
        }

        /// <summary>
        /// Разрушили выбранный блок
        /// </summary>
        private void _BlockDestroyed(BlockBase block)
        {
            _game.TrancivePacket(new PacketC07PlayerDigging(_blockPos));
            _game.World.SetBlockToAir(_blockPos, 46);
            _isBlockDestroy = false;

            if (block.IsParticle)
            {
                _game.World.SpawnParticle(0, 25, _blockPos.ToVector3Center(),
                    new Vector3(1), 1, block.IndexBlock);
            }
        }
    }
}
