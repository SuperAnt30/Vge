using System;
using System.Threading;

namespace Vge
{
    public class Server
    {
        /// <summary>
        /// Запустить сервер в отдельном потоке
        /// </summary>
        public void StartServer()
        {
            Thread myThread = new Thread(ServerLoop);
            myThread.Start();
        }

        /// <summary>
        /// Указывает, запущен сервер или нет. Установите значение false, чтобы инициировать завершение работы. 
        /// </summary>
        private bool serverRunning = true;

        public void Stop() => serverRunning = false;

        /// <summary>
        /// Метод должен работать в отдельном потоке
        /// </summary>
        public void ServerLoop()
        {
            try
            {
                int jj = 0;
                // Рабочий цикл сервера
                while (serverRunning)
                {
                    Thread.Sleep(1);
                    if (++jj > 1000)
                    {
                        jj = 0;
                        OnTick();
                    }
                    //if (++xx > 900) xx = 0;
                    //Thread.Sleep(Mth.Max(1, 50 - (int)cacheTime));
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

        private void Close()
        {
            serverRunning = false;
            OnCloseded();
        }

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
        /// Событие одного такта от сервера
        /// </summary>
        public event EventHandler Tick;
        private void OnTick() => Tick?.Invoke(this, new EventArgs());
    }
}
