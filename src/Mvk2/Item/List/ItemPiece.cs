using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vge.Entity.Player;
using Vge.Item;
using Vge.Util;
using Vge.World;
using Vge.World.Block;
using WinGL.Util;

namespace Mvk2.Item.List
{
    /// <summary>
    /// Предмет метательный кусочек
    /// </summary>
    public class ItemPiece : ItemBase
    {
        /// <summary>
        /// Действие предмета ЛКМ.
        /// Для клиента.
        /// </summary>
        /// <param name="begin">Первый клик, без паузы</param>
        public override ResultHandAction OnAction(bool begin, ItemStack stack, PlayerClientOwner player)
        {
            if (!player.CreativeMode)
            {
                player.Inventory.DecrCurrentItem(1);
            }
            return new ResultHandAction(ResultHandAction.ActionType.UseItem, 12);
        }

        /// <summary>
        /// Действие текущего предмета.
        /// Для сервера.
        /// </summary>
        public override void OnUseAction(ItemStack stack, PlayerServer player)
        {
            player.DropItem(stack.Copy(1), true, true);
            if (!player.CreativeMode)
            {
                player.Inventory.DecrCurrentItem(1);
            }
        }

        /// <summary>
        /// Вспомогательное действие предмета ПКМ.
        /// Для клиента.
        /// </summary>
        /// <param name="begin">Первый клик, без паузы</param>
        /// /// <param name="counter">Счётчик тактов от нажатия вспомогательное действия ПКМ</param>
        public override ResultHandSecond OnSecond(bool begin, ItemStack stack, 
            PlayerClientOwner player, int counter)
        {
            ResultHandSecond result = base.OnSecond(begin, stack, player, counter);
            if (result.Action == ResultHandSecond.ActionType.None || player.IsKeyShift())
            {
                if (counter > 20) counter = 20;
                // Пауза нужна для анимации
                return new ResultHandSecond(3, counter / 4);
            }
            return result;
        }

        /// <summary>
        /// Окончание вспомогательного действия предмета ПКМ, вызывается после отпущения клавиши или смены предмета.
        /// Для клиента.
        /// Возвращает Дополнительный цифровой параметр, если -1 нет действий
        /// </summary>
        /// <param name="abort">Бало ли астановка из-за отмены</param>
        /// <param name="counter">счётчик тактов от нажатия</param>
        public override int OnSecondEnd(ItemStack stack, PlayerClientOwner player,
            bool abort, int counter)
        {
            if (abort)
            {
                return 255;
            }

            if (!player.CreativeMode)
            {
                player.Inventory.DecrCurrentItem(1);
            }
            Console.WriteLine(counter);
            if (counter > 20) counter = 20;

            return counter / 4 + 100;
        }

        /// <summary>
        /// Вспомогательное действие текущего предмета.
        /// Для сервера
        /// </summary>
        /// <param name="number">Дополнительный цифровой параметр</param>
        public override void OnUseSecond(ItemStack stack, PlayerServer player, int number)
        {
            if (number == 255)
            {
                // Отмена броска
                //Console.WriteLine("Отмена броска");
            }
            else if (number >= 100)
            {
                // Бросок
                number -= 100;
                //Console.WriteLine("Бросок " + number);
                player.DropItem(stack.Copy(1), true, number > 1);
                if (!player.CreativeMode)
                {
                    player.Inventory.DecrCurrentItem(1);
                }
            }
            else
            {
                // Замах
                //Console.WriteLine("Замах " + number);
            }
        }
    }
}
