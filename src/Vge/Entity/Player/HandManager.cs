using System;
using System.Runtime.CompilerServices;
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
        /// <summary>
        /// Пауза без предмета
        /// </summary>
        public static int PauseNotItem = 8;
        /// <summary>
        /// Задержка удара без предмета, в тактах (момент удара в анимации руки)
        /// </summary>
        public static int HitDelayNotItem = 5;
        /// <summary>
        /// Импульс отскока при атаке на XZ
        /// </summary>
        private static float _impulsAttackXZ = .9f;
        /// <summary>
        /// Импульс отскока при атаке на Y
        /// </summary>
        private static float _impulsAttackY = .18f;

        protected readonly PlayerClientOwner _player;
        /// <summary>
        /// Класс игры
        /// </summary>
        private readonly GameBase _game;

        /// <summary>
        /// Пауза для активного действия (ЛКМ)
        /// </summary>
        private int _pauseAction;
        /// <summary>
        /// Имеется ли холостой выстрел для активного действия в промежутке, не первый!
        /// </summary>
        private bool _blankShotAction = false;
        /// <summary>
        /// Пауза для вспомогательное действие ПКМ
        /// </summary>
        private int _pauseSecond;
        /// <summary>
        /// Была ли остановка с отменой, флаг для вспомогательного действия
        /// </summary>
        private bool _flagAbort;

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

        #region Отложенный удар ЛКМ

        /// <summary>
        /// Тип отложенного действия ЛКМ
        /// </summary>
        private enum PendingKind
        {
            /// <summary>
            /// Нет ожидания
            /// </summary>
            None,
            /// <summary>
            /// Действие предмета (результат OnAction)
            /// </summary>
            Item,
            /// <summary>
            /// Разрушение блока рукой
            /// </summary>
            DigHand,
            /// <summary>
            /// Атака сущности рукой
            /// </summary>
            AttackHand
        }

        /// <summary>
        /// Какое действие ждёт момента удара в анимации
        /// </summary>
        private PendingKind _pendingKind = PendingKind.None;
        /// <summary>
        /// Сколько тактов осталось до удара
        /// </summary>
        private int _pendingDelay;
        /// <summary>
        /// Результат OnAction, принятый в момент клика
        /// </summary>
        private ResultHandAction _pendingResult;

        #endregion

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
            // Замах прерван
            _pendingKind = PendingKind.None;
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
            // Отложенный удар тикает всегда, даже если ЛКМ уже отпустили —
            // замах начат, удар доводим до конца
            if (_pendingKind != PendingKind.None)
            {
                if (_pendingDelay > 0)
                {
                    _pendingDelay--;
                    if (_pendingDelay == 0)
                    {
                        _ExecutePending();
                    }
                }
            }

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
                _secondPrev = _second;
            }
            _actionPrev = _action;
            _counterSecond++;
        }

        /// <summary>
        /// Действие анимации правой руки
        /// </summary>
        /// <param name="itemStack">Предмет которым делаю действие</param>
        /// <param name="moving">Выбранный объект</param>
        protected virtual bool _ActionAnimationRight(ItemStack itemStack, 
            MovingObjectPosition moving) => false;

        /// <summary>
        /// Отправить анимацию на сервер
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _SendPlayerAnimation()
        {
            _game.TrancivePacket(new PacketC0APlayerAnimation(
                _player.Render.AnimationCodeAdd,
                PacketC0APlayerAnimation.EnumAction.CodeAdd,
                _player.Render.SpeedAnimationCodeAdd));
        }

        /// <summary>
        /// Такт действия
        /// Здесь только принимается решение и запускается анимация,
        /// само действие выполняется в момент удара, в _UpdatePending
        /// </summary>
        protected virtual void _Action(bool begin)
        {
            if (_player.IsSpectator()) return;

            if (begin)
            {
                _pauseAction = 0;
                _blankShotAction = false;
            }

            // Новый замах только после паузы и когда прошлый удар уже состоялся
            if (_pauseAction == 0 && _pendingKind == PendingKind.None)
            {
                MovingObjectPosition moving = _player.MovingObject;
                // Стак предмета в руке
                ItemStack itemStack = _player.Inventory.GetCurrentItem();

                bool isAnimationPrev = false;

                if (!_blankShotAction)
                {
                    isAnimationPrev = true;
                    if (_ActionAnimationRight(itemStack, moving))
                    {
                        // Отправить анимацию
                        _SendPlayerAnimation();
                    }
                }

                PendingKind kind = PendingKind.None;
                int delay = 0;
                ResultHandAction resultAction = default;

                if (itemStack != null)
                {
                    resultAction = itemStack.Item.OnAction(begin, itemStack, _player);
                    if (resultAction.Action == ResultHandAction.ActionType.None)
                    {
                        // Нет действий с предметом
                        _blankShotAction = true;
                    }
                    else
                    {
                        Console.WriteLine("OnAction " + resultAction.Action);
                        kind = PendingKind.Item;
                        // Момент удара в анимации предмета, в тактах
                        delay = resultAction.HitDelay;
                    }
                    _pauseAction = resultAction.Pause;
                    //Console.Write(_pauseAction + " ");
                }
                else if (moving.IsBlock())
                {
                    if (_player.CanDestroyedBlock(moving.Block.GetBlock()))
                    {
                        // Разрушение блока без предмета
                        kind = PendingKind.DigHand;
                        delay = HitDelayNotItem;
                        _pauseAction = PauseNotItem;
                        //Console.WriteLine("BlockDigging");
                    }
                    else
                    {
                        //Console.WriteLine("Нет действий без предмета [block]");
                        _pauseAction = -1; // Но тут если не begin нет анимации
                        _blankShotAction = true;
                    }
                }
                else if (moving.IsEntity())
                {
                    // Атака на сущность, без предмета
                    kind = PendingKind.AttackHand;
                    delay = HitDelayNotItem;
                    _pauseAction = PauseNotItem;
                }
                else
                {
                    // Нет действий без предмета
                    //Console.WriteLine("Нет действий без предмета [null]");
                    _pauseAction = -1;
                    _blankShotAction = true;
                }

                if (kind != PendingKind.None)
                {
                    if (!isAnimationPrev && _ActionAnimationRight(itemStack, moving))
                    {
                        // Отправить анимацию
                        _SendPlayerAnimation();
                    }

                    _pendingKind = kind;
                    _pendingDelay = delay;
                    _pendingResult = resultAction;
                    if (delay <= 0)
                    {
                        _ExecutePending();
                    }
                }
                //Console.WriteLine(_pauseAction);
            }
            else if (_pauseAction > 0)
            {
                _pauseAction--;
            }

            if (_pauseAction == -1)
            {
                _pauseAction = _blankShotAction ? 0 : 10;
            }
            //Console.WriteLine(_pauseAction);
        }

        /// <summary>
        /// Момент удара в анимации: выполняем действие по тому, на что смотрим сейчас
        /// </summary>
        private void _ExecutePending()
        {
            PendingKind kind = _pendingKind;
            _pendingKind = PendingKind.None;

            if (_player.IsSpectator()) return;

            // Цель берём на момент удара: за время замаха игрок мог отвернуться.
            // Если цели нужного типа нет — удар мимо
            MovingObjectPosition moving = _player.MovingObject;

            switch (kind)
            {
                case PendingKind.Item:
                    switch (_pendingResult.Action)
                    {
                        case ResultHandAction.ActionType.DestroyBlock:
                            // Разрушение блока с предметом
                            if (moving.IsBlock())
                            {
                                //Console.WriteLine("BlockDiggingItem2");
                                _BlockDigging(moving.BlockPosition, _pendingResult.Acceleration);
                            }
                            break;
                        case ResultHandAction.ActionType.AttackEntity:
                            // Атака на сущность, с предметом
                            if (moving.IsEntity())
                            {
                                _Attack(moving);
                            }
                            break;
                        case ResultHandAction.ActionType.ItemOnBlock:
                            // Размещение блока (Взаимодействие предмета на выбранный блок)
                            if (moving.IsBlock())
                            {
                                //Console.WriteLine("BlockPlacement2 " + _pauseAction);
                                _game.TrancivePacket(new PacketC08PlayerBlockPlacement(moving.BlockPosition,
                                    moving.Side, moving.Facing, _pendingResult.Replaceable, true));
                            }
                            break;
                        case ResultHandAction.ActionType.UseItem:
                            // Взаимодействия предмета, ЛКМ без блока
                            //Console.WriteLine("UseItem2");
                            _game.TrancivePacket(new PacketC05UseItem());
                            break;
                    }
                    break;

                case PendingKind.DigHand:
                    // Разрушение блока без предмета
                    if (moving.IsBlock() && _player.CanDestroyedBlock(moving.Block.GetBlock()))
                    {
                        //Console.WriteLine("BlockDigging2");
                        _BlockDigging(moving.BlockPosition);
                    }
                    break;

                case PendingKind.AttackHand:
                    // Атака на сущность, без предмета
                    if (moving.IsEntity())
                    {
                        _Attack(moving);
                    }
                    break;
            }
        }

        /// <summary>
        /// Атака сущности
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _Attack(MovingObjectPosition moving)
        {
            //Console.WriteLine("Attack");
            Vector3 vec = (moving.Entity.GetPositionVec() - _player.GetPositionVec()).Normalize();
            _game.TrancivePacket(new PacketC03UseEntity(moving.Entity.Id, moving.RayHit.Y,
                vec.X * _impulsAttackXZ, vec.Y * _impulsAttackY, vec.Z * _impulsAttackXZ));
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

            if (begin)
            {
                _pauseSecond = 0;
                _secondPrev = true;
            }

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
                            _game.TrancivePacket(new PacketC08PlayerBlockPlacement(
                                resultSecond.IsLiquid ? moving.BlockLiquidPosition : moving.BlockPosition,
                                    moving.Side, moving.Facing, resultSecond.Replaceable, true));
                            if (_ActionAnimationRight(itemStack, moving))
                            {
                                // Отправить анимацию
                                _SendPlayerAnimation();
                            }
                            _pauseSecond = resultSecond.Pause;
                        }
                        else if (resultSecond.Action == ResultHandSecond.ActionType.InteractEntity)
                        {
                            // Взаимодействие на сущность, с предметом
                            //Console.WriteLine("InteractItem");
                            _game.TrancivePacket(new PacketC03UseEntity(moving.Entity.Id,
                                PacketC03UseEntity.EnumAction.Interact));
                            if (_ActionAnimationRight(itemStack, moving))
                            {
                                // Отправить анимацию
                                _SendPlayerAnimation();
                            }
                            _pauseSecond = resultSecond.Pause;
                        }
                        else if (resultSecond.Action == ResultHandSecond.ActionType.UseItem)
                        {
                            // Взаимодействия предмета, ПКМ без блока
                            _flagBeginUseSecond = true;
                             //Console.WriteLine("UseItem");
                            _game.TrancivePacket(new PacketC05UseItem(resultSecond.Number));
                            if (_ActionAnimationRight(itemStack, moving))
                            {
                                // Отправить анимацию
                                _SendPlayerAnimation();
                            }
                            _pauseSecond = resultSecond.Pause;
                        }
                        else
                        {
                            // Нет вспомогательного действий с предметом
                            //Console.WriteLine("Нет вспомогательного действий с предметом");
                            if (begin) _secondPrev = false;
                        }
                        //Console.WriteLine(_pauseSecond);
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

            BlockState blockState = _game.World.GetBlockState(blockPos);
            BlockBase block = blockState.GetBlock();
            BlockPos blockPosShift = block.ShiftPosition(blockPos, blockState);

            if (!_isBlockDestroy || !blockPosShift.Equals(_blockPos))
            {
                begin = true;
                _blockPos = blockPosShift;
                _isBlockDestroy = true;
            }
            else
            {
                // Если блок тот же самый, то отменяем началку, удары могут быть кликами
                begin = false;
            }

            if (!blockPosShift.Equals(blockPos))
            {
                blockState = _game.World.GetBlockState(_blockPos);
                block = blockState.GetBlock();
            }

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

                if (_destroy >= _hardness)
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
                        _blockPos.ToVector3Center(), .5f, .9f + _game.World.Rnd.NextFloat() * .2f);

                    _game.World.ParticleDiggingBlock(block, _player.MovingObject.RayHit, 5, true);
                }
            }
            else
            {
                // Разрушили
                _BlockDestroyed(block);
            }

            // Инструмент ламаем
            _player.DamageItemTool(1);
        }

        /// <summary>
        /// Разрушили выбранный блок
        /// </summary>
        private void _BlockDestroyed(BlockBase block)
        {
            _game.TrancivePacket(new PacketC07PlayerDigging(_blockPos));
            _game.World.SetBlockToAir(_blockPos);
            _isBlockDestroy = false;
        }
    }
}
