using System.IO;
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

        public GameNet() => IsLoacl = false;

        /// <summary>
        /// Запуск игры
        /// </summary>
        public override void GameStarting()
        {
            base.GameStarting();

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
            socket = new SocketClient(System.Net.IPAddress.Parse("192.168.1.21"), 32021);
            socket.ReceivePacket += Socket_ReceivePacket;
            socket.Receive += Socket_Receive;
            socket.Error += Socket_Error;

            socket.Connect();
        }

        public override void GameStoping(string notification = "")
        {
            if (socket != null)
            {
                // Игра по сети
                socket.Disconnect();
                // отправляем событие остановки
              //  ThreadServerStoped(errorNet);
            }
            Stop(notification);
        }

        /// <summary>
        /// Остановка сервера и игры
        /// </summary>
        /// <param name="notification">Уведомление причины</param>
        private void Stop(string notification = "")
        {
            // Останавливаем поток
            packets.Clear();
            OnStoped(notification);
        }

        private void Socket_Error(object sender, ErrorEventArgs e)
        {
            Stop("Error: " + e.GetException().Message);
            OnError(e.GetException());
        }

        private void Socket_Receive(object sender, ServerPacketEventArgs e)
        {
            if (e.Packet.status == StatusNet.Disconnect)
            {
                Stop("gui.error.server.disconnect");
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
    }

}
