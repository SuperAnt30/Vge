using System;
using System.Threading;
using Vge.Event;
using Vge.Network;

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
        private readonly Server server;
        /// <summary>
        /// Уведомление остановки сервера
        /// </summary>
        private string stopNotification = "";

        public GameLocal(WindowMain window) : base(window)
        {
            server = new Server(Log);
            server.Closeded += Server_Closeded;
            server.Error += Server_Error;
            server.TextDebug += Server_TextDebug;
            server.RecievePacket += Server_RecievePacket;
            server.RecieveMessage += Server_RecieveMessage;
        }

        /// <summary>
        /// Задать паузу для одиночной игры
        /// </summary>
        public override void SetGamePauseSingle(bool value)
        {
            IsGamePaused = server.SetGamePauseSingle(value);
            // TODO::TestUserAllKill();
            server.TestUserAllKill();
        }

        #region Server

        /// <summary>
        /// Запуск игры
        /// </summary>
        public override void GameStarting()
        {
            base.GameStarting();
            Log.Client(SRL.StartingSingle);
            Log.Save();
            server.Starting();
            // TODO::2024-08-27 Временно включил сеть
            server.RunNet(32021);

            // Тест краша
            //int i = 0; int j = 5 / i;
        }

        /// <summary>
        /// Остановка игры
        /// </summary>
        public override void GameStoping(string notification = "")
        {
            if (server != null)
            {
                stopNotification = notification;
                server.Stop();
            }
            else
            {
                packets.Clear();
                OnStoped(notification);
            }
        }

        /// <summary>
        /// Запущен ли сервер
        /// </summary>
        public bool IsServerRunning() => server != null && server.IsServerRunning;

        private void Server_Error(object sender, ThreadExceptionEventArgs e)
            => OnError(e.Exception);

        private void Server_TextDebug(object sender, StringEventArgs e) 
            => OnServerTextDebug(e.Text);

        private void Server_Closeded(object sender, EventArgs e)
        {
            packets.Clear();
            OnStoped(stopNotification);
        }

        private void Server_RecievePacket(object sender, PacketBufferEventArgs e)
            => packets.ReceiveBuffer(e.Buffer.bytes);

        private void Server_RecieveMessage(object sender, StringEventArgs e)
        {
            // TODO::2024-08-27 сообщение основному клиенту от сервера
        }

        #endregion

        /// <summary>
        /// Отправить пакет на сервер
        /// </summary>
        public override void TrancivePacket(IPacket packet)
        {
            if (IsServerRunning())
            {
                streamPacket.Trancive(packet);
                server.LocalReceivePacket(null, streamPacket.ToArray());
            }
        }
    }
}
