namespace Vge.World.Сalendar
{
    /// <summary>
    /// Фаза луны
    /// </summary>
    public enum EnumMoonPhase
    {
        /// <summary>
        /// #0 Новолуние, когда Луна не видна
        /// </summary>
        NewMoon,
        /// <summary>
        /// #1 Hастущий серп (молодая луна)
        /// </summary>
        WaxingCrescent,
        /// <summary>
        /// #2 Первая четверь
        /// </summary>
        FirstQuarter,
        /// <summary>
        /// 3# Растущая выпуклая луна
        /// </summary>
        WaxingGibbous,
        /// <summary>
        /// #4 Полнолуние, когда освещена вся Луна целиком
        /// </summary>
        FullMoon,
        /// <summary>
        /// #5 Убывающая выпуклая луна
        /// </summary>
        WaningGibbous,
        /// <summary>
        /// #6 Третья четверть
        /// </summary>
        ThirdQuarter,
        /// <summary>
        /// #7 убывающий серп
        /// </summary>
        WaningCrescent
    }
}
