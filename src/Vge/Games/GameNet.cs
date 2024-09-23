using System;
using System.IO;
using System.Net;
using System.Threading;
using Vge.Event;
using Vge.Network;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;

namespace Vge.Games
{
    /// <summary>
    /// Класс игры по сети, сервер не создаём
    /// </summary>
    public class GameNet : GameBase
    {
        private SocketSide socket;
        
        /// <summary>
        /// IP адрес сервера
        /// </summary>
        private readonly string ipAddress;
        /// <summary>
        /// Portс сервера
        /// </summary>
        private readonly int port;

        private bool isWorkGame = false;

        
        /// <summary>
        /// Оповещение если был стоп из потока
        /// </summary>
        private string stopText;

        public GameNet(WindowMain window, string ipAddress, int port) : base(window)
        {
            IsLoacl = false;
            this.ipAddress = ipAddress;
            this.port = port;
        }

        /// <summary>
        /// Псевдоним игрока
        /// </summary>
        public override string ToLoginPlayer() => "AntNet";
        /// <summary>
        /// токен игрока
        /// </summary>
        public override string ToTokenPlayer() => "F0";

        /// <summary>
        /// Имеется ли связь с сервером
        /// </summary>
        private bool IsConnect() => socket != null && socket.IsConnect();

        #region StartStopPause

        /// <summary>
        /// Запуск игры
        /// </summary>
        public override void GameStarting()
        {
            base.GameStarting();
            Log.Client(Srl.StartingMultiplayer);
            Log.Save();

            socket = new SocketSide(IPAddress.Parse(ipAddress), port);
            socket.ReceivePacket += Socket_ReceivePacket;
            socket.Error += Socket_Error;
            socket.Connected += Socket_Connected;
            socket.Disconnected += Socket_Disconnected;

            // Запуск поток для синхронной связи по сокету
            Thread myThread = new Thread(NetThread) { Name = "GameNetwork" };
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

        public override void GameStoping(string notification, bool isWarning)
        {
            window.LScreen.Process(L.T("Leaving") + Ce.Ellipsis);
            stopIsWarning = isWarning;
            stopNotification = notification;
            if (socket != null)
            {
                // Игра по сети
                socket.DisconnectFromClient(notification);
                // отправляем событие остановки
                //  ThreadServerStoped(errorNet);
            }
            else
            {
                Stop(notification, isWarning);
            }
        }

        /// <summary>
        /// Остановка сервера и игры
        /// </summary>
        /// <param name="notification">Уведомление причины</param>
        private void Stop(string notification, bool isWarning)
        {
            // Останавливаем поток
            packets.Clear();
            if (stopNotification == "")
            {
                OnStoped(notification , isWarning);
            }
            else
            {
                OnStoped(stopNotification, stopIsWarning);
            }
        }

        private void Socket_Connected(object sender, EventArgs e)
            => isWorkGame = true;

        private void Socket_Disconnected(object sender, StringEventArgs e)
        {
            isWorkGame = false;
            StopAfterTick(e.Text);
        }

        private void Socket_Error(object sender, ErrorEventArgs e)
            => StopAfterTick("Error: " + e.GetException().Message);

        private void Socket_ReceivePacket(object sender, PacketBufferEventArgs e)
            => packets.ReceiveBuffer(e.Buffer.bytes);

        /// <summary>
        /// Остановить через тик
        /// </summary>
        private void StopAfterTick(string text)
        {
            // Сюда прилетаем из друго-го потока, ставим пометку на остановку
            // В ближайшем тике произойдёт остановка
            stopText = text;
            isStop = true;
        }

        #endregion

        #region TickDraw

        /// <summary>
        /// Игровой такт
        /// </summary>
        public override void OnTick(float deltaTime)
        {
            // Проверка на разрыв долгий сервера (нет связи)
            if (flagTick && TimeOut())
            {
                GameStoping(Srl.TheServerIsNotResponding, true);
            }
            base.OnTick(deltaTime);

            if (isStop)
            {
                Stop(stopText, true);
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
            if (IsConnect())
            {
                PacketS02LoadingGame.EnumStatus status = packet.GetStatus();
                if (status == PacketS02LoadingGame.EnumStatus.BeginNet)
                {
                    socket.SendPacket(new PacketC02LoginStart(ToLoginPlayer(), ToTokenPlayer(), Ce.IndexVersion));
                }
                else if (status == PacketS02LoadingGame.EnumStatus.VersionAnother)
                {
                    socket.DisconnectFromClient(L.T("VersionAnother"));
                }
                else if (status == PacketS02LoadingGame.EnumStatus.LoginDuplicate)
                {
                    socket.DisconnectFromClient(L.T("LoginDuplicate"));
                }
                else if (status == PacketS02LoadingGame.EnumStatus.LoginIncorrect)
                {
                    socket.DisconnectFromClient(L.T("LoginIncorrect"));
                }
                else if (status == PacketS02LoadingGame.EnumStatus.InvalidToken)
                {
                    socket.DisconnectFromClient(L.T("InvalidToken"));
                }
            }
        }

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

        #endregion
    }
}
