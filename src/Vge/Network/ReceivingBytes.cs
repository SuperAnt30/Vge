using System;
using System.Net.Sockets;

namespace Vge.Network
{
    /// <summary>
    /// Объект склейки и возрат при получении полного пакета
    /// </summary>
    public class ReceivingBytes : SocketWork
    {
        /// <summary>
        /// Массив байт сборки пакета
        /// </summary>
        private byte[] bytesCache = new byte[0];
        /// <summary>
        /// Массив байт если остаток пакета меньше 5 байт
        /// </summary>
        private byte[] bytesCache5  = new byte[0];
        /// <summary>
        /// Длинна пакета 
        /// </summary>
        private int bodyLength = 0;
        /// <summary>
        /// Длинна фактическая пакета 
        /// </summary>
        private int bodyFactLength = 0;

        private int indexRun2;

        public ReceivingBytes(Socket workSocket) : base(workSocket) { }

        /// <summary>
        /// Добавить пакет данных
        /// </summary>
        public void Receiving(byte[] dataPacket)
        {
            try
            {
                // с какого индекса начинаем
                int indexRun = 0;

                if (bodyLength == 0)
                {
                    if (bytesCache5.Length > 0)
                    {
                        // склейка двух массивов BytesCache5 + dataPacket 
                        dataPacket = JoinAr(bytesCache5, dataPacket);
                        bytesCache5 = new byte[0];
                    }

                    if (dataPacket[0] == 1)
                    {
                        // Начало пакета

                        // длинна пакета
                        bodyLength = BitConverter.ToInt32(dataPacket, 1);

                        // устанавливаем индекс
                        indexRun = 5;

                        bytesCache = new byte[bodyLength];
                        bodyFactLength = 0;
                    }
                    else if (dataPacket[0] == 0)
                    {
                        // Пришел пакет от сервера, надо разорвать связь
                        ServerPacket sp2 = new ServerPacket(WorkSocket, StatusNet.Disconnecting);
                        OnReceive(new ServerPacketEventArgs(sp2));
                        return;
                    }
                    else
                    {
                        throw new Exception("Ошибка в склейке данных [ReceivingBytes:Receiving]");
                    }
                }
                // Определяем тикущую длинну пакета
                int lengthPacket = dataPacket.Length - indexRun;
                // Общая длинна с фактическим паетом
                int length = lengthPacket + bodyFactLength;
                // Если общая длинна больше нужной
                if (length > bodyLength)
                {
                    // Меняем длину пакета с учётом нужного
                    lengthPacket -= (length - bodyLength);
                    length = bodyLength;
                }
                // Объеденяем массив байт пакетов
                if (indexRun == 0)
                {
                    int begin = bodyFactLength;

                    for (int i = 0; i < lengthPacket; i++)
                    {
                        bytesCache[i + begin] = dataPacket[i];
                    }
                }
                else
                {
                    byte[] vs = DivisionAr(dataPacket, indexRun, lengthPacket);
                    for (int i = 0; i < vs.Length; i++)
                    {
                        bytesCache[i] = vs[i];
                    }
                }
                // Добавляем длинну
                bodyFactLength += lengthPacket;

                // Закончен основной пакет
                ServerPacket sp = new ServerPacket(WorkSocket, bytesCache, bodyFactLength);

                // Финиш сборки паета
                if (bodyFactLength == bodyLength)
                {
                    // Отправляем событие получения
                    OnReceive(new ServerPacketEventArgs(sp));

                    // Обнуляем глобальные переменные
                    bytesCache = new byte[0];
                    bodyLength = 0;

                    indexRun2 = lengthPacket + indexRun;
                    if (indexRun2 < dataPacket.Length)
                    {
                        // не прерывный пакет
                        byte[] vs = DivisionAr(dataPacket, indexRun2, dataPacket.Length - indexRun2);
                        if (vs.Length < 5)
                        {
                            // Если остался хвостик фиксируем для след пакета
                            bytesCache5 = vs;
                        }
                        else
                        {
                            // Обрабатываем следующий пакет
                            Receiving(vs);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
                // исключение намекает на разрыв соединения
            }
        }

        /// <summary>
        /// Вернуть массив байт для отправки сообщения
        /// </summary>
        public static byte[] BytesSender(byte[] bytes)
        {
            byte[] ret = new byte[bytes.Length + 5];
            Buffer.BlockCopy(new byte[] { 1 }, 0, ret, 0, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(bytes.Length), 0, ret, 1, 4);
            Buffer.BlockCopy(bytes, 0, ret, 5, bytes.Length);
            return ret;
        }

        /// <summary>
        /// Событие, получать
        /// </summary>
        public event ServerPacketEventHandler Receive;
        /// <summary>
        /// Событие получать
        /// </summary>
        private void OnReceive(ServerPacketEventArgs e) => Receive?.Invoke(this, e);

        /// <summary>
        /// Разделить часть массива
        /// </summary>
        public static byte[] DivisionAr(byte[] first, int indexStart, int indexLength)
        {
            byte[] ret = new byte[indexLength];
            Array.Copy(first, indexStart, ret, 0, indexLength);
            return ret;
        }

        /// <summary>
        /// Объеденить два массива
        /// </summary>
        private byte[] JoinAr(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            first.CopyTo(ret, 0);
            second.CopyTo(ret, first.Length);
            return ret;
        }
    }
}
