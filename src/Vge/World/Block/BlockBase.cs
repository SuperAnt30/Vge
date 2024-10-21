namespace Vge.World.Block
{
    /// <summary>
    /// Базовый объект Блока
    /// </summary>
    public class BlockBase
    {

        /// <summary>
        /// Имеет ли блок данные
        /// </summary>
        public bool IsMetadata { get; protected set; }
        /// <summary>
        /// Сколько света вычитается для прохождения этого блока Air = 0
        /// В VoxelEngine он в public static byte GetBlockLightOpacity(EnumBlock eblock)
        /// получть инфу не создавая блок
        /// </summary>
        public byte LightOpacity { get; protected set; } = 15;
        /// <summary>
        /// Количество излучаемого света (плафон)
        /// </summary>
        public int LightValue { get; protected set; }
        /// <summary>
        /// Отмечает, относится ли этот блок к типу, требующему случайной пометки в тиках. 
        /// Объект ChunkStorage подсчитывает блоки, чтобы в целях эффективности отобрать фрагмент из 
        /// случайного списка обновлений фрагментов.
        /// </summary>
        public bool NeedsRandomTick { get; protected set; }
        /// <summary>
        /// Блок не прозрачный
        /// </summary>
        public bool IsNotTransparent { get; protected set; }
        /// <summary>
        /// Можно ли выбирать блок
        /// </summary>
        public bool IsAction { get; protected set; } = true;
        /// <summary>
        /// Может ли блок сталкиваться
        /// </summary>
        public bool IsCollidable { get; protected set; } = true;

        /// <summary>
        /// Инициализировать блок
        /// </summary>
        public virtual void Initialization()
        {
            // Задать что блок не прозрачный
            if (LightOpacity > 13) IsNotTransparent = true;
        }

        /// <summary>
        /// Дополнительная инициализация блока после инициализации предметов
        /// </summary>
        public virtual void InitializationAfterItems() { }

    }
}
