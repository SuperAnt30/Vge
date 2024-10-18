using System;
using System.Threading;

namespace Vge.Util
{
    /// <summary>
    /// Ждать обработчик
    /// </summary>
    public class WaitHandler
    {
        /// <summary>
        /// Флаг выполненого такта
        /// </summary>
        private bool _flagExecution;
        /// <summary>
        /// Запущен ли поток
        /// </summary>
        private bool _isRuning;
        /// <summary>
        /// Объект-событие
        /// </summary>
        private readonly AutoResetEvent _waitHandler = new AutoResetEvent(true);
        /// <summary>
        /// Название потока
        /// </summary>
        private readonly string _nameThread;

        public WaitHandler(string nameThread) => _nameThread = nameThread;

        /// <summary>
        /// Запуск
        /// </summary>
        public void Run()
        {
            _isRuning = true;
            // Запускаем отдельный поток для загрузки и генерации чанков
            Thread myThread = new Thread(_ThreadUpdate) { Name = _nameThread };
            myThread.Start();
        }

        /// <summary>
        /// Запуск в потоке
        /// </summary>
        public void RunInFlow()
        {
            _flagExecution = false;
            // Сигнализируем, что waitHandler в сигнальном состоянии
            _waitHandler.Set();
        }

        /// <summary>
        /// Ждём
        /// </summary>
        public void Waiting()
        {
            ushort sleep = 0;
            while (_isRuning && !_flagExecution)
            {
                if (++sleep == 0) Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Останавливаем
        /// </summary>
        public void Stoping()
        {
            _isRuning = false;
            // Сигнализируем, что waitHandler в сигнальном состоянии
            _waitHandler.Set();
        }

        /// <summary>
        /// Отдельный поток для дополнительного мира
        /// </summary>
        private void _ThreadUpdate()
        {
            while (_isRuning)
            {
                // Ожидаем сигнала
                _waitHandler.WaitOne();
                if (_isRuning)
                {
                    OnDoInFlow();
                }
                _flagExecution = true;
            }
        }

        #region Event

        /// <summary>
        /// Событие, Делай в потоке
        /// </summary>
        public event EventHandler DoInFlow;
        /// <summary>
        /// Событие Делай в потоке
        /// </summary>
        protected void OnDoInFlow() => DoInFlow?.Invoke(this, new EventArgs());

        #endregion
    }
}
