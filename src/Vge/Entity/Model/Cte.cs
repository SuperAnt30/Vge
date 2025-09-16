namespace Vge.Entity.Model
{
    /// <summary>
    /// Константы названий тег для параметров сущности в json
    /// </summary>
    public sealed class Cte
    {

        #region Layer

        /// <summary>
        /// Название группы. (string)
        /// </summary>
        public const string Group = "Group";
        /// <summary>
        /// Массив названий папок. ([])
        /// </summary>
        public const string Folder = "Folder";

        #endregion

        #region Entity

        /// <summary>
        /// Объект фигуры. (string)
        /// </summary>
        public const string Model = "Model";
        /// <summary>
        /// Масштаб. (float)
        /// </summary>
        public const string Scale = "Scale";
        /// <summary>
        /// Индекс текстуры. (int)
        /// </summary>
        public const string TextureId = "TextureId";
        /// <summary>
        /// Только двигаться для тригерров анимации, вместо Forward, Back, Left, Right. (bool)
        /// </summary>
        public const string OnlyMove = "OnlyMove";
        /// <summary>
        /// Интервал между морганием глаз в игровых тиках, если равно 0, значит нет глаз. (int)
        /// </summary>
        public const string BlinkEye = "BlinkEye";
        /// <summary>
        /// Имя слоёв одежды. (string) 
        /// </summary>
        public const string NameShapeLayers = "NameShapeLayers";

        /// <summary>
        /// Имя клипа как ключ для программы, должны быть уникальны в одной сущности. (string)
        /// #Code: "Sneak" Code нужен только там где нет TriggeredBy тут он не обязателен и даже не нужен. И обязателен на Idle
        /// </summary>
        public const string Code = "Code";
        /// <summary>
        /// Имя клипа в Blockbanche. (string)
        /// </summary>
        public const string Animation = "Animation";
        /// <summary>
        /// Скорость анимации. (float)
        /// </summary>
        public const string AnimationSpeed = "AnimationSpeed";
        /// <summary>
        /// Название тригеров для активации текущей анимации. ([string])
        /// </summary>
        public const string TriggeredBy = "TriggeredBy";
        /// <summary>
        /// Объект силы элементов костей. ({})
        /// </summary>
        public const string ElementWeight = "ElementWeight";

        #endregion

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
        /// <summary>
        /// Анимация. ([])
        /// </summary>
        public const string Animations = "animations";

        #endregion

        #region Texture

        /// <summary>
        /// Название текстуры с модели. (string)
        /// </summary>
        public const string Texture = "texture";
        /// <summary>
        /// Данные текстуры, в формате image/png;base64. (string)
        /// </summary>
        public const string Source = "source";
        /// <summary>
        /// Массив слоёв текстур. ([])
        /// </summary>
        public const string Layers = "layers";
        /// <summary>
        /// Данные текстуры для слоя, в формате image/png;base64. (string)
        /// </summary>
        public const string DataUrl = "data_url";

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
        /// Видимость кости. (bool)
        /// </summary>
        public const string Visibility = "visibility";
        /// <summary>
        /// Дети древо скелета. ([])
        /// </summary>
        public const string Children = "children";

        #endregion

        #region Animation

        /// <summary>
        /// Цикличность анимации. (string)
        /// loop-цикл, hold-остановить на последнем кадре, once-проиграть один раз
        /// </summary>
        public const string Loop = "loop";
        /// <summary>
        /// Цикличность анимации, вариант тэг Loop, остановить на последнем кадре
        /// </summary>
        public const string Hold = "hold";
        /// <summary>
        /// Цикличность анимации, вариант тэг Loop, once-проиграть один раз
        /// </summary>
        public const string Once = "once";
        /// <summary>
        /// Длинна ролика. (float)
        /// </summary>
        public const string Length = "length";
        /// <summary>
        /// Начальное и конечное время в мс для микса. (string) но в реале цифра int
        /// </summary>
        public const string StartDelay = "start_delay";
        /// <summary>
        /// Аниматор костей. ({})
        /// </summary>
        public const string Animators = "animators";
        /// <summary>
        /// Кадры кости. ([])
        /// </summary>
        public const string Keyframes = "keyframes";
        /// <summary>
        /// Канал выполняет или rotation или position. (string)
        /// </summary>
        public const string Channel = "channel";
        /// <summary>
        /// Время когда надо выполнять данный кадр. (float)
        /// </summary>
        public const string Time = "time";
        /// <summary>
        /// Расположение позиции. ([])
        /// </summary>
        public const string DataPoints = "data_points";
        /// <summary>
        /// Координата. (float)
        /// </summary>
        public const string X = "x";
        /// <summary>
        /// Координата. (float)
        /// </summary>
        public const string Y = "y";
        /// <summary>
        /// Координата. (float)
        /// </summary>
        public const string Z = "z";

        #endregion


    }
}
