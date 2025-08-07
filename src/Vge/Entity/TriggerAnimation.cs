using System.Runtime.CompilerServices;

namespace Vge.Entity
{
    /// <summary>
    /// Анимационный триггер для сущности
    /// </summary>
    public class TriggerAnimation
    {
        /// <summary>
        /// Было ли изменение
        /// </summary>
        public bool Changed { get; private set; }
        /// <summary>
        /// Состояние когда сущность не стоит на земле, парит в воздухе
        /// </summary>
        public bool Levitate { get; private set; }
        /// <summary>
        /// Состояние когда сущность находится в воде, для состояния плавания
        /// </summary>
        public bool Water { get; private set; }
        /// <summary>
        /// Состояние когда сущность соприкосается с элементом лазить по вертикали
        /// </summary>
        public bool Climb { get; private set; }

        /// <summary>
        /// Тип байт данных общего перемещения
        /// FBLRSnJSp
        /// </summary>
        private byte _movingWork;
        
        /// <summary>
        /// Задать состояние в воздухе
        /// </summary>
        public void SetLevitate(bool value)
        {
            Levitate = value;
            Changed = true;
        }

        /// <summary>
        /// Задать байт флагов анимации движения
        /// FBLRSnSp
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMovingFlags(byte moving)
        {
            _movingWork = moving;
            Changed = true;
        }

        /// <summary>
        /// Выполнено, данные в конце после отправки данных игрового такта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Done() => Changed = false;

        /// <summary>
        /// Очистить, предать в исходное положение
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _movingWork = 0;
            Levitate = Water = Climb = false;
            Changed = true;
        }

        /// <summary>
        /// Полностью без движения, все значение пустые
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsIdleAll() => _movingWork == 0;

        /// <summary>
        /// Было ли перемещение вперёд
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsForward() => _GetFlag(0);
        /// <summary>
        /// Было ли перемещение назад
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBack() => _GetFlag(1);
        /// <summary>
        /// Был ли шаг влево
        /// </summary>  
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsStrafeLeft() => _GetFlag(2);
        /// <summary>
        /// Был ли шаг вправо
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsStrafeRight() => _GetFlag(3);
        /// <summary>
        /// Было ли присесть
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSneak() => _GetFlag(4);
        /// <summary>
        /// Было ли прыжок
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsJump() => _GetFlag(5);
        /// <summary>
        /// Было ли ускорение
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSprinting() => _GetFlag(6);
        /// <summary>
        /// Горизонтальное перемещение
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMove() => (_movingWork & 0xF) != 0;
        /// <summary>
        /// Имеется ли сейчас движение только стрейф, без движения вперёд или назад или бездействия
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsMovingStrafe() => (_movingWork & 3) == 0 && ((_movingWork >> 2) & 3) != 0;

        /// <summary>
        /// Получить значение бита по флагу
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _GetFlag(int flag) => (_movingWork & 1 << flag) != 0;

        public override string ToString() => (IsForward() ? "F" : "")
            + (IsBack() ? "B" : "")
            + (IsStrafeLeft() ? "L" : "")
            + (IsStrafeRight() ? "R" : "")
            + (IsSneak() ? "Sn" : "")
            + (IsSprinting() ? "Sp" : "")
            + (IsJump() ? "J" : "");
    }
}
