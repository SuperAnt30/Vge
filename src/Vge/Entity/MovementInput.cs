using System.Runtime.CompilerServices;
using Vge.Entity.Physics;
using WinGL.Util;

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
        public bool Forward { get; private set; }
        /// <summary>
        /// Перемещение назад
        /// </summary>
        public bool Back { get; private set; }
        /// <summary>
        /// Шаг влево
        /// </summary>
        public bool StrafeLeft { get; private set; }
        /// <summary>
        /// Шаг вправо
        /// </summary>
        public bool StrafeRight { get; private set; }
        /// <summary>
        /// Присесть
        /// </summary>
        public bool Sneak { get; private set; }
        /// <summary>
        /// Прыжок
        /// </summary>
        public bool Jump { get; private set; }
        /// <summary>
        /// Ускорение
        /// </summary>
        public bool Sprinting { get; private set; }
        
        /// <summary>
        /// Было ли изменение по управлению
        /// </summary>
        public bool Changed { get; private set; }

        /// <summary>
        /// Тип байт данных общего перемещения
        /// FBLRSnJSp
        /// </summary>
        public byte Flags { get; private set; }
        /// <summary>
        /// Скорость перемещения в любую сторону кроме вертиалки 0 .. +n
        /// </summary>
        public float MoveSpeed { get; private set; }
        /// <summary>
        /// Скорость перемещения шага влево -n .. +n право
        /// </summary>
        public float MoveStrafe { get; private set; }
        /// <summary>
        /// Скорость перемещения вперёд -n .. +n назад
        /// </summary>
        public float MoveForward { get; private set; }
        /// <summary>
        /// Скорость перемещения вертикали вниз -n .. +n вверх
        /// </summary>
        public float MoveVertical { get; private set; }

        /// <summary>
        /// Коэффициент ускорения вперёд, для мобов
        /// </summary>
        private float _speed = 1;

        /// <summary>
        /// Перегенерировать атрибуты передвежения
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _GenMove()
        {
            MoveForward = (Back ? _speed * Cp.BackSpeed : 0) - (Forward ? _speed : 0);
            MoveStrafe = (StrafeRight ? _speed * Cp.StrafeSpeed : 0) - (StrafeLeft ? _speed * Cp.StrafeSpeed : 0);
            MoveSpeed = Mth.Max(Mth.Abs(MoveStrafe), Mth.Abs(MoveForward));
        }

        /// <summary>
        /// Перегенерировать атрибуты вертикали
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float _GenMoveVertical() => MoveVertical = (Jump ? _speed : 0) - (Sneak ? _speed : 0);

        #region Flags

        /// <summary>
        /// Задать значение бита по флагу
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _SetFlag(int flag, bool value)
        {
            if (value) Flags = (byte)(Flags | 1 << flag);
            else Flags = (byte)(Flags & ~(1 << flag));
        }

        /// <summary>
        /// Задать перемещение вперёд
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _Forward(bool value) => _SetFlag(0, value);
        /// <summary>
        /// Задать перемещение назад
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _Back(bool value) => _SetFlag(1, value);
        /// <summary>
        /// Задать шаг влево
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _StrafeLeft(bool value) => _SetFlag(2, value);
        /// <summary>
        /// Задать шаг вправо
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _StrafeRight(bool value) => _SetFlag(3, value);
        /// <summary>
        /// Задать присесть
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _Sneak(bool value) => _SetFlag(4, value);
        /// <summary>
        /// Задать прыжок
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _Jump(bool value) => _SetFlag(5, value);
        /// <summary>
        /// Задать ускорение
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void _Sprinting(bool value) => _SetFlag(6, value);

        #endregion

        #region Set

        /// <summary>
        /// Задать остановиться
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetStop()
        {
            Forward = Sprinting = Sneak = Jump = Back = StrafeLeft = StrafeRight = false;
            MoveVertical = MoveForward = MoveStrafe = MoveSpeed = 0;
            Flags = 0;
        }

        /// <summary>
        /// Задать перемещение вперёд с заданным ускорением
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetForward(float speed)
        {
            _speed = speed;
            SetForward(true);
        }

        /// <summary>
        /// Изменено перемещение вперёд
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetForward(bool value)
        {
            if (value != Forward)
            {
                Forward = value;
                _Forward(value);

                if (value && Back)
                {
                    // Отмена идти назад если зажаты две клавиши
                    Back = false;
                    _Back(false);
                }
                else if (!value && Sprinting)
                {
                    // Отменить ускорение если оно было
                    Sprinting = false;
                    _Sprinting(false);
                }
                _GenMove();
                Changed = true;
            }
        }

        /// <summary>
        /// Изменено перемещение назад
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBack(bool value)
        {
            if (value != Back)
            {
                Back = value;
                _Back(value);

                if (value && Forward)
                {
                    // Отмена идти вперёд если зажаты две клавиши
                    Forward = false;
                    _Forward(false);
                    if (Sprinting)
                    {
                        // Отменить ускорение если оно было
                        Sprinting = false;
                        _Sprinting(false);
                    }
                }
                _GenMove();
                Changed = true;
            }
        }

        /// <summary>
        /// Изменен шаг влево
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetStrafeLeft(bool value)
        {
            if (value != StrafeLeft)
            {
                StrafeLeft = value;
                _StrafeLeft(value);

                if (value && StrafeRight)
                {
                    // Отмена шага вправо если зажаты две клавиши
                    StrafeRight = false;
                    _StrafeRight(false);
                }
                _GenMove();
                Changed = true;
            }
        }

        /// <summary>
        /// Изменен шаг вправо
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetStrafeRight(bool value)
        {
            if (value != StrafeRight)
            {
                StrafeRight = value;
                _StrafeRight(value);

                if (value && StrafeLeft)
                {
                    // Отмена шага вправо если зажаты две клавиши
                    StrafeLeft = false;
                    _StrafeLeft(false);
                }
                _GenMove();
                Changed = true;
            }
        }

        /// <summary>
        /// Изменен присесть
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSneak(bool value)
        {
            if (value != Sneak)
            {
                Sneak = value;
                _Sneak(value);

                if (value && Sprinting)
                {
                    // Если присели, а было ускорение, отключаем ускорение
                    Sprinting = false;
                    _Sprinting(false);
                }
                _GenMoveVertical();
                Changed = true;
            }
        }

        /// <summary>
        /// Изменен прыжок
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetJump(bool value)
        {
            if (value != Jump)
            {
                Jump = value;
                _Jump(value);
                _GenMoveVertical();
                Changed = true;
            }
        }

        /// <summary>
        /// Изменено ускорение
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetSprinting(bool value)
        {
            if (value != Sprinting)
            {
                // Нельзя ускорится если крадёмся
                if (!Sneak && (value || (!value && !Forward)))
                {
                    Sprinting = value;
                    _Sprinting(value);
                    Changed = true;
                }
            }
        }

        #endregion

        /// <summary>
        /// Обновить данные в после после отправки данных игрового такта
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateAfter() => Changed = false;

        public override string ToString()
        {
            string s = "";
            if (Forward) s += "F" + _speed.ToString("0.0");
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
