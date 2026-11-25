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
        /// Взаимодействие блока с жидкостью
        /// </summary>
        public bool IsLiquid;
        /// <summary>
        /// Пауза задержки до взаимодейстыия, для анимации
        /// </summary>
        public int UseDelay;
        /// <summary>
        /// Используем OnSecondEnd
        /// </summary>
        public bool UseEnd;
        /// <summary>
        /// Принудительная анимация, даже если сработал отказ
        /// </summary>
        public bool Animation;

        /// <summary>
        /// Указать действие и паузу до след действия
        /// </summary>
        public ResultHandSecond(ActionType action, int pause = -1)
        {
            Pause = pause;
            Number = 0;
            Action = action;
            Replaceable = false;
            IsLiquid = false;
            UseEnd = false;
            UseDelay = -1;
            Animation = false;
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
            IsLiquid = false;
            UseEnd = false;
            UseDelay = -1;
            Animation = false;
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
            IsLiquid = false;
            UseEnd = false;
            UseDelay = -1;
            Animation = false;
        }

        /// <summary>
        /// Указать действие и паузу до след действия
        /// </summary>
        public ResultHandSecond(ActionType action, bool isLiquid)
        {
            Pause = -1;
            Number = 0;
            Action = action;
            Replaceable = true;
            IsLiquid = isLiquid;
            UseEnd = false;
            UseDelay = -1;
            Animation = false;
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
