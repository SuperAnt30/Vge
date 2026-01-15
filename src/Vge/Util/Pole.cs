using System.Runtime.CompilerServices;
using WinGL.Util;

namespace Vge.Util
{
    /**
     *      (North)
     *      (Север)
     *        0;-1 
     *        Yaw 0
     *         N
     * (West)  | (East)
     * (Запад) | (Восток)
     *   W ----+---- E 
     * -1;0    |    1;0
     * Yaw -90 |   Yaw 90
     *         S
     *        0;1 
     *    Yaw 180||-180
     *      (South)
     *       (Юг)
     **/
    /// <summary>
    /// Направление вгляда полюсов
    /// </summary>
    public enum Pole
    {
        /// <summary>
        /// Север 4 || 8
        /// </summary>
        North = 4,
        /// <summary>
        /// Юг 5 || 16
        /// </summary>
        South = 5,
        /// <summary>
        /// Запад 3 || 4
        /// </summary>
        West = 3,
        /// <summary>
        /// Восток 2 
        /// </summary>
        East = 2,
        /// <summary>
        /// Вверх 0
        /// </summary>
        Up = 0,
        /// <summary>
        /// Низ 1
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

        public readonly static byte[] Flag = new byte[] { 0, 1, 2, 4, 8, 16 };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        /// Повернуть стороны по X
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RotateX(int side, int rotate)
        {
            if (rotate != 0)
            {
                if (side == 0) // Up Вверх
                {
                    side = rotate == 90 ? 5 : rotate == 180 ? 1 : 4;
                }
                else if (side == 1) // Down Низ
                {
                    side = rotate == 90 ? 4 : rotate == 180 ? 0 : 5;
                }
                else if (side == 4) // North Север
                {
                    side = rotate == 90 ? 0 : rotate == 180 ? 5 : 1;
                }
                else if (side == 5) // South Юг
                {
                    side = rotate == 90 ? 1 : rotate == 180 ? 4 : 0;
                }
            }
            return side;
        }

        /// <summary>
        /// Повернуть стороны по Y
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        /// <summary>
        /// Получите облицовку, соответствующую заданному углу (0-360). Угол 0 - SOUTH, угол 90 - WEST.
        /// </summary>
        /// <param name="angle">угол в радианах</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Pole FromAngle(float angle)
        {
            if (angle >= -Glm.Pi45 && angle <= Glm.Pi45) return Pole.North;
            else if (angle > Glm.Pi45 && angle < Glm.Pi135) return Pole.East;
            else if (angle < -Glm.Pi45 && angle > -Glm.Pi135) return Pole.West;
            return Pole.South;
        }
    }
}
