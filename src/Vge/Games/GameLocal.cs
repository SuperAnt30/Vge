using System;
using System.Threading;
using Vge.Network;
using Vge.Util;

namespace Vge.Games
{
    /// <summary>
    /// Класс локальной игры, надо создать сервер
    /// </summary>
    public class GameLocal : GameBase
    {
        /// <summary>
        /// Объект сервера
        /// </summary>
        private Server server;

        public GameLocal() { }

        /// <summary>
        /// Задать паузу для одиночной игры
        /// </summary>
        public override void SetGamePauseSingle(bool value)
        {
            IsGamePaused = server.SetGamePauseSingle(value);
        }

        #region Server

        /// <summary>
        /// Запуск игры
        /// </summary>
        public override void GameStarting()
        {
            server = new Server();
            server.Closeded += Server_Closeded;
            server.Error += Server_Error;
            server.TextDebug += Server_TextDebug;
            server.Starting();
        }

        /// <summary>
        /// Остановка игры
        /// </summary>
        public override void GameStoping()
        {
            if (server != null)
            {
                server.Stoping();
            }
        }

        private void Server_Error(object sender, ThreadExceptionEventArgs e)
            => OnError(e.Exception);

        private void Server_TextDebug(object sender, StringEventArgs e) 
            => OnServerTextDebug(e.Text);

        private void Server_Closeded(object sender, EventArgs e)
            => OnStoped();

        #endregion
    }
}
