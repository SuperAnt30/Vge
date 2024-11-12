using WinGL.Util;
/// <summary>
/// ConstEngine
/// Класс констант и статический данных для движка
/// </summary>
public sealed class Ce
{
    #region Debug

    /// <summary>
    /// Отладка на экране
    /// </summary>
    public static bool IsDebugDraw = false;
    /// <summary>
    /// Отладка чанков в 2д
    /// </summary>
    public static bool IsDebugDrawChunks = false;

    #endregion

    #region Game

    /// <summary>
    /// Какой FPS вне игры, для уменьшения нагрузки на комп
    /// </summary>
    public const int FpsOffside = 30;
    /// <summary>
    /// Максимальное количество выгрузки чанков за такт
    /// </summary>
    public const int MaxCountDroppedChunks = 100;
    /// <summary>
    /// Минимальный желаемый размер партии закачки чанков
    /// </summary>
    public const byte MinDesiredBatchSize = 2;
    /// <summary>
    /// Максимальный желаемый размер партии закачки чанков
    /// </summary>
    public const byte MaxDesiredBatchSize = 128;
    /// <summary>
    /// Стартовое значение желаемого размера партии закачки чанков
    /// </summary>
    public const byte StartDesiredBatchSize = 16;
    /// <summary>
    /// Максимальный время для закачки чанков
    /// </summary>
    public const int MaxBatchChunksTime = Tick​​Time / 3 * 2;
    /// <summary>
    /// Максимальный время для закачки и распаковки чанков
    /// </summary>
    public const int MaxBatchChunksTimeUnpack = Tick​​Time / 2;
    /// <summary>
    /// Через сколько тактов происходит переформирование FrustumCulling если хоть один чанк не догружен
    /// </summary>
    public const int CheckTickInitFrustumCulling = 5;

    #endregion

    #region Game Геометрия

    /**
   *      (North)
   *        0;-1
   *         N
   * (West)  |   (East)
   *   W ----+---- E 
   * -1;0    |    1;0
   *         S
   *        0;1
   *      (South)
   **/

    /// <summary>
    /// Обзор в виде круга
    /// </summary>
    public static Vector2i[] OverviewCircles = new Vector2i[] { new Vector2i(0) };
    /// <summary>
    /// Обзор в виде Сферы для Альфа блоков
    /// </summary>
    public static Vector3i[] OverviewAlphaSphere = new Vector3i[] { new Vector3i(0) };

    /// <summary>
    /// Область в один блок без центра, 4 блоков
    /// </summary>
    //public readonly static Vector2i[] AreaOne4 = new Vector2i[] {
    //    new Vector2i(0, 1), new Vector2i(1, 0), new Vector2i(0, -1), new Vector2i(-1, 0)
    //};
    /// <summary>
    /// Область в один блок без центра, 8 блоков
    /// </summary>
    //public readonly static Vector2i[] AreaOne8 = new Vector2i[] {
    //    new Vector2i(0, 1), new Vector2i(1, 1), new Vector2i(1, 0), new Vector2i(1, -1),
    //    new Vector2i(0, -1), new Vector2i(-1, -1), new Vector2i(-1, 0), new Vector2i(-1, 1)
    //};
    /// <summary>
    /// Область в один блок без центра, 8 блоков Х
    /// </summary>
    public readonly static int[] AreaOne8X = new int[] { 0, 1, 1, 1, 0, -1, -1, -1 };
    /// <summary>
    /// Область в один блок без центра, 8 блоков Y
    /// </summary>
    public readonly static int[] AreaOne8Y = new int[] { 1, 1, 0, -1, -1, -1, 0, 1 };

    /// <summary>
    /// Область в один блок c центром, 9 блоков
    /// </summary>
    //public readonly static Vector2i[] AreaOne9 = new Vector2i[] { new Vector2i(0, 0),
    //    new Vector2i(0, 1), new Vector2i(1, 1), new Vector2i(1, 0), new Vector2i(1, -1),
    //    new Vector2i(0, -1), new Vector2i(-1, -1), new Vector2i(-1, 0), new Vector2i(-1, 1)
    //};

    /// <summary>
    /// Получить индекс по вектору в один блок без центра, 8 блоков
    /// </summary>
    public static int GetAreaOne8(int x, int y)
    {
        if (x == 1)
        {
            if (y == 0) return 2;
            if (y == -1) return 3;
            return 1;
        }
        if (x == -1)
        {
            if (y == -1) return 5;
            if (y == 0) return 6;
            return 7;
        }
        if (y == -1) return 4;
        return 0;
    }
    /// <summary>
    /// Получить индекс по вектору в один блок без центра, 9 блоков
    /// </summary>
    public static int GetAreaOne9(int x, int y)
    {
        if (x == 0)
        {
            if (y == 0) return 0;
            if (y == 1) return 1;
            return 5;
        }
        if (x == 1)
        {
            if (y == 0) return 3;
            if (y == -1) return 4;
            return 2;
        }
        if (y == -1) return 6;
        if (y == 0) return 7;
        return 8;
    }

    #endregion

    #region Text

    /// <summary>
    /// Перенос коретки в тексте
    /// </summary>
    public const string Br = "\r\n";
    /// <summary>
    /// Многоточие
    /// </summary>
    public const string Ellipsis = "...";

    #endregion

    #region Server

    /// <summary>
    /// Индекс версии, покуда 1
    /// </summary>
    public const int IndexVersion = 1;
    /// <summary>
    /// Количество тактов в секунду, в minecraft 20
    /// </summary>
    public const int Tps = 30;
    /// <summary>
    /// Время в мс на такт, в minecraft 50
    /// </summary>
    public const int Tick​​Time = 1000 / Tps;

    #endregion

    #region Soket

    /// <summary>
    /// Указывающее, используется ли поток Socket в алгоритме Nagle.
    /// Значение false, если объект Socket использует алгоритм Nagle.
    /// https://learn.microsoft.com/ru-ru/dotnet/api/system.net.sockets.socket.nodelay?view=net-8.0
    /// </summary>
    public const bool NoDelay = true;
    /// <summary>
    /// Размер заголовка TCP пакета для подсчёта трафика
    /// </summary>
    public const byte SizeHeaderTCP = 40;

    #endregion

    #region Init

    /// <summary>
    /// Была ли инициализация для сервера
    /// </summary>
    private static bool _isInitServer = false;
    /// <summary>
    /// Была ли инициализация для клиента
    /// </summary>
    private static bool _isInitClient = false;

    /// <summary>
    /// Инициализация для сервера.
    /// Используется один раз
    /// </summary>
    public static void InitServer()
    {
        if (!_isInitServer)
        {
            _isInitServer = true;
        }
    }

    /// <summary>
    /// Инициализация для клиента.
    /// Используется один раз
    /// </summary>
    public static void InitClient()
    {
        if (!_isInitClient)
        {
            _isInitClient = true;
        }
    }

    #endregion
}

