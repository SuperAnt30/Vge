namespace Vge.World.Block
{
    /// <summary>
    /// Интерфейс материала
    /// </summary>
    public interface IMaterial
    {
        /// <summary>
        /// Получить уникальный номер материала
        /// </summary>
        int IndexMaterial { get; }

        /// <summary>
        /// Возвращает, если блоки этих материалов являются жидкостями
        /// </summary>
        bool IsLiquid { get; }
        /// <summary>
        /// Не требует инструмента для разрушения блока
        /// </summary>
        bool RequiresNoTool { get; }
        /// <summary>
        /// Является ли блок стеклянным, блок или панель
        /// </summary>
        bool Glass { get; }
        /// <summary>
        /// Дёрн не сохнет, под этим блоком
        /// </summary>
        bool TurfDoesNotDry { get; }
        /// <summary>
        /// Воспламеняет (лава или огонь)
        /// </summary>
        bool Ignites { get; }
        /// <summary>
        /// Растёт корень
        /// </summary>
        bool RootGrowing { get; }
    }
}
