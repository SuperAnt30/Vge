using System;
using System.Threading;

namespace Vge.Realms
{
    /// <summary>
    /// Объект который запускает текстуры, звук, и формирует всё для игры
    /// </summary>
    public class Loading
    {
        public Loading() { }

        /// <summary>
        /// Запустить загрузку в отдельном потоке
        /// </summary>
        public void Starting()
        {
            Thread myThread = new Thread(Loop) { Name = "Loading" };
            myThread.Start();
        }

        /// <summary>
        /// Метод должен работать в отдельном потоке
        /// </summary>
        private void Loop()
        {
            Steps();
            OnFinish();
        }

        /// <summary>
        /// Этот метод как раз и реализует список загрузок
        /// </summary>
        protected virtual void Steps() { }

        /// <summary>
        /// Максимальное количество шагов
        /// </summary>
        public virtual int GetMaxCountSteps() => 0;

        #region Event

        /// <summary>
        /// Событие шаг
        /// </summary>
        public event EventHandler Step;
        /// <summary>
        /// Событие шаг
        /// </summary>
        protected void OnStep() => Step?.Invoke(this, new EventArgs());

        /// <summary>
        /// Событие заканчивать
        /// </summary>
        public event EventHandler Finish;
        /// <summary>
        /// Событие заканчивать
        /// </summary>
        private void OnFinish() => Finish?.Invoke(this, new EventArgs());

        #endregion
    }
}
