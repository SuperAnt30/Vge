namespace Vge.Entity
{
    /// <summary>
    /// Константы названий тег для параметров сущности в json
    /// </summary>
    public sealed class Cte
    {
        /// <summary>
        /// Объект фигуры. (string)
        /// </summary>
        public const string Model = "Model";

        /// <summary>
        /// Ширина. (int)
        /// </summary>
        public const string Width = "width";
        /// <summary>
        /// Высота. (int)
        /// </summary>
        public const string Height = "height";


        #region Model

        /// <summary>
        /// Объект разрешения. ({})
        /// </summary>
        public const string Resolution = "resolution";
        /// <summary>
        /// Массив текстур. ([])
        /// </summary>
        public const string Textures = "textures";
        /// <summary>
        /// Массив элементов параллелепипедов. ([])
        /// </summary>
        public const string Elements = "elements";
        /// <summary>
        /// Древо скелета. ([])
        /// </summary>
        public const string Outliner = "outliner";

        #endregion

        #region Texture

        /// <summary>
        /// Данные текстуры, в формате image/png;base64. (string)
        /// </summary>
        public const string Source = "source";

        #endregion

        #region Elements

        /// <summary>
        /// Название куба. (string)
        /// </summary>
        public const string Name = "name";
        /// <summary>
        /// Идентификационный ключ куба. (string)
        /// </summary>
        public const string Uuid = "uuid";
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

        /// <summary>
        /// Объект стороны Север. ({})
        /// </summary>
        public const string North = "north";
        /// <summary>
        /// Объект стороны Юг. ({})
        /// </summary>
        public const string South = "south";
        /// <summary>
        /// Объект стороны Запад. ({})
        /// </summary>
        public const string West = "west";
        /// <summary>
        /// Объект стороны Восток. ({})
        /// </summary>
        public const string East = "east";
        /// <summary>
        /// Объект стороны Вверх. ({})
        /// </summary>
        public const string Up = "up";
        /// <summary>
        /// Объект стороны Низ. ({})
        /// </summary>
        public const string Down = "down";

        #endregion

        #region Outliner

        /// <summary>
        /// Дети древо скелета. ([])
        /// </summary>
        public const string Children = "children";

        #endregion

    }
}
