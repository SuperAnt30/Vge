﻿using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using Vge.Event;
using Vge.Network;
using WinGL.Util;

namespace Vge.Games
{
    public class Server
    {
        /// <summary>
        /// Количество тактов в секунду, в minecraft 20
        /// </summary>
        private const int speedTps = 20;
        /// <summary>
        /// Время в мс на такт, в minecraft 50
        /// </summary>
        private const int speedMs = 1000 / speedTps;
        

        /// <summary>
        /// Увеличивается каждый тик 
        /// </summary>
        public uint TickCounter { get; protected set; }

        /// <summary>
        /// Часы для Tps
        /// </summary>
        private readonly Stopwatch stopwatchTps = new Stopwatch();
        /// <summary>
        /// Для перевода тактов в мили секунды Stopwatch.Frequency / 1000;
        /// </summary>
        private readonly long frequencyMs;
        /// <summary>
        /// Указывает, запущен сервер или нет. Установите значение false, 
        /// чтобы инициировать завершение работы. 
        /// </summary>
        private bool serverRunning = true;
        /// <summary>
        /// Устанавливается при появлении предупреждения «Не могу угнаться», 
        /// которое срабатывает снова через 15 секунд. 
        /// </summary>
        private long timeOfLastWarning;
        /// <summary>
        /// Хранение тактов за 1/5 секунды игры, для статистики определения среднего времени такта
        /// </summary>
        private long[] tickTimeArray = new long[4];
        /// <summary>
        /// Пауза в игре, только для одиночной версии
        /// </summary>
        private bool isGamePaused = false;
        /// <summary>
        /// Разница времени для такта
        /// </summary>
        private long differenceTime;
        /// <summary>
        /// Нчальное время такта
        /// </summary>
        private long beginTime;

        /// <summary>
        /// Принято пакетов за секунду
        /// </summary>
        private int rx = 0;
        /// <summary>
        /// Передано пакетов за секунду
        /// </summary>
        private int tx = 0;
        /// <summary>
        /// Принято пакетов за предыдущую секунду
        /// </summary>
        private int rxPrev = 0;
        /// <summary>
        /// Передано пакетов за предыдущую секунду
        /// </summary>
        private int txPrev = 0;
        /// <summary>
        /// статус запуска сервера
        /// </summary>
        private string strNet = "";
        /// <summary>
        /// Сокет для сети
        /// </summary>
        private SocketServer socketServer;
        /// <summary>
        /// Объект работы с пакетами
        /// </summary>
       // private ProcessServerPackets packets;

        public Server()
        {
            frequencyMs = Stopwatch.Frequency / 1000;
            stopwatchTps.Start();
        }

        #region StartStopPause

        /// <summary>
        /// Запустить сервер в отдельном потоке
        /// </summary>
        public void Starting()
        {
            Thread myThread = new Thread(Loop);
            myThread.Start();
        }

        /// <summary>
        /// Запрос остановки сервера
        /// </summary>
        public void Stop() => serverRunning = false;

        /// <summary>
        /// Задать паузу для одиночной игры
        /// </summary>
        public bool SetGamePauseSingle(bool value)
        {
            isGamePaused = !IsRunNet() && value;
            OnTextDebug();
            return isGamePaused;
        }

        #endregion

        #region Net

        /// <summary>
        /// Получить истину запущена ли сеть
        /// </summary>
        public bool IsRunNet() => socketServer != null && socketServer.IsConnected;

        /// <summary>
        /// Запустить на сервере сеть
        /// </summary>
        public void RunNet(int port)
        {
            if (!IsRunNet())
            {
                socketServer = new SocketServer(port);
                socketServer.ReceivePacket += SocketServer_ReceivePacket;
                socketServer.Receive += SocketServer_Receive;
                socketServer.Stopped += SocketServer_Stopped;
                socketServer.Runned += SocketServer_Runned;
                socketServer.Error += SocketServer_Error;
                socketServer.Run();
            }
        }

