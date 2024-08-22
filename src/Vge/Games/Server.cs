using System;
using System.Diagnostics;
using System.Threading;
using Vge.Util;
using WinGL.Util;

namespace Vge.Games
{
    public class Server
    {
        /// <summary>
        /// Время в мс на такт, в minecraft 50
        /// speedMs * speedTps = 1000
        /// </summary>
        private const int speedMs = 50;
        /// <summary>
        /// Количество тактов в секунду, в minecraft 20
        /// speedMs * speedTps = 1000
        /// </summary>
        private const int speedTps = 20;

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
        public void Stoping() => serverRunning = false;

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
        public bool IsRunNet() => false;

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
                Close();
            }
        }

        /// <summary>
        /// Сохраняет все необходимые данные для подготовки к остановке сервера
        /// </summary>
        private void Close()
        {
            //Log.Log("server.stoping");
            //World.WorldStoping();
            // Тут надо сохранить мир
            //packets.Clear();
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
            return string.Format("{0:0.00} tps {1:0.00} ms Rx {2} Tx {3} {4}",
                tps, averageTime, rxPrev, txPrev, isGamePaused ? "PAUSE" : "");
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
        protected virtual void OnTextDebug() 
            => TextDebug?.Invoke(this, new StringEventArgs(ToStringDebugTps()));

        #endregion
    }
}
