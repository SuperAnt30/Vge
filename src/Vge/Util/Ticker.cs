using System;
using System.Diagnostics;
using System.Threading;
using WinGL.Util;

namespace Vge.Util
{
    /// <summary>
    /// Объект клиентской частки, в котором есть один луп, 
    /// но с двумя разделениями, для кадра и для игрового тика
    /// </summary>
    public class Ticker
    {
        /// <summary>
        /// Получает частоту таймера в виде количества тактов в милисекунду
        /// </summary>
        public static long TimerFrequency { get; private set; }

        /// <summary>
        /// Желаемое количество кадров в секунду
        /// </summary>
        public int WishFrame { get; private set; } = 60;
        /// <summary>
        /// Желаемое количество тактов в секунду
        /// </summary>
        public readonly int WishTick = 20;
        /// <summary>
        /// Получить коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1
        /// </summary>
        public float Interpolation { get; private set; } = 0;
        /// <summary>
        /// Запущен ли loop игры
        /// </summary>
        public bool IsRunGameLoop { get; set; } = true;

        private readonly Stopwatch stopwatch = new Stopwatch();
        /// <summary>
        /// Интервал в тиках для такта
        /// </summary>
        private readonly long intervalTick;
        /// <summary>
        /// Интервал в тиках для кадра
        /// </summary>
        private long intervalFrame;
        /// <summary>
        /// Время для сна без нагрузки для кадра
        /// </summary>
        private long sleepFrameDef;
        /// <summary>
        /// Время для сна без нагрузки для такта
        /// </summary>
        private readonly long sleepTickDef;
        /// <summary>
        /// Получает частоту таймера в виде количества тактов в милисекунду
        /// </summary>
        private readonly long timerFrequency;
        /// <summary>
        /// Получает частоту таймера в виде количества тактов в один TPS
        /// </summary>
        private readonly long timerFrequencyTps;
        /// <summary>
        /// Максимальный fps
        /// </summary>
        private bool isMax = false;

        /// <summary>
        /// Последнее время кадра
        /// </summary>
        private long lastTimeFrame;
        /// <summary>
        /// Последнее время тика
        /// </summary>
        private long lastTimeTick;
        /// <summary>
        /// Тикущее время начальной проверки
        /// </summary>
        private long currentTimeBegin;
        /// <summary>
        /// Сколько надо милисекунд для сна кадра
        /// </summary>
        private int sleepFrame;
        /// <summary>
        /// Сколько надо милисекунд для сна тика
        /// </summary>
        private int sleepTick;
        /// <summary>
        /// Время в тактах для следующего тика
        /// </summary>
        private long nextTick;
        /// <summary>
        /// Время в тактах для следующего кадра
        /// </summary>
        private long nextFrame;

        public Ticker(int wishTick = 20)
        {
            WishTick = wishTick;
            timerFrequency = Stopwatch.Frequency / 1000;
            timerFrequencyTps = Stopwatch.Frequency / wishTick;
            intervalTick = timerFrequencyTps;
            sleepTickDef = intervalTick / timerFrequency;

            stopwatch.Start();
            lastTimeFrame = stopwatch.ElapsedTicks;
            lastTimeTick = lastTimeFrame;

            nextTick = lastTimeTick + intervalTick;
            nextFrame = lastTimeFrame + intervalFrame;

            TimerFrequency = Stopwatch.Frequency / 1000;
        }

        /// <summary>
        /// Задать желаемый фпс
        /// </summary>
        public void SetWishFrame(int frame)
        {
            if (frame > 250)
            {
                isMax = true;
            }
            else
            {
                isMax = false;
                WishFrame = frame;
                intervalFrame = Stopwatch.Frequency / WishFrame;
                sleepFrameDef = intervalFrame / timerFrequency;
            }
            ResetTimeFrame();
        }

        /// <summary>
        /// Обнулить время таймя, чтоб не было попытки достичь желаемое с ускорением лимита
        /// </summary>
        public void ResetTimeFrame()
        {
            lastTimeFrame = currentTimeBegin = stopwatch.ElapsedTicks;
            nextFrame = lastTimeFrame + intervalFrame;
        }

        /// <summary>
        /// Метод запуска такта
        /// </summary>
        public void DoTick()
        {
            // Проверяем такт
            currentTimeBegin = stopwatch.ElapsedTicks;
            if (IsRunGameLoop)
            {
                if (currentTimeBegin >= nextTick)
                {
                    lastTimeTick = currentTimeBegin;
                    nextTick += intervalTick;
                    OnTick();
                    Interpolation = 0;
                }
                else
                {
                    Interpolation = (currentTimeBegin - lastTimeTick) / (float)timerFrequencyTps;
                    if (Interpolation > 1f) Interpolation = 1f;
                    if (Interpolation < 0) Interpolation = 0f;
                }
            }
            else
            {
                if (currentTimeBegin >= nextTick)
                {
                    lastTimeTick = currentTimeBegin;
                    nextTick += intervalTick;
                    Interpolation = 0;
                }
            }
            // Проверяем кадр
            if (isMax)
            {
                OnFrame();
            }
            else
            {
                currentTimeBegin = stopwatch.ElapsedTicks;
                if (currentTimeBegin >= nextFrame)
                {
                    lastTimeFrame = currentTimeBegin;
                    nextFrame += intervalFrame;
                    OnFrame();
                }

                // Нужно ли засыпание
                currentTimeBegin = stopwatch.ElapsedTicks;
                sleepTick = nextTick < currentTimeBegin ? 0 : (int)sleepTickDef - Mth.Floor((nextTick - currentTimeBegin) / timerFrequency);
                sleepFrame = nextFrame < currentTimeBegin ? 0 : (int)sleepFrameDef - Mth.Floor((nextFrame - currentTimeBegin) / timerFrequency);

                // Находим на именьшое засыпание и засыпаем
                if (sleepFrame < sleepTick) Thread.Sleep(sleepFrame);
                else Thread.Sleep(sleepTick);
            }
        }

        /// <summary>
        /// Получить время в милисекундах с момента запуска проекта
        /// </summary>
        public long Time() => stopwatch.ElapsedMilliseconds;
        /// <summary>
        /// Получить время в тактах с момента запуска проекта
        /// </summary>
        public long TimeTicks() => stopwatch.ElapsedTicks;

        #region Events

        /// <summary>
        /// Событие одного игрового такта
        /// </summary>
        public event EventHandler Tick;
        private void OnTick() => Tick?.Invoke(this, new EventArgs());

        /// <summary>
        /// Событие кадра
        /// </summary>
        public event EventHandler Frame;
        private void OnFrame() => Frame?.Invoke(this, new EventArgs());

        #endregion
    }
}