        private void SocketServer_Error(object sender, System.IO.ErrorEventArgs e)
        {
            OnError(e.GetException());
        }

        private void SocketServer_Runned(object sender, EventArgs e)
        {
            isGamePaused = false;
            // Log.Log("server.run.net");
        }

        private void SocketServer_Stopped(object sender, EventArgs e)
        {
            socketServer = null;
            if (serverRunning)
            {
                // Если сеть остановилась, а луп нет, 
                // запускаем остановку лупа и последующее завершение
                Stop();
            }
            else
            {
                // Если луп уже остановлен, то сразу к закрытию
                Stoped();
            }
        }

        private void SocketServer_Receive(object sender, ServerPacketEventArgs e)
        {
            if (e.Packet.status == StatusNet.Disconnect)
            {
               // World.Players.PlayerRemove(e.Packet.WorkSocket);
            }
            else if (e.Packet.status == StatusNet.Connect)
            {
                // Отправляем игроку пинг
                //ResponsePacket2(e.Packet.WorkSocket, new PacketSF0Connection(""));
            }
        }

        private void SocketServer_ReceivePacket(object sender, ServerPacketEventArgs e)
        {
            LocalReceivePacket(e.Packet.workSocket, e.Packet.bytes);
        }

        /// <summary>
        /// Локальная передача пакета
        /// </summary>
        public void LocalReceivePacket(Socket socket, byte[] buffer)
        {
            rx++;
           // packets.ReceiveBuffer(socket, buffer);
        }

        /// <summary>
        /// Обновить количество клиентов
        /// </summary>
        public void UpCountClients() => strNet = IsRunNet() ? "net[" + socketServer.SocketCount() + "]" : "";

