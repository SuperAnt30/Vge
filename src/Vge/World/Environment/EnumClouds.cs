using System;

namespace Vge.World.Environment
{
    /// <summary>
    /// Перечесление облачности
    /// </summary>
    public enum EnumClouds
    {
        /// <summary>
        /// Пасмурно = .31f
        /// </summary>
        Overcast = 5,
        /// <summary>
        /// Сильная облачность = .41f
        /// </summary>
        HeavilyCloudy = 4,
        /// <summary>
        /// Значительная облачность = .51f
        /// </summary>
        MostlyCloudy = 3,
        /// <summary>
        /// Облачно = .61f
        /// </summary>
        Cloudy = 2,
        /// <summary>
        /// Мало облачно = .76f
        /// </summary>
        PartlyCloudy = 1,
        /// <summary>
        /// Ясно = .91f
        /// </summary>
        Clear = 0
    }

    public static class CloudConditionsConvert
    {
        public static readonly int CountEnumClouds = Enum.GetNames(typeof(EnumClouds)).Length;

        public static float FewClouds(EnumClouds enumClouds)
        {
            switch (enumClouds)
            {
                case EnumClouds.Overcast: return .31f; // .0961
                case EnumClouds.HeavilyCloudy: return .41f; // .1681
                case EnumClouds.MostlyCloudy: return .51f; // .2601
                case EnumClouds.Cloudy: return .61f; // .3721
                case EnumClouds.PartlyCloudy: return .76f; // .5776
            }
            return .91f; // .8281
        }
    }
}
