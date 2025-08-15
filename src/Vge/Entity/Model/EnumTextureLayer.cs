namespace Vge.Entity.Model
{
    /// <summary>
    /// Варианты текстур для слоёв
    /// </summary>
    public enum EnumTextureLayer
    {
        /// <summary>
        /// Обычная текстура, без ни каких корректировок
        /// </summary>
        None,
        /// <summary>
        /// Имеются слои, но текстура для сущности, обрезаем нижнуюю часть
        /// </summary>
        ThereAreLayers,
        /// <summary>
        /// Имеются слои, текстура для слоёв, обрезаем вверхнуюю часть
        /// </summary>
        ForLayer
    }
}
