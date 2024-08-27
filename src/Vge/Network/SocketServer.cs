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
    public class SocketServer : SocketBase
    {
        /// <summary>
        /// Получить истину есть ли соединение
        /// </summary>
        public override bool IsConnected => WorkSocket != null;
        /// <summary>
        /// Колекция сокетов клиентов
        /// </summary>
        private List<Socket> clients = new List<Socket>();

        public SocketServer(int port) : base(port) { }

        #region Runing

        /// <summary>
        /// Начинаем слушать входящие соединения
        /// </summary>
        public bool Run()
        {
            if (IsConnected) return false;

            try
            {
                // очистили список клиентов
                clients.Clear();

                // Создание сокета сервера
                WorkSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Связываем сокет с конечной точкой
                WorkSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
                // Начинаем слушать входящие соединения
                WorkSocket.Listen(10);

                // Запуск ожидание клиента
                WorkSocket.BeginAccept(new AsyncCallback(AcceptCallback), WorkSocket);

                OnRunned();
            }
            catch (Exception e)
            {
                WorkSocket = null;
                // TODO::2024-08-27 надо продумать ошибку если сеть не открылась
                OnError(new ErrorEventArgs(e));
                OnStopped();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Остановить сервер
        /// </summary>
        public bool Stop()
        {
            if (IsConnected)
            {
                try
                {
                    if (clients.Count > 0)
                    {
                        for (int i = clients.Count - 1; i >= 0; i--)
                        {
                            DisconnectHandler(clients[i], StatusNet.Disconnecting);
                        }
                    }
                    WorkSocket.Close();
                    WorkSocket = null;

                    return true;
                }
                catch (Exception e)
                {
                    OnError(new ErrorEventArgs(e));
                    return false;
                }
            }
            else
            {
                OnStopped();
            }
            return false;
        }

        #endregion

        /// <summary>
        /// Ждём покуда не получим клиента
        /// </summary>
        private void AcceptCallback(IAsyncResult ar)
        {
            // обрабатываемый сокет
            Socket socket;

            try
            {
                socket = WorkSocket.EndAccept(ar);
            }
            catch (Exception e)
            {
                if (!IsConnected)
                {
                    OnStopped();
                }
                else
                {
                    OnError(new ErrorEventArgs(e));
                }
                return;
            }

            // Добавляем в массив клиентов
            clients.Add(socket);

            // События соединения клиента
            if (socket.RemoteEndPoint is IPEndPoint endPoint)
            {
                OnUserConnected(endPoint.ToString());
            }

            // Создаём объекты склейки и присваиваем ему событие
            ReceivingBytes rb = new ReceivingBytes(socket);
            rb.Receive += RbReceive;

            // Получили клиента, оповещаем
            ServerPacket sp = new ServerPacket(socket, StatusNet.Connect);
            OnReceive(new ServerPacketEventArgs(sp));

            // Запуск ожидание ответа от клиента
            Thread myThread = new Thread(ReceiveBuff);
            myThread.Start(rb);

            // Запуск ожидание следующего клиента
            try
            {
                WorkSocket.BeginAccept(new AsyncCallback(AcceptCallback), WorkSocket);
            }
            catch (Exception e)
            {
                OnError(new ErrorEventArgs(e));
            }
        }

        /// <summary>
        /// Получить буфер по сети
        /// </summary>
        private void ReceiveBuff(object obj)
        {
            ReceivingBytes rb = obj as ReceivingBytes;
            if (rb == null)
            {
                OnError(new ErrorEventArgs(new Exception("Отсутствует объект склейки для данного сокета [ServerSocket:ReceivingBytes]")));
                return;
            }
            
            Socket socket = rb.WorkSocket;
            // Чтение данных из клиентского сокета. 
            int bytesRead = 0;
            try
            {
                bytesRead = socket.Receive(buff);
            }
            catch
            {
                // Затычка, если сеть будет разорвана
            }

            try
            {
                if (bytesRead > 0)
                {
                    // Если длинны данный больше 0, то обрабатываем данные
                    rb.Receiving(ReceivingBytes.DivisionAr(buff, 0, bytesRead));

                    // Запуск ожидание следующего ответа от клиента
                    if (socket != null && socket.Connected)
                    {
                        ReceiveBuff(rb);
                    }
                }
                else
                {
                    // Если данные отсутствуют, то разрываем связь
                    DisconnectHandler(socket, StatusNet.Disconnect);
                }
            }
            catch (Exception e)
            {
                // Возвращаем ошибку
                OnError(new ErrorEventArgs(e));
            }
        }
        
        /// <summary>
        /// Разорвать соединение с игроком по сокету
        /// </summary>
        public void DisconnectPlayer(Socket socket) => DisconnectHandler(socket, StatusNet.Disconnecting);

        /// <summary>
        /// Разрываем соединение с текущим обработчиком
        /// </summary>
        private void DisconnectHandler(Socket socket, StatusNet status)
        {
            ServerPacket sp = new ServerPacket(socket, status);
            clients.Remove(socket);
            try { socket.Send(new byte[] { 0 }); } catch { } // защита от вылета сервера
            OnReceive(new ServerPacketEventArgs(sp));
        }

        /// <summary>
        /// Отправить пакет
        /// </summary>
        public void SendPacket(Socket socket, byte[] bytes) => SenderOld(socket, bytes);

        /// <summary>
        /// Количество сокет клиентов
        /// </summary>
        public int SocketCount() => clients.Count;

        /// <summary>
        /// Получить всех клиентов
        /// </summary>
        public Socket[] GetSocketClients()
        {
            //TODO::2024-08-26 Socket[] временно
            return clients.ToArray();
        }

        #region Event

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
        /// Событие пользователь соединён
        /// </summary>
        public event StringEventHandler UserConnected;
        protected virtual void OnUserConnected(string text)
            => UserConnected?.Invoke(this, new StringEventArgs(text));

        #endregion
    }
}
