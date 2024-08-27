using System;
using System.IO;
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
        private Server server;
        /// <summary>
        /// Уведомление остановки сервера
        /// </summary>
        private string stopNotification = "";

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
            base.GameStarting();
            Log.Log("[Client] Запускается одиночная...");
            stopNotification = "";
            server = new Server(Log);
            server.Closeded += Server_Closeded;
            server.Error += Server_Error;
            server.TextDebug += Server_TextDebug;
            server.RecievePacket += Server_RecievePacket;
            server.Starting();
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

        private void Server_RecievePacket(object sender, ServerPacketEventArgs e)
            => RecievePacket(e);

        #endregion

        /// <summary>
        /// Отправить пакет на сервер
        /// </summary>
        public override void TrancivePacket(IPacket packet)
        {
            if (IsServerRunning())
            {
                using (MemoryStream writeStream = new MemoryStream())
                {
                    using (StreamBase stream = new StreamBase(writeStream))
                    {
                        writeStream.WriteByte(ProcessPackets.GetId(packet));
                        packet.WritePacket(stream);
                        server.LocalReceivePacket(null, writeStream.ToArray());
                    }
                }
            }
        }
    }
}
