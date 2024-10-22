using System;
using System.Threading;
using Vge.Event;
using Vge.Gui.Screens;
using Vge.Network;
using Vge.Network.Packets.Server;
using Vge.World;

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
        private readonly GameServer _server;
        /// <summary>
        /// Флаг разрешение на запуск
        /// </summary>
        private readonly bool _flagRun = true;

        public GameLocal(WindowMain window, GameSettings gameSettings, AllWorlds worlds) : base(window)
        {
            _server = new GameServer(Log, gameSettings, worlds);
            _server.Closeded += _Server_Closeded;
            _server.Error += _Server_Error;
            _server.TextDebug += _Server_TextDebug;
            _server.TagDebug += _Server_TagDebug;
            _server.RecievePacket += _Server_RecievePacket;
            _server.RecieveMessage += _Server_RecieveMessage;
            _flagRun = _server.Init();
        }

        #region StartStopPause

        /// <summary>
        /// Задать паузу для одиночной игры
        /// </summary>
        public override void SetGamePauseSingle(bool value)
            => IsGamePaused = _server.SetGamePauseSingle(value);

        #endregion

        #region Server

        /// <summary>
        /// Включить сетевую игру
        /// </summary>
        public void OpenNet(int port) => _server.RunNet(port);
        /// <summary>
        /// Получить истину запущена ли сеть
        /// </summary>
        public bool IsRunNet() => _server.IsRunNet();

        /// <summary>
        /// Запуск игры
        /// </summary>
        public override void GameStarting()
        {
            base.GameStarting();
            Log.Client(Srl.StartingSingle);
            Log.Save();
            if (_flagRun)
            {
                if (Player.Login != "")
                {
                    //server.Starting(32021);
                    _server.Starting(Player.Login, Player.Token);
                }
                else
                {
                    _packets.Clear();
                    _OnStoped(L.T("PlayerNameMustNotBeEmpty"), true);
                }
            }
            else
            {
                _packets.Clear();
                _OnStoped(L.T("ErrorWhileLoadingFile"), true);
            }
        }

        /// <summary>
        /// Остановка игры
        /// </summary>
        public override void GameStoping(string notification, bool isWarning)
        {
            window.LScreen.Process(L.T("Saving") + Ce.Ellipsis);
            if (_server != null)
            {
                _stopNotification = notification;
                _server.Stop();
            }
            else
            {
                _packets.Clear();
                _OnStoped(notification, isWarning);
            }
        }

        /// <summary>
        /// Запущен ли сервер
        /// </summary>
        public bool IsServerRunning() => _server != null && _server.IsServerRunning;

        private void _Server_Error(object sender, ThreadExceptionEventArgs e)
            => _OnError(e.Exception);

        private void _Server_TextDebug(object sender, StringEventArgs e) 
            => _OnServerTextDebug(e.Text);

        private void _Server_TagDebug(object sender, StringEventArgs e)
            => _OnTagDebug(e);

        private void _Server_Closeded(object sender, EventArgs e)
        {
            _packets.Clear();
            // Сюда прилетаем из друго-го потока, ставим пометку на остановку
            // В ближайшем тике произойдёт остановка
            _isStop = true;
        }

        private void _Server_RecievePacket(object sender, PacketBufferEventArgs e)
            => _packets.ReceiveBuffer(e.Buffer.bytes);

        private void _Server_RecieveMessage(object sender, StringEventArgs e)
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

            if (_isStop)
            {
                _OnStoped(_stopNotification, _stopIsWarning);
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
            if (window.Screen != null && window.Screen is ScreenWorking screen)
            {
                if (packet.Status == PacketS02LoadingGame.EnumStatus.Begin)
                {
                    screen.ServerBegin(packet.Value);
                }
                else if (packet.Status == PacketS02LoadingGame.EnumStatus.Step)
                {
                    screen.ServerStep();
                }
                else if (packet.Status == PacketS02LoadingGame.EnumStatus.ServerGo)
                {
                    window.ScreenClose();
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
                _server.LocalReceivePacket(null, WritePacket.TranciveToArray(packet));
            }
        }

        #endregion

        public override void Dispose()
        {
            base.Dispose();
            if (_server != null)
            {
                _server.Stop();
            }
        }
    }
}
