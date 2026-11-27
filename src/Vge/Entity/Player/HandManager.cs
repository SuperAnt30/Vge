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
        public static int PauseNotItem = 5;
        /// <summary>
        /// Пауза для действия руки или предмета
        /// </summary>
        public static int PauseUseItem = 4;
        /// <summary>
        /// Задержка активного действия (ЛКМ), в тактах (момент удара в анимации руки)
        /// </summary>
        public static int ActionDelay = 1;
        /// <summary>
        /// Задержка вспомогательного действия (ПКМ), в тактах (момент взаимодействия в анимации руки)
        /// </summary>
        public static int SecondDelay = 1;

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
        protected readonly GameBase _game;

        /// <summary>
        /// Пауза для активного действия (ЛКМ)
        /// </summary>
        private int _pauseAction;
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
        /// Флаг запуска концовки вспомогательного предмета
        /// </summary>
        private bool _flagUseSecondEnd;

        private struct Pending
        {
            /// <summary>
            /// Сколько тактов осталось до удара
            /// </summary>
            public int Delay;
            /// <summary>
            /// Индекс предмета, которым замахнулись. Если сменили — удар отменяется
            /// </summary>
            public readonly int ItemIndexStack;
            /// <summary>
            /// Начальный ли клик
            /// </summary>
            public readonly bool Begin;

            public Pending(int delay, int itemIndexStack, bool begin)
            {
                Delay = delay;
                ItemIndexStack = itemIndexStack;
                Begin = begin;
            }
        }

        #region Отложенное активное действие ЛКМ

        /// <summary>
        /// Тип отложенного действия ЛКМ
        /// </summary>
        private enum PendingKindAction
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
        private PendingKindAction _pendingKindAction = PendingKindAction.None;
        /// <summary>
        /// В ожидании для ЛКМ
        /// </summary>
        private Pending _pendingAction;
        /// <summary>
        /// Можно ли прервать выполнение, если анимация ещё не закончена.
        /// Нужен для прерывания последующих разрушений блоков при зажатой ЛКМ
        /// </summary>
        private bool _isStopAction = false;

        #endregion

        #region Отложенный вспомогательно действие ПКМ
        
        /// <summary>
        /// Тип отложенного действия ПКМ
        /// </summary>
        private enum PendingKindSecond
        {
            /// <summary>
            /// Нет ожидания
            /// </summary>
            None,
            /// <summary>
            /// Действие предмета (результат OnSecond)
            /// </summary>
            Item,
            /// <summary>
            /// Взаимодействие блока без предмета
            /// </summary>
            UseHandBlock,
            /// <summary>
            /// Взаимодействие сущности без предмета
            /// </summary>
            UseHandEntity
        }

        /// <summary>
        /// Какое действие ждёт момента удара в анимации
        /// </summary>
        private PendingKindSecond _pendingKindSecond = PendingKindSecond.None;
        /// <summary>
        /// В ожидании для ПКМ
        /// </summary>
        private Pending _pendingSecond;

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
            _pendingKindAction = PendingKindAction.None;
            _pendingKindSecond = PendingKindSecond.None;
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
            if (_pendingKindAction != PendingKindAction.None)
            {
                if (_pendingAction.Delay > 0)
                {
                    _pendingAction.Delay--;
                    if (_pendingAction.Delay == 0)
                    {
                        _ExecutePendingAction();
                    }
                }
            }
            if (_pendingKindSecond != PendingKindSecond.None)
            {
                if (_pendingSecond.Delay > 0)
                {
                    _pendingSecond.Delay--;
                    if (_pendingSecond.Delay == 0)
                    {
                        _ExecutePendingSecond();
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
            }

            if (_pauseAction > 0) _pauseAction--;
            if (_pauseSecond > 0) _pauseSecond--;

            _actionPrev = _action;
            _secondPrev = _second;
            _counterSecond++;
        }

        /// <summary>
        /// Активное действие анимации правой руки
        /// </summary>
        /// <param name="itemStack">Предмет которым делаю действие</param>
        protected virtual void _ActionAnimationRight(ItemStack itemStack) { }

        /// <summary>
        /// Вспомогательное действие анимации правой руки
        /// </summary>
        /// <param name="itemStack">Предмет которым делаю действие</param>
        protected virtual void _SecondAnimationRight(ItemStack itemStack) { }

        /// <summary>
        /// Вспомогательное действие анимации правой руки в момент окончании
        /// </summary>
        protected virtual void _SecondAnimationRightEnd() { }

        /// <summary>
        /// Получить индекс предмета, если предмета нет, вернёт -1
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int _GetItemIndex(ItemStack itemStack)
        {
            if (itemStack != null) return itemStack.GetItemIndex();
            return -1;
        }

        /// <summary>
        /// Такт действия
        /// Здесь только принимается решение и запускается анимация,
        /// само действие выполняется в момент удара, в _ExecutePendingAction
        /// </summary>
        protected virtual void _Action(bool begin)
        {
            if (_player.IsSpectator()) return;

            // Новый замах только после паузы и когда прошлый удар уже состоялся
            if (_pauseAction == 0 && _pendingKindAction == PendingKindAction.None)
            {
                //Console.WriteLine("_Action " + begin);
                MovingObjectPosition moving = _player.MovingObject;
                // Стак предмета в руке
                ItemStack itemStack = _player.Inventory.GetCurrentItem();

                _isStopAction = false;

                PendingKindAction kind = PendingKindAction.None;
                int delay = 0;
                ResultHandAction resultAction = default;

                if (itemStack != null)
                {
                    resultAction = itemStack.Item.OnAction(true, begin, itemStack, _player);
                    if (resultAction.Action == ResultHandAction.ActionType.None)
                    {
                        // Нет действий с предметом
                        //Console.WriteLine("Нет действий c предметом");
                        _pauseAction = 0;
                    }
                    else
                    {
                        if (resultAction.Action == ResultHandAction.ActionType.DestroyBlock)
                        {
                            // Разрушение блока
                            if (!begin) _isStopAction = true;
                        }
                        
                        
                        kind = PendingKindAction.Item;
                        // Момент удара в анимации предмета, в тактах
                        delay = resultAction.HitDelay;
                        _pauseAction = resultAction.Pause;
                        if (_pauseAction == -1) _pauseAction = PauseNotItem;
                        //Console.WriteLine("OnAction " + resultAction.Action + " begin:" + begin + " d:" + delay);
                    }
                }
                else if (moving.IsBlock())
                {
                    if (_player.CanDestroyedBlock(moving.Block.GetBlock()))
                    {
                        // Разрушение блока без предмета
                        if (!begin) _isStopAction = true;
                        kind = PendingKindAction.DigHand;
                        delay = ActionDelay;
                        _pauseAction = PauseNotItem;
                        //Console.WriteLine("BlockDigging");
                    }
                    else
                    {
                        //Console.WriteLine("Нет действий без предмета [block]");
                        _pauseAction = 0;
                    }
                }
                else if (moving.IsEntity())
                {
                    // Атака на сущность, без предмета
                    kind = PendingKindAction.AttackHand;
                    delay = ActionDelay;
                    _pauseAction = PauseNotItem;
                }
                else
                {
                    // Нет действий без предмета
                    //Console.WriteLine("Нет действий без предмета [null]");
                    _pauseAction = 0;
                }

                if (kind != PendingKindAction.None)
                {
                    //Console.WriteLine("Action kind");
                    _ActionAnimationRight(itemStack); // Action kind

                    _pendingKindAction = kind;
                    _pendingAction = new Pending(delay, _GetItemIndex(itemStack), begin);

                    if (delay <= 0)
                    {
                        _ExecutePendingAction();
                    }
                }
                else if (begin)
                {
                    //Console.WriteLine("Action begin");
                    _ActionAnimationRight(itemStack); // Action begin
                }
                //Console.WriteLine(_pauseAction);
            }
        }

        /// <summary>
        /// Момент удара в анимации: выполняем действие по тому, на что смотрим сейчас
        /// </summary>
        private void _ExecutePendingAction()
        {
            PendingKindAction kind = _pendingKindAction;
            _pendingKindAction = PendingKindAction.None;

            if (_player.IsSpectator()) return;

            // Стак предмета в руке
            ItemStack itemStack = _player.Inventory.GetCurrentItem();
            // Сменили предмет во время замаха — удар отменяется
            if (_GetItemIndex(itemStack) != _pendingAction.ItemIndexStack) return;
            // Цель берём на момент удара: за время замаха игрок мог отвернуться.
            MovingObjectPosition moving = _player.MovingObject;

            // Если была смена объекта
            //if (!moving.Equals(_pendingAction.Moving)) return;

            switch (kind)
            {
                case PendingKindAction.Item:
                    ResultHandAction resultAction = itemStack.Item.OnAction(false, _pendingAction.Begin, itemStack, _player);
                    switch (resultAction.Action)
                    {
                        case ResultHandAction.ActionType.DestroyBlock:
                            // Разрушение блока с предметом
                            if (moving.IsBlock())
                            {
                                //Console.WriteLine("BlockDiggingItem2");
                                _BlockDigging(moving.BlockPosition, resultAction.Acceleration);
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
                                    moving.Side, moving.Facing, resultAction.Replaceable, true));
                            }
                            break;
                        case ResultHandAction.ActionType.UseItem:
                            // Взаимодействия предмета, ЛКМ без блока
                            //Console.WriteLine("UseItem2");
                            _game.TrancivePacket(new PacketC05UseItem());
                            break;
                    }
                    break;

                case PendingKindAction.DigHand:
                    // Разрушение блока без предмета
                    if (moving.IsBlock() && _player.CanDestroyedBlock(moving.Block.GetBlock()))
                    {
                        //Console.WriteLine("BlockDigging2");
                        _BlockDigging(moving.BlockPosition);
                    }
                    break;

                case PendingKindAction.AttackHand:
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
        protected virtual void _ActionEnd()
        {
            //Console.WriteLine("--");
            if (_isStopAction)
            {
                // Замах прерван
                _pendingKindAction = PendingKindAction.None;
            }
        }

        /// <summary>
        /// Такт вспомогательного действия 
        /// </summary>
        protected virtual void _Second(bool begin)
        {
            if (_player.IsSpectator()) return;

            if (begin && _pauseSecond < 0) _pauseSecond = 0;

            // Новый замах только после паузы и когда прошлый юз уже состоялся
            if (_pauseSecond == 0 && _pendingKindSecond == PendingKindSecond.None)
            {
                //Console.WriteLine("_Second " + begin);
                MovingObjectPosition moving = _player.MovingObject;
                // Стак предмета в руке
                ItemStack itemStack = _player.Inventory.GetCurrentItem();

                PendingKindSecond kind = PendingKindSecond.None;
                int delay = SecondDelay;
                ResultHandSecond resultSecond = default;
                _flagUseSecondEnd = false;
                if (!_player.IsKeyShift() && moving.IsBlock() && moving.Block.GetBlock().OnBlockActivated(
                    _player, moving.BlockPosition, moving.Side, moving.Facing))
                {
                    // Активация блока
                    kind = PendingKindSecond.UseHandBlock;
                    _pauseSecond = -1;
                }
                else if (itemStack != null)
                {
                    resultSecond = itemStack.Item.OnSecond(true, begin, 
                        itemStack, _player, _counterSecond);
                    if (resultSecond.Action == ResultHandSecond.ActionType.None)
                    {
                        // Нет вспомогательного действий с предметом
                        //Console.WriteLine("Нет вспомогательного действий с предметом");
                        _pauseSecond = resultSecond.Pause == -1 ? PauseUseItem : resultSecond.Pause;
                    }
                    else
                    {
                        kind = PendingKindSecond.Item;

                        _flagUseSecondEnd = resultSecond.UseEnd;
                        // Момент удара в анимации предмета, в тактах
                        if (resultSecond.UseDelay != -1) delay = resultSecond.UseDelay;
                        _pauseSecond = resultSecond.Pause;
                        //Console.WriteLine("OnSecond " + resultSecond.Action + " P:" + _pauseSecond + " d:" + delay);
                    }
                }
                else if (moving.IsBlock())
                {
                    // Клик ПКМ на блок без предмета OnBlockActivated
                    kind = PendingKindSecond.UseHandBlock;
                    _pauseSecond = -1;
                    //Console.WriteLine("Click RightMouse");
                }
                else if (moving.IsEntity())
                {
                    // Взаимодействие на сущность, без предмета
                    kind = PendingKindSecond.UseHandEntity;
                    _pauseSecond = -1;
                    //Console.WriteLine("Interact");
                }
                else
                {
                    // Нет вспомогательного действий без предмета
                    _pauseSecond = -1;
                    //Console.WriteLine("Нет вспомогательного действий без предмета!!!");
                }

                if (kind != PendingKindSecond.None)
                {
                    //Console.WriteLine("Second kind");
                    _SecondAnimationRight(itemStack); // Second kind

                    _pendingKindSecond = kind;
                    _pendingSecond = new Pending(delay, _GetItemIndex(itemStack), begin);

                    if (delay <= 0)
                    {
                        _ExecutePendingSecond();
                    }
                }
                else if (begin || resultSecond.Animation)
                {
                    //Console.WriteLine("Second begin");
                    _SecondAnimationRight(itemStack); // Second begin
                }
            }
        }

        /// <summary>
        /// Момент взаимодействия в анимации: выполняем действие по тому, на что смотрим сейчас
        /// </summary>
        private void _ExecutePendingSecond()
        {
            PendingKindSecond kind = _pendingKindSecond;
            _pendingKindSecond = PendingKindSecond.None;

            if (_player.IsSpectator()) return;

            // Стак предмета в руке
            ItemStack itemStack = _player.Inventory.GetCurrentItem();
            // Сменили предмет во время замаха — удар отменяется
            if (_GetItemIndex(itemStack) != _pendingSecond.ItemIndexStack) return;

            // Цель берём на момент удара: за время замаха игрок мог отвернуться.
            // Если цели нужного типа нет — удар мимо
            MovingObjectPosition moving = _player.MovingObject;

            // Если была смена объекта
            //if (!moving.Equals(_pendingSecond.Moving)) return;

            switch (kind)
            {
                case PendingKindSecond.Item:

                    ResultHandSecond resultSecond = itemStack.Item.OnSecond(false, _pendingSecond.Begin, 
                        itemStack, _player, _counterSecond);
                    switch (resultSecond.Action)
                    {
                        case ResultHandSecond.ActionType.BlockPlacement:
                            // Размещение блока (Взаимодействие предмета на выбранный блок)
                            if (moving.IsBlock())
                            {
                                //Console.WriteLine("BlockPlacement2");
                                _game.TrancivePacket(new PacketC08PlayerBlockPlacement(
                                    resultSecond.IsLiquid 
                                        ? moving.BlockLiquidPosition : moving.BlockPosition,
                               moving.Side, moving.Facing, resultSecond.Replaceable, true));
                            }
                            break;
                        case ResultHandSecond.ActionType.InteractEntity:
                            // Взаимодействие на сущность, с предметом
                            if (moving.IsEntity())
                            {
                               // Console.WriteLine("InteractItem2");
                                _game.TrancivePacket(new PacketC03UseEntity(moving.Entity.Id,
                                    PacketC03UseEntity.EnumAction.Interact));
                            }
                            break;
                        case ResultHandSecond.ActionType.UseItem:
                            // Взаимодействия предмета, ПКМ без блока
                            //Console.WriteLine("UseItem2");
                            _game.TrancivePacket(new PacketC05UseItem(resultSecond.Number));
                            break;
                    }
                    break;

                case PendingKindSecond.UseHandBlock:
                    // Клик ПКМ на блок без предмета
                    if (moving.IsBlock())
                    {
                        //Console.WriteLine("Click RightMouse2");
                        //OnBlockActivated
                        _game.TrancivePacket(new PacketC08PlayerBlockPlacement(moving.BlockPosition,
                            moving.Side, moving.Facing, false, false));
                    }
                    break;

                case PendingKindSecond.UseHandEntity:
                    // Взаимодействие на сущность, без предмета
                    if (moving.IsEntity())
                    {
                        //Console.WriteLine("Interact2");
                        _game.TrancivePacket(new PacketC03UseEntity(moving.Entity.Id,
                            PacketC03UseEntity.EnumAction.Interact));
                    }
                    break;
            }
        }

        /// <summary>
        /// Конец вспомогательного действия
        /// </summary>
        protected virtual void _SecondEnd()
        {
            if (_player.IsSpectator()) return;

            // Если замах ещё не состоялся — выполняем его сейчас, чтобы начало ушло раньше конца
            if (_pendingKindSecond != PendingKindSecond.None) //_ExecutePendingSecond();
            {
                _pendingKindSecond = PendingKindSecond.None;
            }
            else if (_flagUseSecondEnd)
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
                    //Console.WriteLine("Second kind");
                }
            }
            _SecondAnimationRightEnd(); // Second End
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
                    _game.World.PlaySound(block.Material.SampleBreak(_game.World.Rnd),
                        _blockPos.ToVector3Center(), .5f, .9f + _game.World.Rnd.NextFloat() * .2f);

                    _game.World.ParticleDiggingBlock(block, _player.MovingObject.RayHit, 16, true);
                }
            }
            else
            {
                // Разрушили
                _BlockDestroyed();
            }

            // Инструмент ламаем
            _player.DamageItemTool(1);
        }

        /// <summary>
        /// Разрушили выбранный блок
        /// </summary>
        private void _BlockDestroyed()
        {
            _game.TrancivePacket(new PacketC07PlayerDigging(_blockPos));
            _game.World.SetBlockToAir(_blockPos);
            _isBlockDestroy = false;
        }
    }
}
