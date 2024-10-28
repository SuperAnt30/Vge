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
    public const int MaxBatchChunksTime = Tick​​Time * 4 / 5;
    /// <summary>
    /// Обзор в виде круга
    /// </summary>
    public static Vector2i[] OverviewCircles = new Vector2i[] { new Vector2i(0) };

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

