using System.Runtime.CompilerServices;

namespace Vge.Entity
{
    /// <summary>
    /// Анимационный триггер для сущности
    /// </summary>
    public class TriggerAnimation
    {
        /// <summary>
        /// Прыжок
        /// </summary>
        private bool _jump;
        /// <summary>
        /// Тип байт данных общего перемещения
        /// FBLRSnSp
        /// </summary>
        private byte _moving;
        /// <summary>
        /// Предыдущее значение
        /// </summary>
        private byte _movingPrev;

        /// <summary>
        /// Получить байт данных общего перемещения
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetMoving() => _moving;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetAnimation(byte moving) => _moving = moving;

        /// <summary>
        /// Подготовить удаление
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PrepareRemoved() => _moving = (byte)(_movingPrev & ~_moving);

        /// <summary>
        /// Подготовить Добавление
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PrepareAddendum() => _moving = (byte)(_moving & ~_movingPrev);

        /// <summary>
        /// Было ли изменение с предыдущим значением
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsChanged() => _moving != _movingPrev;

        /// <summary>
        /// Обновить данные в конце после отправки данных игрового такта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdatePrev()
        {
            _movingPrev = _moving;
            _jump = false;
        }

        /// <summary>
        /// Очистить, предать в исходное положение
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _moving = 0;
            _jump = false;
        }

        /// <summary>
        /// Задать прыжок
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Jump() => _jump = true;
        /// <summary>
        /// Было ли прыжок
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsJump() => _jump;

        /// <summary>
        /// Задать перемещение вперёд
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Forward(bool value) => _SetFlag(0, value);
        /// <summary>
        /// Задать перемещение назад
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Back(bool value) => _SetFlag(1, value);
        /// <summary>
        /// Задать шаг влево
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StrafeLeft(bool value) => _SetFlag(2, value);
        /// <summary>
        /// Задать шаг вправо
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StrafeRight(bool value) => _SetFlag(3, value);
        /// <summary>
        /// Задать присесть
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sneak(bool value) => _SetFlag(4, value);
        /// <summary>
        /// Задать ускорение
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Sprinting(bool value) => _SetFlag(5, value);

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
        /// Было ли ускорение
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsSprinting() => _GetFlag(5);

        /// <summary>
        /// Задать значение бита по флагу
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _SetFlag(int flag, bool value)
        {
            if (value) _moving = (byte)(_moving | 1 << flag);
            else _moving = (byte)(_moving & ~(1 << flag));
        }

        /// <summary>
        /// Получить значение бита по флагу
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool _GetFlag(int flag) => (_moving & 1 << flag) != 0;

        public override string ToString() => (IsForward() ? "F" : "")
            + (IsBack() ? "B" : "")
            + (IsStrafeLeft() ? "L" : "")
            + (IsStrafeRight() ? "R" : "")
            + (IsSneak() ? "Sn" : "")
            + (IsSprinting() ? "Sp" : "")
            + (_jump ? "Jump" : "");
    }
}
