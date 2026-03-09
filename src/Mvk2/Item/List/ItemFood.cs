using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Предмет еда
    /// </summary>
    public class ItemFood : ItemBase
    {



        /// <summary>
        /// Вспомогательное действие предмета ПКМ.
        /// Для клиента.
        /// </summary>
        /// <param name="begin">Первый клик, без паузы</param>
        public override ResultHandSecond OnSecond(bool begin, ItemStack stack, 
            PlayerClientOwner player, int counter)
        {
            // шаг потреблении еды в тактах
            int step = 30;
            if (begin)
            {
                // Начинаем кушать и подаём паузу
                return new ResultHandSecond(step, 0);
            }
            // Время прошла мы одну единицу съели, и продолжаем кушать
            // Тут надо уменьшить голод у перса на клиенте
            if (!player.CreativeMode)
            {
                player.Inventory.DecrCurrentItem(1);
            }
            return new ResultHandSecond(step, 2);
        }

        /// <summary>
        /// Окончание вспомогательного действия предмета ПКМ, вызывается после отпущения клавиши или смены предмета.
        /// Для клиента.
        /// Возвращает Дополнительный цифровой параметр, если -1 нет действий
        /// </summary>
        /// <param name="abort">Бало ли астановка из-за отмены</param>
        /// <param name="counter">счётчик тактов от нажатия</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int OnSecondEnd(ItemStack stack, PlayerClientOwner player, 
            bool abort, int counter) => 1;

        /// <summary>
        /// Вспомогательное действие текущего предмета.
        /// Для сервера
        /// </summary>
        /// <param name="number">Дополнительный цифровой параметр</param>
        public override void OnUseSecond(ItemStack stack, PlayerServer player, int number)
        {
            if (number == 0)
            {
                // Персонаж начинает кушать, надо включить анимацию!
                //Console.WriteLine("Кушаем");
            }
            else if (number == 1)
            {
                // Персонаж прервал трапезу, выключить анимацию!
                //Console.WriteLine("Стоп кушать");
            }
            else
            {
                // Тут надо уменьшить голод у перса на сервере
                if (!player.CreativeMode)
                {
                    player.Inventory.DecrCurrentItem(1);
                }
            }
        }
    }
}
