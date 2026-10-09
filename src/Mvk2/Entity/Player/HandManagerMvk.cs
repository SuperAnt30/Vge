using Mvk2.Entity.List;
using Mvk2.Item;
using Vge.Entity.Player;
using Vge.Games;
using Vge.Item;
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

        public HandManagerMvk(GameBase game, PlayerClientOwnerMvk player)
            : base(game, player)
        {
            _playerMvk = player;
        }

        /// <summary>
        /// Действие анимации правой руки
        /// </summary>
        /// <param name="itemStack">Предмет которым делаю действие</param>
        /// <param name="moving">Выбранный объект</param>
        protected override bool _ActionAnimationRight(ItemStack itemStack, 
            MovingObjectPosition moving)
        {
            if (itemStack == null)
            {
               // return false;
                _player.Render.SetAnimationCodeAdd("AttackRight", 3);
            }
            else
            {
                if (itemStack.Item.IndexItem == ItemsRegMvk.AxeIron.IndexItem)
                {
                    ItemStack itemStackLeft = _playerMvk.InvPlayer.GetCurrentLeftItem();
                    if (itemStackLeft != null 
                        && itemStackLeft.Item.IndexItem == ItemsRegMvk.AxeIron.IndexItem)
                    {
                        _player.Render.SetAnimationCodeAdd("ActionTwo", 4);
                        return true;
                    }
                }
                _player.Render.SetAnimationCodeAdd("AttackRight", 2);
                //_player.Render.SetAnimationCodeAdd("ActionTwo", 4);
            }
            return true;
        }
    }
}
