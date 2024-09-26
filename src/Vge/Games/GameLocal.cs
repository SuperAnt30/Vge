using System;
using System.Threading;
using Vge.Event;
using Vge.Gui.Screens;
using Vge.Network;
using Vge.Network.Packets.Server;

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
        /// Псевдоним игрока
        /// </summary>
        public override string ToLoginPlayer() => "AntLocal";
        /// <summary>
        /// токен игрока
        /// </summary>
        public override string ToTokenPlayer() => "F1";

        #region StartStopPause

        /// <summary>
        /// Задать паузу для одиночной игры
        /// </summary>
        public override void SetGamePauseSingle(bool value)
            => IsGamePaused = server.SetGamePauseSingle(value);

        #endregion

        #region Server

        /// <summary>
        /// Включить сетевую игру
        /// </summary>
        public void OpenNet() => server.RunNet(32021);

        /// <summary>
        /// Запуск игры
        /// </summary>
        public override void GameStarting()
        {
            base.GameStarting();
            Log.Client(Srl.StartingSingle);
            Log.Save();
            server.Starting(ToLoginPlayer(), ToTokenPlayer());
            // TODO::2024-08-27 Временно включил сеть
            //OpenNet();
        }

        /// <summary>
        /// Остановка игры
        /// </summary>
        public override void GameStoping(string notification, bool isWarning)
        {
            window.LScreen.Process(L.T("Saving") + Ce.Ellipsis);
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

        #region TickDraw

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

        #endregion

        #region Packets

        /// <summary>
        /// Получили пакет загрузки от сервера
        /// </summary>
        public override void PacketLoadingGame(PacketS02LoadingGame packet)
        {
            if (window.Screen != null && window.Screen is ScreenWorking screen)
            {
                if (packet.GetStatus() == PacketS02LoadingGame.EnumStatus.Begin)
                {
                    screen.ServerBegin(packet.GetValue());
                }
                else if (packet.GetStatus() == PacketS02LoadingGame.EnumStatus.Step)
                {
                    screen.ServerStep();
                }
            }
        }

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

        #endregion

        public override void Dispose()
        {
            if (server != null)
            {
                server.Stop();
            }
        }
    }
}