        /// <summary>
        /// Отправить пакет клиенту
        /// </summary>
        public void ResponsePacket(Socket socket, IPacket packet)
        {
            //using (MemoryStream writeStream = new MemoryStream())
            //{
            //    using (StreamBase stream = new StreamBase(writeStream))
            //    {
            //        writeStream.WriteByte(ProcessPackets.GetId(packet));
            //        packet.WritePacket(stream);
            //        byte[] buffer = writeStream.ToArray();
            //        tx++;
            //        ServerPacket spacket = new ServerPacket(socket, buffer);
            //        if (socket != null)
            //        {
            //            server.SendPacket(socket, buffer);
            //        }
            //        else
            //        {
            //            OnRecievePacket(new ServerPacketEventArgs(spacket));
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Отправить пакет всем клиентам
        /// </summary>
        public void ResponsePacketAll(IPacket packet)
        {
            //if (World != null)
            //{
            //    World.Players.SendToAll(packet);
            //}
        }

        /// <summary>
        /// Разорвать соединение с игроком по сокету
        /// </summary>
        public void DisconnectPlayer(Socket socket) => socketServer.DisconnectPlayer(socket);

        #endregion

        #region Loop & Tick

        /// <summary>
        /// До игрового цикла
        /// </summary>
        private void ToLoop()
        {
            // Запуск игрока
            // Запуск чанков 
        }

        /// <summary>
        /// Метод должен работать в отдельном потоке
        /// </summary>
        private void Loop()
        {
            try
            {
                ToLoop();
                long currentTime = stopwatchTps.ElapsedMilliseconds;
                long cacheTime = 0;
                int timeSleep;
                //Log.Log("server.runed");

                // Рабочий цикл сервера
                while (serverRunning)
                {
                    beginTime = stopwatchTps.ElapsedMilliseconds;
                    // Разница в милсекунда с прошлого такта
                    differenceTime = beginTime - currentTime;
                    if (differenceTime < 0) differenceTime = 0;

                    // Если выше 2 секунд задержка
                    if (differenceTime > 2000 && currentTime - timeOfLastWarning >= 15000)
                    {
                        // Не успеваю!Изменилось ли системное время, или сервер перегружен?
                        // Отставание на {differenceTime} мс, пропуск тиков({differenceTime / 50}) 
                        //Log.Log("Не успеваю! Отставание на {0} мс, пропуск тиков {1}", differenceTime, differenceTime / 50);
                        differenceTime = 2000;
                        timeOfLastWarning = currentTime;
                    }

                    cacheTime += differenceTime;
                    currentTime = beginTime;

                    while (cacheTime > speedMs)
                    {
                        cacheTime -= speedMs;
                        if (!isGamePaused) OnTick();
                    }
                    timeSleep = speedMs - (int)cacheTime;
                    Thread.Sleep(timeSleep > 0 ? timeSleep : 1);
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            finally
            {
                Stoping();
            }
        }

        /// <summary>
        /// Сохраняет все необходимые данные для подготовки к остановке сервера
        /// </summary>
        private void Stoping()
        {
            //Log.Log("server.stoping");
            //World.WorldStoping();
            // Тут надо сохранить мир
            //packets.Clear();
            if (socketServer != null)
            {
                socketServer.Stop();
            }
            else
            {
                Stoped();
            }
        }

        /// <summary>
        /// Закрыт луп и сокет
        /// </summary>
        private void Stoped()
        {
            //Log.Log("server.stoped");
            //Log.Close();
            OnCloseded();
        }

        /// <summary>
        /// Игровой тик
        /// </summary>
        private void OnTick()
        {
            beginTime = stopwatchTps.ElapsedTicks;
            TickCounter++;

            // Прошла секунда
            if (TickCounter % speedTps == 0)
            {
                UpCountClients();

                if (TickCounter % 600 == 0)
                {
                    // раз в 30 секунд обновляем тик с клиентом
                  //  ResponsePacketAll(new PacketS03TimeUpdate(TickCounter));
                }
            }

            Thread.Sleep(10);
            // Тут игровые мировые тики

            differenceTime = stopwatchTps.ElapsedTicks - beginTime;

            // Прошла 1/5 секунда, или 4 такта
            if (TickCounter % 4 == 0)
            {
                // лог статистика за это время
                OnTextDebug();
            }

            // фиксируем время выполнения такта
            tickTimeArray[TickCounter % 4] = differenceTime;
        }

        #endregion

        /// <summary>
        /// Строка для дебага, формируется по запросу
        /// </summary>
        private string ToStringDebugTps()
        {
            // Среднее время выполнения 4 тактов, должно быть меньше 50
            float averageTime = Mth.Average(tickTimeArray) / frequencyMs;
            // TPS за последние 4 тактов (1/5 сек), должен быть 20
            float tps = averageTime > speedMs ? speedMs / averageTime * speedTps : speedTps;
            return string.Format("{0:0.00} tps {1:0.00} ms Rx {2} Tx {3} {4}{5}",
                tps, averageTime, rxPrev, txPrev, strNet, isGamePaused ? " PAUSE" : "");
        }

        #region Event

        /// <summary>
        /// Событие ошибки на сервере
        /// </summary>
        public event ThreadExceptionEventHandler Error;
        private void OnError(Exception ex)
            => Error?.Invoke(this, new ThreadExceptionEventArgs(ex));

        /// <summary>
        /// Событие закрыть
        /// </summary>
        public event EventHandler Closeded;
        private void OnCloseded() => Closeded?.Invoke(this, new EventArgs());

        /// <summary>
        /// Событие текст для отладки
        /// </summary>
        public event StringEventHandler TextDebug;
        private void OnTextDebug() 
            => TextDebug?.Invoke(this, new StringEventArgs(ToStringDebugTps()));

        /// <summary>
        /// Событие получить от сервера пакет
        /// </summary>
        public event ServerPacketEventHandler RecievePacket;
        private void OnRecievePacket(ServerPacketEventArgs e) 
            => RecievePacket?.Invoke(this, e);

        #endregion
    }
}
