/// <summary>
/// ConstEngine
/// Класс констант и статический данных для движка
/// </summary>
public sealed class Ce
{
    #region Game

    /// <summary>
    /// Какой FPS вне игры, для уменьшения нагрузки на комп
    /// </summary>
    public const int FpsOffside = 30;

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

}

