using Mvk2.Entity.List;
using Mvk2.Item.List.Tools;
using System;
using System.Runtime.CompilerServices;
using Vge.Entity.Player;
using Vge.Games;
using Vge.Item;
using Vge.Network.Packets.Client;
using Vge.Util;

namespace Mvk2.Entity.Player
{
    /// <summary>
    /// Объект менеджер действий рук игрока. Левой и правой клавишей мыши.
    /// В прошлом было объект ItemInWorldManager, что-то подобное, но там именно очередь ударов
    /// </summary>
    public class HandManagerMvk : HandManager
    {
        private readonly PlayerClientOwnerMvk _playerMvk;

        /// <summary>
        /// Клип взмаха с паузой
        /// </summary>
        private string _clipSwing = "";

        public HandManagerMvk(GameBase game, PlayerClientOwnerMvk player)
            : base(game, player)
        {
            _playerMvk = player;
        }

        /// <summary>
        /// Активное действие анимации правой руки
        /// </summary>
        /// <param name="itemStack">Предмет которым делаю действие</param>
        protected override void _ActionAnimationRight(ItemStack itemStack)
        {
            if (itemStack == null)
            {
                _player.Render.SetAnimationCodeAdd("AttackRight");
            }
            else if (_CheckAxis(itemStack))
            {
                _player.Render.SetAnimationCodeAdd("ActionAxes", itemStack.Item.HitSpeed);
            }
            else if (itemStack.Item.Action != "")
            {
                _player.Render.SetAnimationCodeAdd(itemStack.Item.Action, itemStack.Item.HitSpeed);
            }
            else
            {
                _player.Render.SetAnimationCodeAdd("ActionAxe", itemStack.Item.HitSpeed);
            }

            _SendPlayerAnimation();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _CheckAxis(ItemStack itemStack)
        {
            if (itemStack.Item is ItemAxe itemAxe)
            {
                ItemStack itemStackLeft = _playerMvk.InvPlayer.GetCurrentLeftItem();
                return itemStackLeft != null && itemStackLeft.Item is ItemAxe;
            }
            return false;
        }

        /// <summary>
        /// Вспомогательное действие анимации правой руки
        /// </summary>
        /// <param name="itemStack">Предмет которым делаю действие</param>
        protected override void _SecondAnimationRight(ItemStack itemStack)
        {
            if (itemStack == null)
            {
                _player.Render.SetAnimationCodeAdd("AttackRight");
            }
            else if (_CheckAxis(itemStack))
            {
                _clipSwing = "HoldTwo";
                _player.Render.SetAnimationCodeAdd(_clipSwing, itemStack.Item.HitSpeed);
            }
            else if (itemStack.Item.Action != "")
            {
                _player.Render.SetAnimationCodeAdd(itemStack.Item.Action, itemStack.Item.HitSpeed);
            }
            else
            {
                _player.Render.SetAnimationCodeAdd("ActionAxe", itemStack.Item.HitSpeed);
            }

            _SendPlayerAnimation();
        }

        /// <summary>
        /// Вспомогательное действие анимации правой руки в момент окончании
        /// </summary>
        /// <param name="itemStack">Предмет которым делаю действие</param>
        protected override void _SecondAnimationRightEnd(ItemStack itemStack)
        {
            if (_clipSwing != "")
            {
                _player.Render.StopingClip(_clipSwing);
                _SendPlayerAnimationStoping();
                _clipSwing = "";

                _player.Render.SetAnimationCodeAdd("AttackRight");
                _SendPlayerAnimation();
            }
        }

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
        /// Отправить остановку анимации на сервер
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _SendPlayerAnimationStoping()
        {
            _game.TrancivePacket(new PacketC0APlayerAnimation(
                _clipSwing, PacketC0APlayerAnimation.EnumAction.Stoping));
        }

    }
}
