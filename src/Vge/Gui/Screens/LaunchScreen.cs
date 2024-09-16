namespace Vge.Gui.Screens
{
    /**
     * ScreenSplash *
     *  |
     *  +-ScreenMainMenu *
     *     |  
     *     +-ScreenSingle .
     *     |  |
     *     |  +-ScreenWorldLoading
     *     |  |
     *     |  +-ScreenWorldSaving
     *     |
     *     +-ScreenMultiplayer .
     *     |  |
     *     |  +-ScreenConnecting
     *     |
     *     +-ScreenOptions .
     *     
     * Дополнительные:
     *  |
     *  +-ScreenNotification * (Ошибка, выход в главное меню)
     *  |
     *  +-ScreenYesNo * (Надо принять решения и вернутся в предыдущий экран)
     *  |
     *  +-ScreenInGameMenu
     *  
     */

    /// <summary>
    /// Запуск экранов
    /// </summary>
    public class LaunchScreen : Warp
    {
        public LaunchScreen(WindowMain window) : base(window) { }

        /// <summary>
        /// Закрыть скрин
        /// </summary>
        public void Close() => window.ScreenClose();

        /// <summary>
        /// Запустить родителя с атрибутом
        /// </summary>
        public void Parent(ScreenBase screen, EnumScreenParent enumParent)
            => window.ScreenLaunchFromParent(screen, enumParent);

        /// <summary>
        /// Создать скрин заставки
        /// </summary>
        public virtual void Splash() => window.ScreenCreate(new ScreenSplash(window));
        /// <summary>
        /// Создать скрин главного меню
        /// </summary>
        public virtual void MainMenu() => window.ScreenCreate(new ScreenMainMenu(window));
        /// <summary>
        /// Создать скрин выбора одиночной игры
        /// </summary>
        public virtual void Single() => window.ScreenCreate(new ScreenSingle(window));
        /// <summary>
        /// Создать скрин опций
        /// </summary>
        public virtual void Options() => window.ScreenCreate(new ScreenOptions(window));
        /// <summary>
        /// Создать скрин оповещения, после которого выйдем в меню
        /// </summary>
        public virtual void Notification(string notification)
            => window.ScreenCreate(new ScreenNotification(window, notification));
        /// <summary>
        /// Создать скрин сообщения YesNo
        /// </summary>
        public virtual void YesNo(ScreenBase parent, string text) 
            => window.ScreenCreate(new ScreenYesNo(window, parent, text), false);
     
        
    }
}
