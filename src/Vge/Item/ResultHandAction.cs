using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vge.Item
{
    /// <summary>
    /// Структура результата действий рук
    /// </summary>
    public struct ResultHandAction
    {
        /// <summary>
        /// Пауза
        /// </summary>
        public readonly int Pause;
        /// <summary>
        /// Ускорение разрушении блока, по умолчанию 1, при 0 ломает за один удар.
        /// </summary>
        public readonly float Acceleration;
        /// <summary>
        /// Действие 
        /// </summary>
        public readonly ActionType Action;
        /// <summary>
        /// Может на этот блок поставить другой, к примеру трава
        /// </summary>
        public bool Replaceable;

        /// <summary>
        /// Указать действие и паузу до след действия
        /// </summary>
        public ResultHandAction(ActionType action, int pause = -1)
        {
            Pause = pause;
            Acceleration = 0;
            Action = action;
            Replaceable = false;
        }

        /// <summary>
        /// Установить паузу для разрушения блока
        /// </summary>
        public ResultHandAction(int pause, float acceleration)
        {
            Pause = pause;
            Acceleration = acceleration;
            Action = ActionType.DestroyBlock;
            Replaceable = false;
        }

        /// <summary>
        /// Установить паузу для взаимодействии с блоком
        /// </summary>
        public ResultHandAction(int pause, bool replaceable)
        {
            Pause = pause;
            Acceleration = 0;
            Action = ActionType.ItemOnBlock;
            Replaceable = replaceable;
        }

        public enum ActionType
        {
            /// <summary>
            /// Не было ни каких действи
            /// </summary>
            None,
            /// <summary>
            /// Разрушение блока
            /// </summary>
            DestroyBlock,
            /// <summary>
            /// Атака сущности
            /// </summary>
            AttackEntity,
            /// <summary>
            /// Взаимодействие предмета на выбранный блок
            /// </summary> 
            ItemOnBlock,
            /// <summary>
            /// Взаимодействия предмета
            /// </summary>
            UseItem
        }
    }
}
