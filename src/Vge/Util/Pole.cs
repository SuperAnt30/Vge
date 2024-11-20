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
    }
}
