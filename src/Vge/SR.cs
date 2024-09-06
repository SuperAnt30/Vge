/// <summary>
/// StringsResource 
/// Строки ресурсов, на которые ссылается код
/// </summary>
internal sealed class SR
{
    internal static string GetString(string name, params object[] args)
        => string.Format(name, args);

    #region Audio

    internal const string TheOpenALSoundLibraryFailedToInitialize = "Библиотека звука OpenAL не смогла инициализироваться, скорее всего файл OpenAL64.dll не подходит";
    internal const string TheOpenALSoundLibraryHasCollectedManyChannels = "Библиотека звука OpenAL собрала больше 10000 каналов, это подозрительно!";

    #endregion

    #region Network

    internal const string ErrorInGluingNetworkData = "Ошибка в склейке сетевых данных";
    internal const string ErrorWhileStartingTheNetwork = "Ошибка в момент запуска сети. ";
    internal const string ThrownOut = "Выкинут";
    internal const string StopServer = "Стоп сервер";

    #endregion

    #region Renderer

    internal const string TheSymbolIsNotInTheList = "Символ [{0}] отсутствует в перечне";
    internal const string ThereIsNoDrawing = "Отсутствует прорисовка";

    #endregion

    #region Util

    internal const string FileMissing = "Файл {0} отсутствует";
    internal const string IncorrectParameterValue = "Некорректное значение параметра";

    #endregion
}
