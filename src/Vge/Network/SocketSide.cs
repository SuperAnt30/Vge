using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Vge.Event;
using Vge.Util;

namespace Vge.Network
{
    /// <summary>
    /// Объект стороны сокета.
    /// Может выступать в роли клиента, или на стороне сервера.
    /// </summary>
    public class SocketSide
    {
        /// <summary>
        /// IP адрес сервера
        /// </summary>
        private readonly IPAddress ip;
        /// <summary>
        /// Порт сервера
        /// </summary>
        private readonly int port;
        /// <summary>
        /// Объект склейки
        /// </summary>
        private readonly ReceivingBytes receivingBytes = new ReceivingBytes();
        /// <summary>
        /// Буфер для принятия данных с сети
        /// </summary>
        private readonly byte[] buff = new byte[1024];
        /// <summary>
        /// Массив очередей пакетов
        /// </summary>
        private readonly DoubleList<byte[]> packets = new DoubleList<byte[]>();

        /// <summary>
        /// Флаг пометки сервер ли сделал разрыв
        /// true - текущая сторона отправила запрос на Disconnect
        /// </summary>
        private bool isServerDisconnect = false;
        /// <summary>
        /// Получить рабочий сокет
        /// </summary>
        private Socket socket;
        /// <summary>
        /// Объект-событие
        /// </summary>
        private AutoResetEvent waitHandler;

        /// <summary>
        /// Создать сокет на стороне клиента
        /// </summary>
        public SocketSide(IPAddress ip, int port)
        {
            this.ip = ip;
            this.port = port;
            receivingBytes.Receive += ReceivingBytes_Receive;
        }

        /// <summary>
        /// Создать сокет на стороне сервера зная уже пришедший сокет
        /// </summary>
        public SocketSide(Socket socket)
        {
            this.socket = socket;
            IPEndPoint iPEndPoint = socket.RemoteEndPoint as IPEndPoint;
            ip = iPEndPoint.Address;
            port = iPEndPoint.Port;
            receivingBytes.Receive += ReceivingBytes_Receive;
        }

        /// <summary>
        /// Запустить сокет
        /// </summary>
        public bool SocketRun()
        {
            try
            {
                if (socket == null)
                {
                    socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                    {
                        NoDelay = Ce.NoDelay
                    };
                    socket.Connect(ip, port);
                    return true;
                }
            }
            catch (SocketException e)
            {
                OnError(new ErrorEventArgs(e));
            }
            return false;
        }

        /// <summary>
        /// Ответ готовности сообщения
        /// </summary>
        private void ReceivingBytes_Receive(object sender, PacketBufferEventArgs e)
            => OnReceivePacket(new PacketBufferEventArgs(e.Bytes, e.Count, this));

        /// <summary>
        /// Имеется ли связь
        /// </summary>
        public bool IsConnect() => socket != null && socket.Connected;

        /// <summary>
        /// Соединяемся к серверу
        /// </summary>
        public void Connect()
        {
            if (socket == null)
            {
                // Соединение отсутствует
                OnError(new ErrorEventArgs(new Exception(Srl.NoConnection)));
                return;
            }

            waitHandler = new AutoResetEvent(true);

            // Запускаем отдельный поток для отправки сообщений
            Thread myThread = new Thread(ThreadLoopSend) { Name = "SoketSide" };
            myThread.Start();

            OnConnected();

            // Ждём ответ
            while (WaitingReceive()) { }
        }

        

        /// <summary>
        /// Отдельный поток цикла для отправки пакетов
        /// </summary>
        private void ThreadLoopSend()
        {
            while (socket != null)
            {
                try
                {
                    if (!packets.Empty())
                    {
                        packets.Step();
                        int count = packets.CountBackward;
                        for (int i = 0; i < count; i++)
                        {
                            socket.Send(packets.GetNext());
                        }
                    }
                }
                catch
                {
                    // Сюда можем попасть в момент когда сокет закрылся в другом потоке
                }
                // Ожидаем сигнала
                waitHandler.WaitOne();
            }
        }

