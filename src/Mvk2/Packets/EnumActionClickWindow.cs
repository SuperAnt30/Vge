namespace Mvk2.Packets
{
    /// <summary>
    /// Варианты действия для пакета PacketC0EClickWindow атрибуты Action
    /// </summary>
    public enum EnumActionClickWindow : byte
    {
        /// <summary>
        /// Открыто окно отладочного ящика
        /// </summary>
        OpenBoxDebug = 0,
        /// <summary>
        /// Закрыть окно
        /// </summary>
        Close = 1,
        /// <summary>
        /// Выкинуть предмет из рук что в кэше
        /// </summary>
        ThrowOutAir = 2,
        /// <summary>
        /// Кликнули на слот инвентаря
        /// </summary>
        ClickSlot = 3,
        /// <summary>
        /// Запрос на сделать крафт предмета(ов)
        /// </summary>
        Craft = 4,
        /// <summary>
        /// Добавить топливо в печь
        /// </summary>
        FurnaceFuel = 5,
        /// <summary>
        /// Открыт инвентарь персонажа
        /// </summary>
        OpenInventory = 6,
        /// <summary>
        /// Открыто окно знаний
        /// </summary>
        OpenKnowledge = 7,
        /// <summary>
        /// Окно конца игры, выбор продолжить играть
        /// </summary>
        EndContinue = 8
    }
}
