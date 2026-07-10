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
        /// Импульс отскока при атаке на XZ
        /// </summary>
        private static float _impulsAttackXZ = .9f;
        /// <summary>
        /// Импульс отскока при атаке на Y
        /// </summary>
        private static float _impulsAttackY = .18f;

        private readonly PlayerClientOwner _player;
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
                _secondPrev = _second;
            }
            _actionPrev = _action;
            _counterSecond++;
        }

        

        /// <summary>
        /// Такт действия
        /// </summary>
        protected virtual void _Action(bool begin)
        {
            if (_player.IsSpectator()) return;

            if (begin)
            {
                _pauseAction = 0;
                _blankShotAction = false;
            }

            if (_pauseAction == 0)
            {
                if (!_blankShotAction)
                {
                    // TODO::2026-07-10 AttackRight, можно вынести в HandManagerMvk
                    _player.Render.SetAnimationCodeAdd("AttackRight", 2);
                    // Отправить анимацию
                    _game.TrancivePacket(new PacketC0APlayerAnimation(
                        _player.Render.AnimationCodeAdd, 
                        PacketC0APlayerAnimation.EnumAction.CodeAdd,
                        _player.Render.SpeedAnimationCodeAdd));
                }

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
                        _Attack(moving);
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
                        _blankShotAction = true;
                    }
                    _pauseAction = resultAction.Pause;
                    //Console.Write(_pauseAction + " ");
                }
                else if (moving.IsBlock())
                {
                    if (_player.CanDestroyedBlock(moving.Block.GetBlock()))
                    {
                        // Разрушение блока без предмета
                        _BlockDigging(moving.BlockPosition);
                        //Console.WriteLine("BlockDigging");
                        _pauseAction = 6; //TODO::2026-05-30 Тут надо Интструмент!!! entityPlayer.PauseTimeBetweenBlockDestruction
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
                    _Attack(moving);
                    _pauseAction = -1;
                }
                else
                {
                    // Нет действий без предмета
                    //Console.WriteLine("Нет действий без предмета [null]");
                    _pauseAction = -1;
                    _blankShotAction = true;
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
                            _game.TrancivePacket(new PacketC08PlayerBlockPlacement(moving.BlockPosition,
                                    moving.Side, moving.Facing, resultSecond.Replaceable, true));
                            _pauseSecond = resultSecond.Pause;
                        }
                        else if (resultSecond.Action == ResultHandSecond.ActionType.InteractEntity)
                        {
                            // Взаимодействие на сущность, с предметом
                            //Console.WriteLine("InteractItem");
                            _game.TrancivePacket(new PacketC03UseEntity(moving.Entity.Id,
                                PacketC03UseEntity.EnumAction.Interact));
                            _pauseSecond = resultSecond.Pause;
                        }
                        else if (resultSecond.Action == ResultHandSecond.ActionType.UseItem)
                        {
                            // Взаимодействия предмета, ПКМ без блока
                            _flagBeginUseSecond = true;
                             //Console.WriteLine("UseItem");
                            _game.TrancivePacket(new PacketC05UseItem(resultSecond.Number));
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
                        _blockPos.ToVector3Center(), 1, .9f + _game.World.Rnd.NextFloat() * .2f);

                    _game.World.ParticleDiggingBlock(block, _player.MovingObject.RayHit, 5, true);
                    //if (block.IsParticle)
                    //{
                        //_game.World.SpawnParticle((ushort)EnumParticles.Smoke, 5, _player.MovingObject.RayHit,
                        //    new Vector3(.25f), 1, block.IndexBlock);
                    //}
                }
            }
            else
            {
                // Разрушили
                _BlockDestroyed(block);
            }

            // Инструмент ламаем
            DamageItemTool(1);
        }

        /// <summary>
        /// Разрушили выбранный блок
        /// </summary>
        private void _BlockDestroyed(BlockBase block)
        {
            _game.TrancivePacket(new PacketC07PlayerDigging(_blockPos));
            _game.World.SetBlockToAir(_blockPos, 46);
            _isBlockDestroy = false;

            //if (block.IsParticle)
            //{
            //    _game.World.SpawnParticle(0, 20, _blockPos.ToVector3Center(),
            //        new Vector3(1), 1, block.IndexBlock);
            //}
        }

        /// <summary>
        /// Наносим урон предмету (инструменту)
        /// </summary>
        /// <param name="damage">Сила урона</param>
        /// <param name="isLeft">Левая ли рука</param>
        public void DamageItemTool(int damage, bool isLeft = false)
        {
            if (!_player.CreativeMode)
            {
                ItemStack stack = isLeft ? _player.Inventory.GetCurrentLeftItem()
                    : _player.Inventory.GetCurrentItem(); 

                if (stack != null)
                {
                    damage = stack.DamageItemTool(_player, damage, isLeft);
                    if (damage > 0)
                    {
                        _game.TrancivePacket(new PacketC06DamageItem((byte)damage, isLeft));
                    }
                }
            }
        }
    }
}
