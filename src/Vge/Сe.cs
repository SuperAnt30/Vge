/// <summary>
/// ConstEngine
/// Класс констант и статический данных для движка
/// </summary>
public sealed class Ce
{
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
    /// Количество тактов в секунду, в minecraft 20
    /// </summary>
    public const int Tps = 30;
    /// <summary>
    /// Время в мс на такт, в minecraft 50
    /// </summary>
    public const int Tick​​Time = 1000 / Tps;

    #endregion
}

