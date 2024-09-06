namespace Vge.Event
{
    public delegate void GameStopEventHandler(object sender, GameStopEventArgs e);
    public class GameStopEventArgs
    {
        /// <summary>
        /// Уведомление
        /// </summary>
        public readonly string Notification;
        /// <summary>
        /// Статус внимания
        /// </summary>
        public readonly bool IsWarning;

        public GameStopEventArgs(string notification, bool isWarning)
        {
            Notification = notification;
            IsWarning = isWarning;
        }
    }
}
