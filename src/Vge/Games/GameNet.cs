using System.IO;
using Vge.Network;

namespace Vge.Games
{
    /// <summary>
    /// Класс игры по сети, сервер не создаём
    /// </summary>
    public class GameNet : GameBase
    {
        SocketClient socket;

        public GameNet()
        {
            IsLoacl = false;
        }

        /// <summary>
        /// Запуск игры
        /// </summary>
        public override void GameStarting()
        {
            // По сети сервер
            socket = new SocketClient(System.Net.IPAddress.Parse("127.0.0.1"), 32021);
            socket.ReceivePacket += Socket_ReceivePacket;
            socket.Receive += Socket_Receive;
            socket.Error += Socket_Error;

            //Task.Factory.StartNew(() =>
            //{
            //    Thread.Sleep(500);
            //    socket.Connect();
            //});
            // TODO::2024-08-23 Net Task pool
            System.Threading.Tasks.Task.Factory.StartNew(socket.Connect);
        }

        

        public override void GameStoping()
        {
            if (socket != null)
            {
                // Игра по сети
                socket.Disconnect();
                // отправляем событие остановки
              //  ThreadServerStoped(errorNet);
            }
            OnStoped();
        }

        private void Socket_Error(object sender, ErrorEventArgs e)
        {
            OnStoped("Error: " + e.GetException().Message);
            OnError(e.GetException());
        }

        private void Socket_Receive(object sender, ServerPacketEventArgs e)
        {
            if (e.Packet.status == StatusNet.Disconnect)
            {
                OnStoped("gui.error.server.disconnect");
            }
        }

        private void Socket_ReceivePacket(object sender, ServerPacketEventArgs e)
            => RecievePacket(e);
    }

}
