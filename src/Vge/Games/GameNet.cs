using System;
using System.IO;
using System.Net;
using System.Threading;
using Vge.Event;
using Vge.Network;

namespace Vge.Games
{
    /// <summary>
    /// Класс игры по сети, сервер не создаём
    /// </summary>
    public class GameNet : GameBase
    {
        private SocketSide socket;
        /// <summary>
        /// Оповещение остановки, если "" её ещё не было
        /// </summary>
        private string notificationStop = "";
        /// <summary>
        /// IP адрес сервера
        /// </summary>
        private readonly string ipAddress;
        /// <summary>
        /// Portс сервера
        /// </summary>
        private readonly int port;

        private bool isWorkGame = false;

        public GameNet(WindowMain window, string ipAddress, int port) : base(window)
        {
            IsLoacl = false;
            this.ipAddress = ipAddress;
            this.port = port;
        }

        /// <summary>
        /// Запуск игры
        /// </summary>
        public override void GameStarting()
        {
            base.GameStarting();
            Log.Client(SRL.StartingMultiplayer);
            Log.Save();

            socket = new SocketSide(IPAddress.Parse(ipAddress), port);
            socket.ReceivePacket += Socket_ReceivePacket;
            socket.Error += Socket_Error;
            socket.Connected += Socket_Connected;
            socket.Disconnected += Socket_Disconnected;

            // Запуск поток для синхронной связи по сокету
            Thread myThread = new Thread(NetThread);
            myThread.Start();
        }

        /// <summary>
        /// Сетевой поток, работает покуда имеется связь
        /// </summary>
        private void NetThread()
        {
            try
            {
                socket.Connect();
            }
            catch(Exception e)
            {
                Socket_Error(socket, new ErrorEventArgs(e));
            }
        }

        public override void GameStoping(string notification = "")
        {
            notificationStop = notification;
            if (socket != null)
            {
                // Игра по сети
                socket.DisconnectFromClient(notification);
                // отправляем событие остановки
                //  ThreadServerStoped(errorNet);
            }
            else
            {
                Stop(notification);
            }
        }

        /// <summary>
        /// Остановка сервера и игры
        /// </summary>
        /// <param name="notification">Уведомление причины</param>
        private void Stop(string notification = "")
        {
            // Останавливаем поток
            packets.Clear();
            OnStoped(notificationStop == "" ? notification : notificationStop);
        }

        private void Socket_Connected(object sender, EventArgs e)
            => isWorkGame = true;

        private void Socket_Disconnected(object sender, StringEventArgs e)
        {
            isWorkGame = false;
            Stop(e.Text);
        }

        private void Socket_Error(object sender, ErrorEventArgs e)
            => Stop("Error: " + e.GetException().Message);

        private void Socket_ReceivePacket(object sender, PacketBufferEventArgs e)
            => packets.ReceiveBuffer(e.Buffer.bytes);

        /// <summary>
        /// Отправить пакет на сервер
        /// </summary>
        public override void TrancivePacket(IPacket packet)
        {
            if (isWorkGame)
            {
                streamPacket.Trancive(packet);
                socket.SendPacket(streamPacket.ToArray());
            }
        }

        /// <summary>
        /// Игровой такт
        /// </summary>
        public override void OnTick(float deltaTime)
        {
            // Проверка на разрыв долгий сервера (нет связи)
            if (TimeOut())
            {
                GameStoping("Сервер не отвечает");
            }
            base.OnTick(deltaTime);
        }
    }
}