        /// <summary>
        /// Отправить пакет
        /// </summary>
        public void SendPacket(byte[] bytes)
        {
            // Этот метод должен отправлять пакеты в отдельном патоке
            if (bytes.Length > 0)
            {
                // Отправляем пакет
                byte[] ret = new byte[bytes.Length + 5];
                ret[0] = 1;
                Buffer.BlockCopy(BitConverter.GetBytes(bytes.Length), 0, ret, 1, 4);
                Buffer.BlockCopy(bytes, 0, ret, 5, bytes.Length);
                packets.Add(ret);
                // Сигнализируем, что waitHandler в сигнальном состоянии
                waitHandler.Set();
            }
        }

        /// <summary>
        /// Отправить пакет
        /// </summary>
        public void SendPacket(IPacket packet)
        {
            if (IsConnect())
            {
                WritePacket writePacket = new WritePacket(packet);
                SendPacket(writePacket.ToArray());
            }
        }

        /// <summary>
        /// Сервер отправляет разрыв связи, причина указывается ранее
        /// </summary>
        public void DisconnectFromServer()
        {
            try
            {
                isServerDisconnect = true;
                Disconnect();
            }
            catch
            {
                // защита от вылета сервера, этот разрыв не всегда нужен,
                // так-как уже сокет может быть разорван
            }
        }

        /// <summary>
        /// Клиент отправляет разрыв связи, указывая причину в text
        /// </summary>
        public void DisconnectFromClient(string text)
        {
            try
            {
                Disconnect();
                if (!isServerDisconnect)
                {
                    // Сюда заходим только если разрыв был со стороны клиента
                    OnDisconnected(text);
                }
            }
            catch (Exception e)
            {
                OnError(new ErrorEventArgs(e));
            }
        }

        private void Disconnect()
        {
            // Разорвали связь
            if (socket != null)
            {
                if (socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Disconnect(false);
                    if (socket != null) socket.Close();
                }
                socket = null;
                if (waitHandler != null)
                {
                    waitHandler.Set();
                }
            }
        }

        /// <summary>
        /// Ждём ответ
        /// </summary>
        private bool WaitingReceive()
        {
            // Чтение данных из клиентского сокета. 
            int bytesRead;
            try
            {
                bytesRead = socket.Receive(buff);
            }
            catch
            {
                // Это сработать может когда связь разорвалась
                bytesRead = 0;
            }

            try
            {
                if (bytesRead > 0)
                {
                    // Если длинны данный больше 0, то обрабатываем данные
                    receivingBytes.Receiving(ReceivingBytes.DivisionAr(buff, 0, bytesRead));

                    // Запуск ожидание следующего ответа от клиента
                    if (socket != null && socket.Connected)
                    {
                        return true;
                    }
                }
                else
                {
                    // Если данные отсутствуют, то разрываем связь
                    // Сюда попадаем если обратная сторона разорвала связь 
                    DisconnectFromClient(Srl.TheConnectionIsBroken);
                }
            }
            catch (Exception e)
            {
                // Для лога, имя ошибки
                DisconnectFromClient(Srl.GetString(Srl.TheConnectionWasBrokenDueToAnError, e.Message));
                // Подробную инфу ошибки в краш файл
                Logger.Crash(e, ToString());
            }
            return false;
        }

        public override string ToString() => ip.ToString() + ":" + port;

        #region Event

        /// <summary>
        /// Событие, запущен
        /// </summary>
        public event EventHandler Connected;
        /// <summary>
        /// Событие запущен
        /// </summary>
        protected void OnConnected() => Connected?.Invoke(this, new EventArgs());

        /// <summary>
        /// Событие, остановлен
        /// </summary>
        public event StringEventHandler Disconnected;
        /// <summary>
        /// Событие остановлен
        /// </summary>
        protected void OnDisconnected(string text) => Disconnected?.Invoke(this, new StringEventArgs(text));

        /// <summary>
        /// Событие ошибка, всегда должно разрывать соединение по итогу
        /// </summary>
        public event ErrorEventHandler Error;
        /// <summary>
        /// Событие ошибки, всегда должно разрывать соединение по итогу
        /// </summary>
        protected void OnError(ErrorEventArgs e) => Error?.Invoke(this, e);

        /// <summary>
        /// Событие, получать пакет
        /// </summary>
        public event PacketBufferEventHandler ReceivePacket;
        /// <summary>
        /// Событие получать пакет
        /// </summary>
        protected void OnReceivePacket(PacketBufferEventArgs e) => ReceivePacket?.Invoke(this, e);

        #endregion
    }
}
