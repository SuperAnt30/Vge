namespace Vge.World.Block
{
    /// <summary>
    /// Константы названий тег для параметров блока в json
    /// </summary>
    public sealed class Ctb
    {
        #region Статы блока

        /// <summary>
        /// Отмечает, относится ли этот блок к типу, требующему случайной пометки в тиках. (bool)
        /// </summary>
        public const string NeedsRandomTick = "NeedsRandomTick";
        /// <summary>
        /// Сколько света вычитается для прохождения этого блока Air = 0. (0-15)
        /// </summary>
        public const string LightOpacity = "LightOpacity";
        /// <summary>
        /// Количество излучаемого света (плафон). (0-15)
        /// </summary>
        public const string LightValue = "LightValue";
        /// <summary>
        /// Прозрачный блок, не только альфа, с прозрачной текстурой. (bool)
        /// </summary>
        public const string Translucent = "Translucent";
        /// <summary>
        /// Блок должен использовать самое яркое значение соседнего света как свое собственное, листва, вода, стекло. (bool)
        /// </summary>
        public const string UseNeighborBrightness = "UseNeighborBrightness";
        /// <summary>
        /// Обрабатывается блок эффектом АmbientOcclusion. (bool)
        /// </summary>
        public const string АmbientOcclusion = "АmbientOcclusion";
        /// <summary>
        /// Обрабатывается блок эффектом Плавного перехода цвета между биомами. (bool)
        /// </summary>
        public const string BiomeColor = "BiomeColor";
        /// <summary>
        /// Может ли быть тень сущности на блоке, только для целых блоков. (bool)
        /// </summary>
        public const string Shadow = "Shadow";
        /// <summary>
        /// Цвет блока, используется при BiomeColor = true. ([red, green, blue]), цвет указывается 0-1.0
        /// </summary>
        public const string Color = "Color";
        /// <summary>
        /// Блок без коллизии
        /// </summary>
        public const string NoCollision = "NoCollision";

        #endregion

        #region Variants 

        /// <summary>
        /// Массив объектов вариантов фигур. ([])
        /// </summary>
        public const string Variants = "Variants";
        /// <summary>
        /// Объект фигуры ждикости. (string)
        /// </summary>
        public const string Liquid = "Liquid";
        /// <summary>
        /// Объект фигуры. ({})
        /// </summary>
        public const string Shape = "Shape";
        /// <summary>
        /// Смещение готовой фигуры на количество px. ([x, y, z]), координаты float
        /// </summary>
        public const string Offset = "Offset";
        /// <summary>
        /// Масштаб 1.0 == 100%. (float)
        /// </summary>
        public const string Scale = "Scale";

        /// <summary>
        /// Имеется ли вращение фигуры на 90 градусах по координате X. (bool)
        /// Если используется и IsRotateX90 и RotateY, сначало вращаем X потом Y
        /// Только для блока
        /// </summary>
        public const string IsRotateX90 = "IsRotateX90";
        /// <summary>
        /// Фращение фигуры кратно 90 градусах по координате Y. (90 || 180 || 270)
        /// Если используется и IsRotateX90 и RotateY, сначало вращаем X потом Y
        /// Только для блока
        /// </summary>
        public const string RotateY = "RotateY";

        #endregion

        #region Shape

        /// <summary>
        /// Объект текстуры, где имеется справочник, ключом выступает string, 
        /// а значением string имя где находится png текстуры, для дальнейшей склейки. ({})
        /// </summary>
        public const string Texture = "Texture";
        /// <summary>
        /// Массив элементов параллелепипедов. ([])
        /// </summary>
        public const string Elements = "Elements";
        /// <summary>
        /// Отъект элемента для жидкого блока. ({})
        /// </summary>
        public const string Element = "Element";
        /// <summary>
        /// Массив сторон параллелепипеда. ([])
        /// </summary>
        public const string Faces = "Faces";
        /// <summary>
        /// Массив начальной точки параллелепипеда px. ([x, y, z]), координаты float
        /// </summary>
        public const string From = "From";
        /// <summary>
        /// Массив конечной точки параллелепипеда px. ([x, y, z]), координаты float
        /// </summary>
        public const string To = "To";
        /// <summary>
        /// Центральная точка вращения куба. ([x, y, z]), координаты float
        /// </summary>
        public const string Origin = "Origin";
        /// <summary>
        /// Задаём вращение блока от центра, в градусах. ([x, y, z]), координаты float
        /// </summary>
        public const string Rotate = "Rotate";
        /// <summary>
        /// Отсутствие оттенка, т.е. зависит от стороны света, если true оттенка нет. (bool)
        /// </summary>
        public const string Shade = "Shade";
        /// <summary>
        /// Контраст, для текстуры без mipmap. (bool)
        /// </summary>
        public const string Sharpness = "Sharpness";
        /// <summary>
        /// Ветер, 0 - нет движения 1 - (по умолчанию) вверх двигается низ нет, 
        /// 2 - низ двигается вверх нет, 3 - двигается всё. (int)
        /// </summary>
        public const string Wind = "Wind";

        #endregion

        #region Face

        /// <summary>
        /// Сторона блока enumPole. (string)
        /// </summary>
        public const string Side = "Side";
        /// <summary>
        /// Цвет биома, где 0 - нет цвета, 1 - трава, 2 - листа, 3 - вода, 4 - свой цвет. (byte)
        /// В жидкостях в объекте Element, остальных Face
        /// </summary>
        public const string TypeColor = "TypeColor";
        /// <summary>
        /// Указывается ключ из объекта Texture. (string)
        /// </summary>
        public const string TextureFace = "Texture";
        /// <summary>
        /// Массив координат текстуры, по умолчанию [0, 0, 16, 16]. ([u1, v1, u2, v2]) координаты float
        /// </summary>
        public const string Uv = "Uv";
        /// <summary>
        /// Вращение текстуры кратно 90 градусах, (90 || 180 || 270)
        /// </summary>
        public const string UvRotate = "UvRotate";
        /// <summary>
        /// Количество пропускающих тактов, для анимациионых текстур, 0 и 1 без паузы. (byte)
        /// </summary>
        public const string Pause = "Pause";

        #endregion
    }
}