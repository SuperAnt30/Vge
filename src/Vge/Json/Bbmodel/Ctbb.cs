namespace Vge.Json.Bbmodel
{
    /// <summary>
    /// Константы названий тег для параметров блоков и предметов из модели Blockbench в json
    /// </summary>
    public sealed class Ctbb
    {
        /// <summary>
        /// Массив текстур. ([])
        /// </summary>
        public const string Textures = "textures";
        /// <summary>
        /// Массив элементов параллелепипедов. ([])
        /// </summary>
        public const string Elements = "elements";

        #region Texture

        /// <summary>
        /// Название текстуры в блоке квада. (string)
        /// </summary>
        public const string Texture = "texture";
        /// <summary>
        /// Название текстуры в блоке текстур. (string)
        /// </summary>
        public const string Name = "name";

        #endregion

        #region Elements

        /// <summary>
        /// Начальная позиция куба. ([x, y, z]), координаты float
        /// </summary>
        public const string From = "from";
        /// <summary>
        /// Конечная позиция куба. ([x, y, z]), координаты float
        /// </summary>
        public const string To = "to";

        /// <summary>
        /// Центральная точка вращения куба. ([x, y, z]), координаты float
        /// </summary>
        public const string Origin = "origin";
        /// <summary>
        /// Вращение куба относительно точки Origin. ([x, y, z]), координаты float
        /// </summary>
        public const string Rotation = "rotation";

        /// <summary>
        /// Объект сторон расположений на текстуре. ({})
        /// </summary>
        public const string Faces = "faces";

        /// <summary>
        /// Расположение на текстуре. ([u1, v1, u2, v2]), координаты float
        /// </summary>
        public const string Uv = "uv";

        #endregion

    }
}
