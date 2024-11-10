using System;

namespace Vge.Network
{
    /// <summary>
    /// Объект склейки и возрат при получении полного пакета
    /// </summary>
    public class ReceivingBytes
    {
        /// <summary>
        /// Массив байт сборки пакета
        /// </summary>
        private byte[] bytesCache;
        /// <summary>
        /// Массив байт если остаток пакета меньше 5 байт
        /// </summary>
        private byte[] bytesCache5;
        /// <summary>
        /// Длинна пакета 
        /// </summary>
        private int bodyLength = 0;
        /// <summary>
        /// Длинна фактическая пакета 
        /// </summary>
        private int bodyFactLength = 0;

        private int indexRun2;

        /// <summary>
        /// Добавить пакет данных
        /// </summary>
        public void Receiving(byte[] dataPacket)
        {
            // с какого индекса начинаем
            byte indexRun = 0;

            if (bodyLength == 0)
            {
                if (bytesCache5 != null)
                {
                    // склейка двух массивов BytesCache5 + dataPacket 
                    dataPacket = JoinAr(bytesCache5, dataPacket);
                    bytesCache5 = null;
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
                else
                {
                    throw new Exception(Sr.ErrorInGluingNetworkData);
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

            // Финиш сборки паета
            if (bodyFactLength == bodyLength)
            {
                // Отправляем событие получения
                OnReceive(new PacketBufferEventArgs(bytesCache, bodyFactLength));

                // Обнуляем глобальные переменные
                bytesCache = null;
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

        #region Event

        /// <summary>
        /// Событие, получать
        /// </summary>
        public event PacketBufferEventHandler Receive;
        /// <summary>
        /// Событие получать
        /// </summary>
        private void OnReceive(PacketBufferEventArgs e) => Receive?.Invoke(this, e);

        #endregion
    }
}
