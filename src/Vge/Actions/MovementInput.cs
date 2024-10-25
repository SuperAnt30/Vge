namespace Vge.Actions
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
        public bool Left;
        /// <summary>
        /// Шаг вправо
        /// </summary>
        public bool Right;
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
        /// Задать управление
        /// </summary>
        public void SetMoveState(bool forward, bool jump, bool sneak, bool sprinting)
        {
            Back = Left = Right = false;
            Forward = forward;
            Jump = jump;
            Sneak = sneak;
            Sprinting = sprinting;
        }

        /// <summary>
        /// Задать перемещение вперёд с заданным ускорением
        /// </summary>
        public void SetForward(float speed)
        {
            Forward = true;
            Speed = speed;
        }

        /// <summary>
        /// Задать остановиться
        /// </summary>
        public void SetStop() 
            => Forward = Sprinting = Sneak = Jump = Back = Left = Right = false;

        /// <summary>
        /// Перемещение шага влево -1.0 .. +1.0 право
        /// </summary>
        public float GetMoveStrafe() => (Right ? 1f : 0) - (Left ? 1f : 0);

        /// <summary>
        /// Перемещение назад 1.0 .. -1.0 вперёд
        /// </summary>
        public float GetMoveForward() => (Back ? 1f : 0f) - (Forward ? Speed : 0f);

        /// <summary>
        /// Перемещение вертикали вверх 1.0 .. -1.0 вниз
        /// </summary>
        public float GetMoveVertical() => (Jump ? 1f : 0f) - (Sneak ? 1f : 0);

        public override string ToString()
        {
            string s = "";
            if (Forward) s += "F" + Speed.ToString("0.0");
            if (Back) s += "B";
            if (Right) s += "R";
            if (Left) s += "L";
            if (Jump) s += "J";
            if (Sneak) s += "S";
            if (Sprinting) s += "sp";
            return s;
        }
    }
}
