using System.Collections.Generic;
using Vge.Util;
using WinGL.Util;
/// <summary>
/// ConstEngine
/// Класс констант и статический данных для движка
/// </summary>
public sealed class Ce
{
    #region Debug

    /// <summary>
    /// Отладка чанков в 2д
    /// </summary>
    public static bool FlagDebugDrawChunks = true;

    #endregion

    #region Game

    /// <summary>
    /// Какой FPS вне игры, для уменьшения нагрузки на комп
    /// </summary>
    public const int FpsOffside = 30;
    /// <summary>
    /// Минимальное обязательное количество загружаемых чанков для якоря за один такт
    /// </summary>
    public const int MinCountLoadingChunks = 2;
    /// <summary>
    /// Минимальный желаемый размер партии закачки чанков
    /// </summary>
    public const int MinDesiredBatchSize = 1;
    /// <summary>
    /// Максимальный желаемый размер партии закачки чанков
    /// </summary>
    public const int MaxDesiredBatchSize = 64;
    /// <summary>
    /// Максимальный время для артии закачки чанков
    /// </summary>
    public const int MaxBatchChunksTime = 20;
    /// <summary>
    /// Сколько выделено времени на загрузку чанков всех якоре за такт в мс
    /// </summary>
    public const int TimeLoadChunksAnchors = 10;

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

    /// <summary>
    /// Двумерный массив обзорных кругио от ближних колец к дальним.
    /// Используется только для сервера
    /// </summary>
    public static Vector2i[][] OverviewCircles { get; private set; }

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
            // 21 это максимальный обзор 16 + 5 серверные
            _InitOverviewCircles(21);
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

    /// <summary>
    /// Инициализация колец обзора
    /// </summary>
    private static void _InitOverviewCircles(int overviewLength)
    {
        // Увеличиваем для корректности массивов
        overviewLength++;
        OverviewCircles = new Vector2i[overviewLength][];
        for (int overview = 0; overview < overviewLength; overview++)
        {
            OverviewCircles[overview] = GenOverviewCircles(overview);
        }
    }

    /// <summary>
    /// Сгенерировать кольза для конкретного обзора
    /// </summary>
    public static Vector2i[] GenOverviewCircles(int overview)
    {
        ComparisonDistance comparison;
        List<ComparisonDistance> r = new List<ComparisonDistance>();
        for (int x = -overview; x <= overview; x++)
        {
            for (int y = -overview; y <= overview; y++)
            {
                comparison = new ComparisonDistance(x, y, Mth.Sqrt(x * x + y * y));
                if (comparison.Distance() - .3f <= overview)
                {
                    r.Add(comparison);
                }
            }
        }
        r.Sort();
        Vector2i[] overviewCircles = new Vector2i[r.Count];
        for (int i = 0; i < r.Count; i++)
        {
            overviewCircles[i] = r[i].GetPosition();
        }
        return overviewCircles;
    }

    #endregion
}

