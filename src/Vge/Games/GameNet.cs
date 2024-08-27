using System.IO;
using System.Net;
using System.Threading;
using Vge.Network;

namespace Vge.Games
{
    /// <summary>
    /// Класс игры по сети, сервер не создаём
    /// </summary>
    public class GameNet : GameBase
    {
        private SocketClient socket;
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

        public GameNet(string ipAddress, int port) : base()
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
            Log.Client("Запускается по сети...");
            Log.Save();
            // Запуск поток для синхронной связи по сокету
            Thread myThread = new Thread(NetThread);
            myThread.Start();
        }

        /// <summary>
        /// Сетевой поток, работает покуда имеется связь
        /// </summary>
        private void NetThread()
        {
            // По сети сервер
            socket = new SocketClient(IPAddress.Parse(ipAddress), port);
            socket.ReceivePacket += Socket_ReceivePacket;
            socket.Receive += Socket_Receive;
            socket.Error += Socket_Error;

            socket.Connect();
        }

        public override void GameStoping(string notification = "")
        {
            notificationStop = notification;
            if (socket != null)
            {
                // Игра по сети
                socket.Disconnect();
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

        private void Socket_Error(object sender, ErrorEventArgs e)
            => Stop("Error: " + e.GetException().Message);

        private void Socket_Receive(object sender, ServerPacketEventArgs e)
        {
            if (e.Packet.status == StatusNet.Disconnect)
            {
                Stop("Сервер разорвал соединение!");
            }
        }

        private void Socket_ReceivePacket(object sender, ServerPacketEventArgs e)
            => RecievePacket(e);

        /// <summary>
        /// Отправить пакет на сервер
        /// </summary>
        public override void TrancivePacket(IPacket packet)
        {
            //if (IsStartWorld)
            {
                using (MemoryStream writeStream = new MemoryStream())
                {
                    using (StreamBase stream = new StreamBase(writeStream))
                    {
                        writeStream.WriteByte(ProcessPackets.GetId(packet));
                        packet.WritePacket(stream);
                        socket.SendPacket(writeStream.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// Игровой такт
        /// </summary>
        public override void OnTick()
        {
            // Проверка на разрыв долгий сервера (нет связи)
            if (TimeOut())
            {
                GameStoping("Сервер не отвечает");
            }
            base.OnTick();
        }
    }

}
