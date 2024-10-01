/// <summary>
/// StringsResource 
/// Строки ресурсов, на которые ссылается код
/// </summary>
internal sealed class Sr
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
    internal const string ThrownOut = "Выброшен";
    internal const string TimeOut = "Тайм-аут";
    internal const string StopServer = "Стоп сервер";
    internal const string TheGameHasntStartedYet = "Игра еще не началась";

    #endregion

    #region Renderer

    internal const string TheSymbolIsNotInTheList = "Символ [{0}] отсутствует в перечне";
    internal const string ThereIsNoDrawing = "Отсутствует прорисовка";

    #endregion

    #region Util

    internal const string FileMissing = "Файл {0} отсутствует";
    internal const string IncorrectParameterValue = "Некорректное значение параметра";
    internal const string TheValueMustBeGreaterThanZero = "Значение должно быть больше нуля";

    #endregion

    #region NBT

    internal const string AddingInappropriateTagTypesToList = "Добавление несоответствующих типов тегов в список тегов";
    internal const string IndexOutOfBoundsToSetTagInTagList = "Индекс выходит за пределы, чтобы установить тег в списке тегов";
    internal const string EmptyStringIsNotAllowed = "Пустая строка не разрешена";

    #endregion
}
