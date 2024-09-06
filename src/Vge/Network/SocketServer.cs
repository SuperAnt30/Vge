using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Vge.Event;

namespace Vge.Network
{
    /// <summary>
    /// Объект сервера используя сокет
    /// </summary>
    public class SocketServer
    {
        /// <summary>
        /// Сокет сервера
        /// </summary>
        private Socket socket;
        /// <summary>
        /// Порт сервера
        /// </summary>
        public readonly int Port;

        /// <summary>
        /// Колекция сокетов клиентов
        /// </summary>
        private List<SocketSide> clients = new List<SocketSide>();

        public SocketServer(int port) => Port = port;

        /// <summary>
        /// Получить истину есть ли соединение
        /// </summary>
        public bool IsConnected() => socket != null;

        #region Runing

        /// <summary>
        /// Начинаем слушать входящие соединения
        /// </summary>
        public bool Run()
        {
            if (socket != null) return true;

            try
            {
                // очистили список клиентов
                clients.Clear();

                // Создание сокета сервера
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Связываем сокет с конечной точкой
                socket.Bind(new IPEndPoint(IPAddress.Any, Port));
                // Начинаем слушать входящие соединения
                socket.Listen(10);

                // Запуск ожидание клиента
                socket.BeginAccept(new AsyncCallback(AcceptCallback), socket);

                OnRunned();
                return true;
            }
            catch (Exception e)
            {
                OnWarningString(SR.ErrorWhileStartingTheNetwork + e.Message);
                if (socket != null)
                {
                    if (socket.Connected)
                    {
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Disconnect(false);
                        socket.Close();
                    }
                    socket = null;
                }
                return false;
            }
        }

        /// <summary>
        /// TODO::TestUserAllKill
        /// </summary>
        public void TestUserAllKill()
        {
            if (clients.Count > 0)
            {
                for (int i = clients.Count - 1; i >= 0; i--)
                {
                    DisconnectHandler(clients[i], SR.ThrownOut);
                }
            }
        }

        /// <summary>
        /// Остановить сервер
        /// </summary>
        public void Stop()
        {
            if (socket == null)
            {
                OnStopped();
            }
            else
            {
                try
                {
                    if (clients.Count > 0)
                    {
                        for (int i = clients.Count - 1; i >= 0; i--)
                        {
                            DisconnectHandler(clients[i], SR.StopServer);
                        }
                    }
                    socket.Close();
                    socket = null;
                }
                catch (Exception e)
                {
                    OnError(new ErrorEventArgs(e));
                }
            }
        }

        #endregion

        /// <summary>
        /// Ждём покуда не получим клиента
        /// </summary>
        private void AcceptCallback(IAsyncResult ar)
        {
            // обрабатываемый сокет
            Socket socketCln;

            try
            {
                socketCln = socket.EndAccept(ar);
            }
            catch (Exception e)
            {
                if (socket == null)
                {
                    OnStopped();
                }
                else
                {
                    OnError(new ErrorEventArgs(e));
                }
                return;
            }

            SocketSide socketSide = new SocketSide(socketCln);
            socketSide.ReceivePacket += SocketSide_ReceivePacket;
            socketSide.Error += SocketSide_Error;
            socketSide.Connected += SocketSide_Connected;
            socketSide.Disconnected += SocketSide_Disconnected;

            OnUserJoined(socketSide.ToString());

            // Запуск поток для синхронной связи по сокету
            Thread myThread = new Thread(NetThread) { Name = "ServerNetwork" };
            myThread.Start(socketSide);

            // Запуск ожидание следующего клиента
            try
            {
                socket.BeginAccept(new AsyncCallback(AcceptCallback), socket);
            }
            catch (Exception e)
            {
                OnError(new ErrorEventArgs(e));
            }
        }

        /// <summary>
        /// Сетевой поток, работает покуда имеется связь
        /// </summary>
        private void NetThread(object obj)
        {
            try
            {
                SocketSide socketClient = obj as SocketSide;

                socketClient.Connect();
            }
            catch(Exception e)
            {
                SocketSide_Error(obj, new ErrorEventArgs(e));
            }
        }

        #region SocketSide

        private void SocketSide_Connected(object sender, EventArgs e)
            => clients.Add((SocketSide)sender);

        private void SocketSide_Disconnected(object sender, StringEventArgs e)
            => DisconnectHandler((SocketSide)sender, e.Text);

        private void SocketSide_Error(object sender, ErrorEventArgs e)
        {
            DisconnectHandler((SocketSide)sender, e.GetException().Message);
            OnWarningString(e.GetException().Message);
        }
            
        private void SocketSide_ReceivePacket(object sender, PacketBufferEventArgs e)
            => OnReceivePacket(e);

        #endregion
        
        /// <summary>
        /// Разрываем соединение с текущим обработчиком
        /// </summary>
        private void DisconnectHandler(SocketSide socketClient, string text)
        {
            OnUserLeft(socketClient.ToString(), text);
            clients.Remove(socketClient);
            socketClient.DisconnectFromServer();
        }

        /// <summary>
        /// Отправить пакет
        /// </summary>
        public void SendPacket(SocketSide socketClient, byte[] bytes) 
            => socketClient.SendPacket(bytes);

        /// <summary>
        /// Количество сокет клиентов
        /// </summary>
        public int SocketCount() => clients.Count;

        /// <summary>
        /// Получить всех клиентов
        /// </summary>
        public SocketSide[] GetSocketClients()
        {
            //TODO::2024-08-26 SocketSide[] временно
            return clients.ToArray();
        }

        #region Event

        /// <summary>
        /// Событие пользователь присоединился
        /// </summary>
        public event StringEventHandler UserJoined;
        /// <summary>
        /// Событие пользователь присоединился
        /// </summary>
        private void OnUserJoined(string text)
            => UserJoined?.Invoke(this, new StringEventArgs(text));

        /// <summary>
        /// Событие пользователь вышел
        /// </summary>
        public event StringEventHandler UserLeft;
        /// <summary>
        /// Событие пользователь вышел
        /// </summary>
        private void OnUserLeft(string ipName, string text)
            => UserLeft?.Invoke(this, new StringEventArgs(ipName, text));

        /// <summary>
        /// Событие предупреждающая строка
        /// </summary>
        public event StringEventHandler WarningString;
        /// <summary>
        /// Событие предупреждающая строка
        /// </summary>
        private void OnWarningString(string text)
            => WarningString?.Invoke(this, new StringEventArgs(text));

        /// <summary>
        /// Событие, запущен
        /// </summary>
        public event EventHandler Runned;
        /// <summary>
        /// Событие запущен
        /// </summary>
        private void OnRunned() => Runned?.Invoke(this, new EventArgs());

        /// <summary>
        /// Событие, остановлен
        /// </summary>
        public event EventHandler Stopped;
        /// <summary>
        /// Событие остановлен
        /// </summary>
        private void OnStopped() => Stopped?.Invoke(this, new EventArgs());

        /// <summary>
        /// Событие, ошибка
        /// </summary>
        public event ErrorEventHandler Error;
        /// <summary>
        /// Событие ошибки
        /// </summary>
        private void OnError(ErrorEventArgs e) => Error?.Invoke(this, e);

        /// <summary>
        /// Событие, получать пакет
        /// </summary>
        public event PacketBufferEventHandler ReceivePacket;
        /// <summary>
        /// Событие получать пакет
        /// </summary>
        private void OnReceivePacket(PacketBufferEventArgs e) => ReceivePacket?.Invoke(this, e);

        #endregion
    }
}
