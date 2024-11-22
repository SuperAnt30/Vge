namespace Vge.Util
{
  /**
   *      (North)
   *        0;-1
   *         N
   * (West)  |   (East)
   *   W ----+---- E 
   * -1;0    |    1;0
   *         S
   *        0;1
   *      (South)
   **/
    /// <summary>
    /// Направление вгляда полюсов
    /// </summary>
    public enum Pole
    {
        /// <summary>
        /// Север
        /// </summary>
        North = 4,
        /// <summary>
        /// Юг
        /// </summary>
        South = 5,
        /// <summary>
        /// Запад
        /// </summary>
        West = 3,
        /// <summary>
        /// Восток
        /// </summary>
        East = 2,
        /// <summary>
        /// Вверх
        /// </summary>
        Up = 0,
        /// <summary>
        /// Низ
        /// </summary>
        Down = 1,
        /// <summary>
        /// Все стороны
        /// </summary>
        All = -1
    }

    public sealed class PoleConvert
    {
        public readonly static byte[] Reverse = new byte[] { 1, 0, 3, 2, 5, 4 };

        public static Pole GetPole(string name)
        {
            if (name == "Up") return Pole.Up;
            if (name == "Down") return Pole.Down;
            if (name == "East") return Pole.East;
            if (name == "West") return Pole.West;
            if (name == "North") return Pole.North;
            if (name == "South") return Pole.South;
            return Pole.All;
        }

        /// <summary>
        /// Повернуть стороны по Y
        /// </summary>
        public static int RotateY(int side, int rotate)
        {
            if (rotate != 0)
            {
                if (side == 2) // East Восток
                {
                    side = rotate == 90 ? 4 : rotate == 180 ? 3 : 5;
                }
                else if (side == 3) // West Запад
                {
                    side = rotate == 90 ? 5 : rotate == 180 ? 2 : 4;
                }
                else if (side == 4) // North Север
                {
                    side = rotate == 90 ? 3 : rotate == 180 ? 5 : 2;
                }
                else if (side == 5) // South Юг
                {
                    side = rotate == 90 ? 2 : rotate == 180 ? 4 : 3;
                }
            }
            return side;
        }
    }
}
