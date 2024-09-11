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
            server.TestUserAllKill();
        }

        #region Server

        /// <summary>
        /// Запуск игры
        /// </summary>
        public override void GameStarting()
        {
            base.GameStarting();
            Log.Client(Srl.StartingSingle);
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
        public override void GameStoping(string notification, bool isWarning)
        {
            if (server != null)
            {
                stopNotification = notification;
                server.Stop();
            }
            else
            {
                packets.Clear();
                OnStoped(notification, isWarning);
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
            // Сюда прилетаем из друго-го потока, ставим пометку на остановку
            // В ближайшем тике произойдёт остановка
            isStop = true;
        }

        private void Server_RecievePacket(object sender, PacketBufferEventArgs e)
            => packets.ReceiveBuffer(e.Buffer.bytes);

        private void Server_RecieveMessage(object sender, StringEventArgs e)
        {
            // TODO::2024-08-27 сообщение основному клиенту от сервера
            // Надо учесть потокобезопастность, прилетает из другого потока
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

        /// <summary>
        /// Игровой такт
        /// </summary>
        public override void OnTick(float deltaTime)
        {
            base.OnTick(deltaTime);

            if (isStop)
            {
                OnStoped(stopNotification, stopIsWarning);
                isStop = false;
            }
        }

        public override void Dispose()
        {
            if (server != null)
            {
                server.Stop();
            }
        }
    }
}
