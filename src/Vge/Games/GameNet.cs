using System;
using System.IO;
using System.Net;
using System.Threading;
using Vge.Event;
using Vge.Network;
using Vge.Network.Packets.Client;
using Vge.Network.Packets.Server;
using Vge.Util;

namespace Vge.Games
{
    /// <summary>
    /// Класс игры по сети, сервер не создаём
    /// </summary>
    public class GameNet : GameBase
    {
        private SocketSide _socket;
        
        /// <summary>
        /// IP адрес сервера
        /// </summary>
        private readonly string _ipAddress;
        /// <summary>
        /// Portс сервера
        /// </summary>
        private readonly int _port;
        /// <summary>
        /// Флаг подключен ли к серверу
        /// </summary>
        private bool _isConnected = false;

        /// <summary>
        /// Оповещение если был стоп из потока
        /// </summary>
        private string _stopText;

        public GameNet(WindowMain window, string ipAddress, int port) : base(window)
        {
            IsLoacl = false;
            _ipAddress = ipAddress;
            _port = port;
        }

#if DEBUG

        /// <summary>
        /// Псевдоним игрока
        /// </summary>
        public override string ToLoginPlayer() => Options.Nickname + "Net";

#endif

        /// <summary>
        /// Имеется ли связь с сервером
        /// </summary>
        private bool _IsConnect() => _socket != null && _socket.IsConnect();

        #region StartStopPause

        /// <summary>
        /// Запуск игры
        /// </summary>
        public override void GameStarting()
        {
            base.GameStarting();
            Log.Client(Srl.StartingMultiplayer);
            Log.Save();

            _socket = new SocketSide(IPAddress.Parse(_ipAddress), _port);
            _socket.ReceivePacket += _Socket_ReceivePacket;
            _socket.Error += _Socket_Error;
            _socket.Connected += _Socket_Connected;
            _socket.Disconnected += _Socket_Disconnected;

            // Запуск поток для синхронной связи по сокету
            Thread myThread = new Thread(_NetThread) { Name = "GameNetwork" };
            myThread.Start();
        }

        /// <summary>
        /// Сетевой поток, работает покуда имеется связь
        /// </summary>
        private void _NetThread()
        {
            try
            {
                _socket.Connect();
            }
            catch(Exception e)
            {
                _Socket_Error(_socket, new ErrorEventArgs(e));
            }
        }

        public override void GameStoping(string notification, bool isWarning)
        {
            base.GameStoping(notification, isWarning);

            window.LScreen.Process(L.T("Leaving") + Ce.Ellipsis);
            _stopIsWarning = isWarning;
            _stopNotification = notification;
            if (_socket != null)
            {
                // Игра по сети
                _socket.DisconnectFromClient(notification);
                // отправляем событие остановки
                //  ThreadServerStoped(errorNet);
            }
            else
            {
                _Stop(notification, isWarning);
            }
        }

        /// <summary>
        /// Остановка сервера и игры
        /// </summary>
        /// <param name="notification">Уведомление причины</param>
        private void _Stop(string notification, bool isWarning)
        {
            // Останавливаем поток
            _packets.Clear();
            if (_stopNotification == "")
            {
                _OnStoped(notification , isWarning);
            }
            else
            {
                _OnStoped(_stopNotification, _stopIsWarning);
            }
        }

        private void _Socket_Connected(object sender, EventArgs e)
            => _isConnected = true;

        private void _Socket_Disconnected(object sender, StringEventArgs e)
        {
            _isConnected = false;
            _StopAfterTick(e.Text);
        }

        private void _Socket_Error(object sender, ErrorEventArgs e)
        {
            WorldRender.Stoping();
            _StopAfterTick("Error: " + e.GetException().Message);
        }

        private void _Socket_ReceivePacket(object sender, PacketBufferEventArgs e)
            => _packets.ReceiveBuffer(e.Bytes, e.Count);

        /// <summary>
        /// Остановить через тик
        /// </summary>
        private void _StopAfterTick(string text)
        {
            // Сюда прилетаем из друго-го потока, ставим пометку на остановку
            // В ближайшем тике произойдёт остановка
            _stopText = text;
            _isStop = true;
        }

        #endregion

        #region TickDraw

        /// <summary>
        /// Игровой такт
        /// </summary>
        /// <param name="deltaTime">Дельта последнего тика в mc</param>
        public override void OnTick(float deltaTime)
        {
            // Проверка на разрыв долгий сервера (нет связи)
            if (_flagTick && Player.TimeOut())
            {
                GameStoping(Srl.TheServerIsNotResponding, true);
            }
            base.OnTick(deltaTime);

            if (_isStop)
            {
                _Stop(_stopText, true);
                _isStop = false;
            }
        }

        #endregion

        #region Packets

        /// <summary>
        /// Получили пакет загрузки от сервера
        /// </summary>
        public override void PacketLoadingGame(PacketS02LoadingGame packet)
        {
            if (_IsConnect())
            {
                PacketS02LoadingGame.EnumStatus status = packet.Status;
                if (status == PacketS02LoadingGame.EnumStatus.BeginNet)
                {
                    _socket.SendPacket(new PacketC02LoginStart(Player.Login, Player.Token, Ce.IndexVersion));
                }
                else if (status == PacketS02LoadingGame.EnumStatus.VersionAnother)
                {
                    _socket.DisconnectFromClient(L.T("VersionAnother"));
                }
                else if (status == PacketS02LoadingGame.EnumStatus.LoginDuplicate)
                {
                    _socket.DisconnectFromClient(L.T("LoginDuplicate"));
                }
                else if (status == PacketS02LoadingGame.EnumStatus.LoginIncorrect)
                {
                    _socket.DisconnectFromClient(L.T("LoginIncorrect"));
                }
                else if (status == PacketS02LoadingGame.EnumStatus.InvalidToken)
                {
                    _socket.DisconnectFromClient(L.T("InvalidToken"));
                }
            }
        }

        /// <summary>
        /// Отправить пакет на сервер
        /// </summary>
        public override void TrancivePacket(IPacket packet)
        {
            if (_isConnected)
            {
                WritePacket writePacket = new WritePacket(packet);
                _socket.SendPacket(writePacket.ToArray());
            }
        }

        #endregion
    }
}
