using System.Runtime.CompilerServices;

namespace Vge.Entity
{
    /// <summary>
    /// Объект параметров перемещения
    /// </summary>
    public class MovementInput
    {
        /// <summary>
        /// Перемещение вперёд
        /// </summary>
        public bool Forward;
        /// <summary>
        /// Перемещение назад
        /// </summary>
        public bool Back;
        /// <summary>
        /// Шаг влево
        /// </summary>
        public bool StrafeLeft;
        /// <summary>
        /// Шаг вправо
        /// </summary>
        public bool StrafeRight;
        /// <summary>
        /// Прыжок
        /// </summary>
        public bool Jump;
        /// <summary>
        /// Присесть
        /// </summary>
        public bool Sneak;
        /// <summary>
        /// Ускорение
        /// </summary>
        public bool Sprinting;
        /// <summary>
        /// Коэффициент ускорения вперёд, для мобов
        /// </summary>
        public float Speed = 1;

        /// <summary>
        /// Изменение в данных общего перемещения
        /// </summary>
        private bool _movingChanged;
        /// <summary>
        /// Прыжок прошлого такта
        /// </summary>
        private bool _jumpPrev;
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
        /// Задать управление
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMoveState(bool forward, bool jump, bool sneak, bool sprinting)
        {
            Back = StrafeLeft = StrafeRight = false;
            Forward = forward;
            Jump = jump;
            Sneak = sneak;
            Sprinting = sprinting;
        }

        /// <summary>
        /// Задать перемещение вперёд с заданным ускорением
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetForward(float speed)
        {
            Forward = true;
            Speed = speed;
        }

        /// <summary>
        /// Задать остановиться
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetStop()
        {
            Forward = Sprinting = Sneak = Jump = Back = StrafeLeft = StrafeRight = false;
            _moving = 0;
        }

        /// <summary>
        /// Перемещение шага влево -1.0 .. +1.0 право
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetMoveStrafe() => (StrafeRight ? 1 : 0) - (StrafeLeft ? 1 : 0);

        /// <summary>
        /// Перемещение назад 1.0 .. -1.0 вперёд
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetMoveForward() => (Back ? 1 : 0) - (Forward ? Speed : 0);

        /// <summary>
        /// Перемещение вертикали вверх 1.0 .. -1.0 вниз
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetMoveVertical() => (Jump ? 1 : 0) - (Sneak ? 1 : 0);

        #region AnimationTrigger

        /// <summary>
        /// Изменение в данных общего перемещения
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void MovingChanged() => _movingChanged = true;

        /// <summary>
        /// Получить байт данных общего перемещения
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetMoving() => _moving;

        /// <summary>
        /// Подготовить данные для различия и вернуть было ли отличие от прошлого такта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PrepareDataForDistinction()
        {
            if (_movingChanged)
            {
                _moving = 0;
                if (Forward) _moving += 1;
                if (Back) _moving += 2;
                if (StrafeLeft) _moving += 4;
                if (StrafeRight) _moving += 8;
                if (Sneak) _moving += 16;
                if (Sprinting) _moving += 32;
                _movingChanged = false;
                return _moving != _movingPrev || Jump && !_jumpPrev;
            }

            return false;
        }

        /// <summary>
        /// Обновить данные в после после отправки данных игрового такта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateAfter()
        {
            _movingPrev = _moving;
            _jumpPrev = Jump;
        }

        /// <summary>
        /// Было ли прыжок в этом такте
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsJump() => Jump && !_jumpPrev;

        #endregion

        public override string ToString()
        {
            string s = "";
            if (Forward) s += "F" + Speed.ToString("0.0");
            if (Back) s += "B";
            if (StrafeRight) s += "R";
            if (StrafeLeft) s += "L";
            if (Jump) s += "J";
            if (Sneak) s += "S";
            if (Sprinting) s += "sp";
            return s;
        }
    }
}
