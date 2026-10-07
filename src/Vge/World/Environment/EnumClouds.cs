using System;

namespace Vge.World.Environment
{
    /// <summary>
    /// Перечесление облачности
    /// </summary>
    public enum EnumClouds
    {
        /// <summary>
        /// Ливень = .31f
        /// </summary>
        Showers = 5,
        /// <summary>
        /// Дождь = .41f
        /// </summary>
        Rain = 4,
        /// <summary>
        /// Сильная облачность = .51f
        /// </summary>
        HeavilyCloudy = 3,
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
                case EnumClouds.Showers: return .31f; // .0961
                case EnumClouds.Rain: return .41f; // .1681
                case EnumClouds.HeavilyCloudy: return .51f; // .2601
                case EnumClouds.Cloudy: return .61f; // .3721
                case EnumClouds.PartlyCloudy: return .76f; // .5776
            }
            return .91f; // .8281
        }
    }
}
