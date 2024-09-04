using System.Diagnostics;

namespace Vge.Util
{
    /// <summary>
    /// Объект сыщик, замеряющий и фиксирующий медленный код
    /// </summary>
    public class Profiler
    {
        /// <summary>
        /// Объект лога
        /// </summary>
        private readonly Logger log;
        /// <summary>
        /// Получает частоту таймера в виде количества тактов в милисекунду
        /// </summary>
        private readonly long timerFrequency;
        /// <summary>
        /// Оглавление перед сообщением
        /// </summary>
        private readonly string prefix;

        private readonly Stopwatch stopwatch = new Stopwatch();
        private string profilingSection;
        private bool profilingEnabled = false;

        public Profiler(Logger log, string prefix = "")
        {
            this.log = log;
            this.prefix = prefix;
            timerFrequency = Stopwatch.Frequency / 1000;
            stopwatch.Start();
        }

        /// <summary>
        /// Начало секции
        /// </summary>
        /// <param name="name">Название секции</param>
        public void StartSection(string name)
        {
            profilingSection = name;
            profilingEnabled = true;
            stopwatch.Restart();
        }

        /// <summary>
        /// Закрыть проверку по времени
        /// </summary>
        public void EndSection(int stepTime = 20)
        {
            if (profilingEnabled)
            {
                profilingEnabled = false;
                long time = stopwatch.ElapsedTicks / timerFrequency;
                if (time > stepTime)
                {
                    log.Log(SRL.SomethingIsTooLong, prefix, profilingSection, time);
                }
            }
        }

        /// <summary>
        /// Закрыть проверку по времени и указать обязательно время в мс 0.00
        /// </summary>
        public void EndSectionLog()
        {
            if (profilingEnabled)
            {
                profilingEnabled = false;
                log.Log(SRL.EndSectionTime, prefix,
                    profilingSection, stopwatch.ElapsedTicks / (float)timerFrequency);
            }
        }

        /// <summary>
        /// Закрыть секцию и тут же открыть
        /// </summary>
        /// <param name="name">Название секции</param>
        public void EndStartSection(string name)
        {
            EndSection();
            StartSection(name);
        }
    }
}
