using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vge.Item
{
    /// <summary>
    /// Структура результата вспомогательного действий рук
    /// </summary>
    public struct ResultHandSecond
    {
        /// <summary>
        /// Пауза
        /// </summary>
        public readonly int Pause;
        /// <summary>
        /// Действие 
        /// </summary>
        public readonly ActionType Action;
        /// <summary>
        /// Дополнительный цифровой параметр
        /// </summary>
        public readonly int Number;
        /// <summary>
        /// Может на этот блок поставить другой, к примеру трава
        /// </summary>
        public bool Replaceable;

        /// <summary>
        /// Указать действие и паузу до след действия
        /// </summary>
        public ResultHandSecond(ActionType action, int pause = -1)
        {
            Pause = pause;
            Number = 0;
            Action = action;
            Replaceable = false;
        }

        /// <summary>
        /// Установить паузу для размещение блока
        /// </summary>
        public ResultHandSecond(int pause, bool replaceable)
        {
            Pause = pause;
            Number = 0;
            Action = ActionType.BlockPlacement;
            Replaceable = replaceable;
        }

        /// <summary>
        /// Установить паузу для размещение блока
        /// </summary>
        public ResultHandSecond(int pause, int number)
        {
            Pause = pause;
            Number = number;
            Action = ActionType.UseItem;
            Replaceable = false;
        }

        public enum ActionType
        {
            /// <summary>
            /// Не было ни каких действи
            /// </summary>
            None,
            /// <summary>
            /// Размещение блока (Взаимодействие предмета на выбранный блок)
            /// </summary>
            BlockPlacement,
            /// <summary>
            /// Взаимодействие сущности
            /// </summary>
            InteractEntity,
            /// <summary>
            /// Взаимодействия предмета
            /// </summary>
            UseItem
        }
    }
}
