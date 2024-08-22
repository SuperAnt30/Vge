using System;
using System.Threading;
using Vge.Network;
using Vge.Util;

namespace Vge.Games
{
    /// <summary>
    /// Абстрактный класс игры
    /// </summary>
    public abstract class GameBase
    {
        /// <summary>
        /// Локальная игра
        /// </summary>
        public bool IsLoacl { get; protected set; } = true;
        /// <summary>
        /// Пауза в игре
        /// </summary>
        public bool IsGamePaused { get; protected set; } = false;


        /// <summary>
        /// Запуск игры
        /// </summary>
        public virtual void GameStarting() { }

        /// <summary>
        /// Остановка игры
        /// </summary>
        public virtual void GameStoping()
        {
            //System.Threading.Thread.Sleep(500);
            OnStoped();
        }

        /// <summary>
        /// Задать паузу для одиночной игры
        /// </summary>
        public virtual void SetGamePauseSingle(bool value) { }

        /// <summary>
        /// Отправить пакет на сервер
        /// </summary>
        public virtual void TrancivePacket(IPacket packet) { }

        public override string ToString()
        {
            return string.Format("{0}{1}",
                IsLoacl ? "Local" : "Net", IsGamePaused ? " Pause" : "");

        }

        #region Event

        /// <summary>
        /// Событие остановлена игра
        /// </summary>
        public event EventHandler Stoped;
        protected void OnStoped() => Stoped?.Invoke(this, new EventArgs());

        /// <summary>
        /// Событие ошибки на сервере
        /// </summary>
        public event ThreadExceptionEventHandler Error;
        protected void OnError(Exception ex)
            => Error?.Invoke(this, new ThreadExceptionEventArgs(ex));

        /// <summary>
        /// Событие текст отладки сервера
        /// </summary>
        public event StringEventHandler ServerTextDebug;
        protected virtual void OnServerTextDebug(string text) 
            => ServerTextDebug?.Invoke(this, new StringEventArgs(text));

        #endregion
    }
}
