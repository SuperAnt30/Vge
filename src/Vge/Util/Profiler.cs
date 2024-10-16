using System.Diagnostics;

namespace Vge.Util
{
    /// <summary>
    /// Объект сыщик, замеряющий и фиксирующий медленный код
    /// </summary>
    public class Profiler
    {

#if DEBUG
        private const int _stepTime = 5;
        public const int StepTime = 25;
#else
        private const int _stepTime = 50;
        public const int StepTime = 60;
#endif
        /// <summary>
        /// Объект лога
        /// </summary>
        private readonly Logger _log;
        /// <summary>
        /// Получает частоту таймера в виде количества тактов в милисекунду
        /// </summary>
        private readonly long _timerFrequency;
        /// <summary>
        /// Оглавление перед сообщением
        /// </summary>
        private readonly string _prefix;

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private string _profilingSection;
        private bool _profilingEnabled = false;

        public Profiler(Logger log, string prefix = "")
        {
            _log = log;
            _prefix = prefix;
            _timerFrequency = Stopwatch.Frequency / 1000;
            _stopwatch.Start();
        }

        /// <summary>
        /// Начало секции
        /// </summary>
        /// <param name="name">Название секции</param>
        public void StartSection(string name)
        {
            _profilingSection = name;
            _profilingEnabled = true;
            _stopwatch.Restart();
        }

        /// <summary>
        /// Закрыть проверку по времени
        /// </summary>
        public void EndSection(int stepTime = _stepTime)
        {
            if (_profilingEnabled)
            {
                _profilingEnabled = false;
                long time = _stopwatch.ElapsedMilliseconds;
                if (time > stepTime)
                {
                    _log.Log(Srl.SomethingIsTooLong, _prefix, _profilingSection, time);
                }
            }
        }

        /// <summary>
        /// Закрыть проверку по времени и указать обязательно время в мс 0.00
        /// </summary>
        public void EndSectionLog()
        {
            if (_profilingEnabled)
            {
                _profilingEnabled = false;
                _log.Log(Srl.EndSectionTime, _prefix,
                    _profilingSection, _stopwatch.ElapsedTicks / (float)_timerFrequency);
            }
        }

        /// <summary>
        /// Закрыть секцию и тут же открыть
        /// </summary>
        /// <param name="name">Название секции</param>
        public void EndStartSection(string name, int stepTime = _stepTime)
        {
            EndSection(stepTime);
            StartSection(name);
        }
    }
}
