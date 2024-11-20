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
    internal const string ItIsImpossibleToCompareTwoObjects = "Невозможно сравнить два объекта";
    internal const string OutOfRangeArray = "Вне диапазона массива, количество {0}";
    internal const string OutOfRange = "Вне диапазона, значение {0}";
    internal const string InvalidEndJsonReadTag = "Некорректный тег чтения Json, отсутствует завершение {0} файла: {1}";
    internal const string InvalidJsonReadTag = "Некорректный тег чтения Json, символ {0} файла: {1}";

    #endregion

    #region Game

    internal const string EmptyArrayIsNotAllowed = "Пустой массив не разрешон";

    #endregion

    #region NBT и JSON

    internal const string AddingInappropriateTagTypesToList = "Добавление несоответствующих типов тегов в список тегов";
    internal const string IndexOutOfBoundsToSetTagInTagList = "Индекс выходит за пределы, чтобы установить тег в списке тегов";
    internal const string EmptyStringIsNotAllowed = "Пустая строка не разрешена";
    internal const string ErrorReadJsonBlockStat = "Ошибка чтения из json, параметров блока {0}";
    internal const string ErrorReadJsonBlockShape = "Ошибка чтения из json, фигура {1} блока {0}";

    #endregion
}
