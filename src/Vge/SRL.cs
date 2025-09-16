/// <summary>
/// StringsResourceLogs 
/// Строки ресурсов логов, на которые ссылается код
/// </summary>
internal sealed class Srl
{
    internal static string GetString(string name, params object[] args)
        => string.Format(name, args);

    #region Prefix

    internal const string Client = "[Client] ";
    internal const string Server = "[Server] ";
    internal const string Error = "[ERROR] ";
    internal const string ErrorException = "[ERROR]{0}: {1}\r\n------\r\n{2}";

    #endregion

    #region Profiler

    internal const string SomethingIsTooLong = "{0}Долго! {1} заняло ~{2} мс";
    internal const string EndSectionTime = "{0}{1} {2:0.00} мс";

    #endregion

    #region Game

    internal const string TheServerIsNotResponding = "Сервер не отвечает";
    internal const string ClosingTheApplication = "Закрытие приложения";
    internal const string TheUserStoppedTheGame = "Пользователь остановил игру";

    #endregion

    #region Network

    internal const string TheConnectionIsBroken = "Связь разорвана";
    internal const string TheConnectionWasBrokenDueToAnError = "Связь разорвана из-за ошибки. {0}";
    internal const string NoConnection = "Соединение отсутствует";

    #endregion

    #region Server

    internal const string Starting = "Запускаем slot={0} idVer={1}";
    internal const string ConnectedToTheServer = "[{0}] Подключен к серверу.";
    internal const string DisconnectedFromServer = "[{0}] Отключен от сервера. {1}.";
    internal const string NetworkModeIsEnabled = "Включен сетевой режим";
    internal const string Go = "Go!";
    internal const string LaggingBehind = "Не успеваю! Отставание на {0} мс, пропуск тиков {1}";
    internal const string Stopping = "Останавливаем...";
    internal const string StoppedServer = "Остановлен.";
    internal const string ServerVersionAnother = "Версия [{1}] клиента [{0}] другая.";
    internal const string ServerSavingGame = "Сохранение игры на сервере.";
    internal const string ServerLoginIncorrect = "Никнейм [{0}] некорректный.";
    internal const string ServerLoginDuplicate = "Никнейм [{0}] повторяется, игрок с таким никнеймом уже в игре.";
    internal const string ServerInvalidToken = "Токен [{1}] никнейма [{0}] отличается с прошлого входа в игру.";
    internal const string ServerLoginStart = "Сетевой игрок [{0}]:[{1}] вошёл в игру.";
    internal const string ServerLoginStartOwner = "Игрок владелец [{0}] вошёл в игру.";
    internal const string EntityAlreadyBeingTracked = "Сущность [{0}] уже отслеживается!.";
    internal const string DetectingObjectTrackingError = "Обнаружение ошибки отслеживания объекта: {0}";
    internal const string UnacceptablePortableItem = "{0} {1} пытался установить недопустимый переносимый предмет";

    #endregion

    #region Client

    internal const string StartingSingle = "Запускается одиночная...";
    internal const string StartingMultiplayer = "Запускается по сети...";
    internal const string StoppedClient = "Остановлен{0}.";

    #endregion

    internal const string ItIsImpossibleToCompareTwoObjects = "Невозможно сравнить два объекта";
}
