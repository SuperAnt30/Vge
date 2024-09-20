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

    internal const string SomethingIsTooLong = "{0}Что-то слишком долго! {1} заняло приблизительно {2} мс";
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

    #endregion

    #region Client

    internal const string StartingSingle = "Запускается одиночная...";
    internal const string StartingMultiplayer = "Запускается по сети...";
    internal const string StoppedClient = "Остановлен{0}.";

    #endregion
}
